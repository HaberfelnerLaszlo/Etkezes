using Etkezes_Ellenor.Data;

using Etkezes_Models;
using Etkezes_Models.ViewModels;

using FingerPrintService;

using Microsoft.EntityFrameworkCore;

namespace Etkezes_Ellenor.Services
{
    public class UserService
    {
        private readonly EtkezesDBcontext _context;
        private readonly ILogger<UserService> _logger;
        private readonly IFPService _fpService;
        private readonly ApiHelper _api;
        public UserService(EtkezesDBcontext context, ILogger<UserService> logger, IFPService fpService, ApiHelper api)
        {
            _context = context;
            _logger = logger;
            _fpService = fpService;
            _api = api;
        }
        public List<User> GetAllUsers()
        {
            return _context.Users.ToList();
        }
        public User? GetUserById(int id)
        {
            return _context.Users.FirstOrDefault(u => u.Id == id);
        }
        public User? GetUserByFId(int fid)
        {
            return _context.Users.FirstOrDefault(u => u.FpId == fid);
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
        public async Task<List<FingerPrintData>> GetAllUserFPTmp()
        {
            var users = await _context.Users.Where(u => u.FpId > 0).AsNoTracking().ToListAsync();
            List<FingerPrintData> result = new List<FingerPrintData>();
            users.ForEach(u => { result.Add(new FingerPrintData() { FpId = u.FpId, FingerTemplate1 = u.FingerPrint1, FingerTemplate2 = u.FingerPrint2 }); });
            return result;
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
        public async Task<bool> UpdateUser(User user)
        {
            try
            {
                user.Updated = DateTime.Now;
                _context.Users.Update(user);
                if (_context.SaveChanges() > 0)
                {
                    var resp = await _api.Put<User>($"/user/{user.Id}", user);
                    return true;
                }
                else return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                return false;
            }
        }
        public async Task<bool> DeleteUser(long id)
        {
            try
            {
                var user = GetUserById((int)id);
                if (user != null)
                {
                    _context.Users.Remove(user);
                    if (_context.SaveChanges() > 0)
                    {
                        await _api.Delete($"/user/{user.Id}");
                        return true;
                    }
                    else return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user");
                return false;
            }
        }
        public async Task<bool> UserLoading()
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
                        if(await _fpService.AddFingerprintAsync(item.FingerPrint1,item.FpId))
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
