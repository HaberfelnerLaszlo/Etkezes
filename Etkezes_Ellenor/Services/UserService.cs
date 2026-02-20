using Etkezes_Ellenor.Data;

using Etkezes_Models;

using FingerPrintService;

namespace Etkezes_Ellenor.Services
{
    public class UserService
    {
        private readonly EtkezesDBcontext _context;
        private readonly ILogger<UserService> _logger;
        private readonly FPService _fpService;
        public UserService(EtkezesDBcontext context, ILogger<UserService> logger, FPService fpService)
        {
            _context = context;
            _logger = logger;
            _fpService = fpService;
        }
        public List<User> GetAllUsers()
        {
            return _context.Users.ToList();
        }
        public User? GetUserById(int id)
        {
            return _context.Users.FirstOrDefault(u => u.Id == id);
        }
        public User GetUserByName(string name)
        {
            return _context.Users.FirstOrDefault(u => u.Name == name) ?? new User();
        }
        public List<User> GetUsersByClass(string osztaly)
        {
            return _context.Users.Where(u => u.Osztaly == osztaly).ToList();
        }
        public List<User> GetUsersByClass_NoFP(string osztaly)
        {
            return _context.Users.Where(u => u.Osztaly == osztaly && String.IsNullOrWhiteSpace(u.FingerPrint1)).ToList();
        }
        public bool UserExists(long id)
        {
            return _context.Users.Any(u => u.Id == id);
        }
        public void AddUser(User user)
        {
            try
            {
                user.Created = DateTime.Now;
                _context.Users.Add(user);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user");
            }
        }
        public bool HandleSuccessfulIdentification(int fId)
        {
            return _context.Users.Any(u => u.FpId == fId || u.FpId + 1000 == fId);
        }
        public void UpdateUser(User user)
        {
            try
            {
                user.Updated = DateTime.Now;
                _context.Users.Update(user);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
            }
        }
        public void DeleteUser(long id)
        {
            try
            {
                var user = GetUserById((int)id);
                if (user != null)
                {
                    user.Updated = DateTime.Now;
                    _context.Users.Remove(user);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user");
            }
        }
        public bool UserLoading()
        {
            try
            {
                IList<User> users = GetAllUsers();
                if (users.Count == 0)
                {
                    Console.WriteLine("No users found.");
                    return false;
                }

                foreach (var item in users.OrderBy(u => u.FpId))
                {
                    if (!string.IsNullOrWhiteSpace(item.FingerPrint1))
                    {
                        if(_fpService.AddFingerprint(item.FingerPrint1,item.FpId))
                        {
                            Console.WriteLine($"User {item.Name} loaded successfully.");
                        }
                        else
                        {
                            Console.WriteLine($"Failed to load user {item.Name}.");
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }   
    }
}
