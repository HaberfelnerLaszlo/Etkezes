using Etkezes_API.Data;

using Etkezes_Models;

namespace Etkezes_API.Services
{
    public class UserService
    {
        public UserService(EtkezesDbContext context)
        {
            _context = context;
        }

        private readonly EtkezesDbContext _context;
        public string ErrorMessage { get; private set; } = string.Empty;
        public List<string> GetAllUsers()
        {
            return _context.Users.Select(u => u.Name).ToList();
        }
        public User GetUserById(long id)
        {
            return _context.Users.FirstOrDefault(u => u.Id == id) ?? new User();
        }
        public List<User> GetUsersByUpDate(DateTime date)
        {
            return _context.Users.Where(u => u.Updated > date).ToList();
        }
        public bool CreateUser(User user)
        {
            try
            {
                _context.Users.Add(user);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                ErrorMessage = $"Error creating user: {ex.Message}";
                return false;
            }
        }
        public bool UpdateUser(long id, User user)
        {
            try
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.Id == id);
                if (existingUser == null)
                {
                    ErrorMessage = "User not found.";
                    return false;
                }
                existingUser.Name = user.Name;
                existingUser.FpId = user.FpId;
                existingUser.FingerPrint1 = user.FingerPrint1;
                existingUser.FingerPrint2 = user.FingerPrint2;
                existingUser.Osztaly = user.Osztaly;
                existingUser.Etkezik = user.Etkezik;
                existingUser.Updated = DateTime.Now;
                existingUser.Uploaded = user.Uploaded;
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                ErrorMessage = $"Error updating user: {ex.Message}";
                return false;
            }
        }
        public bool DeleteUser(long id)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == id);
                if (user == null)
                {
                    ErrorMessage = "User not found.";
                    return false;
                }
                _context.Users.Remove(user);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting user: {ex.Message}");
                ErrorMessage = $"Error deleting user: {ex.Message}";
                return false;
            }
        }

    }
}
