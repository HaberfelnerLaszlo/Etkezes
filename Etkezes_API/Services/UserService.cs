using Etkezes_API.Data;

using Etkezes_Models;

using Microsoft.EntityFrameworkCore;

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
        public async Task<List<string>> GetAllUsersAsync()
        {
            return await _context.Users.Select(u => u.Name).AsNoTracking().ToListAsync();
        }
        public async Task<User?> GetUserByIdAsync(long id)
        {
            return await _context.Users.FindAsync(id);
        }
        public async Task<List<User>> GetUsersByUpDateAsync(DateTime date)
        {
            return await _context.Users.Where(u => u.Updated > date).AsNoTracking().ToListAsync();
        }
        public async Task<List<User>> GetUsersByOsztalyAsync(string osztaly)
        {
            return await _context.Users.Where(u=>u.Osztaly==osztaly).AsNoTracking().ToListAsync();
        }
        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                ErrorMessage = $"Error creating user: {ex.Message}";
                return false;
            }
        }
        public async Task<bool> UpdateUserAsync(long id, User user)
        {
            try
            {
                var existingUser = await _context.Users.FindAsync(id);
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
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                ErrorMessage = $"Error updating user: {ex.Message}";
                return false;
            }
        }
        public async Task DeleteUserAsync(long id)
        {
            try
            {
                await _context.Users.Where(u => u.Id == id).ExecuteDeleteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting user: {ex.Message}");
                ErrorMessage = $"Error deleting user: {ex.Message}";
            }
        }

    }
}
