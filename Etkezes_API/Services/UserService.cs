using Etkezes_API.Data;

using Etkezes_Models;

using Microsoft.EntityFrameworkCore;

namespace Etkezes_API.Services
{
    public class UserService(EtkezesDbContext context, ILogger<UserService> logger)
    {
        public string ErrorMessage { get; private set; } = string.Empty;
        public async Task<List<string>> GetAllUsersAsync()
        {
            return await context.Users.Select(u => u.Name).AsNoTracking().ToListAsync();
        }
        public async Task<User?> GetUserByIdAsync(long id)
        {
            return await context.Users.FindAsync(id);
        }
        public async Task<List<User>> GetUsersByUpDateAsync(DateTime date)
        {
            return await context.Users.Where(u => u.Updated > date).AsNoTracking().ToListAsync();
        }
        public async Task<List<User>> GetUsersByOsztalyAsync(string osztaly)
        {
            try
            {
                var users = await context.Users.Where(u => u.Osztaly == osztaly).AsNoTracking().ToListAsync();
                return users;
                //return await context.Users.Where(u=>u.Osztaly==osztaly).AsNoTracking().ToListAsync();

            }
            catch (Exception e)
            {
                logger.LogError(e, $"Error fetching users by osztaly: {osztaly}");
                throw;
            }
        }
        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {
                context.Users.Add(user);
                await context.SaveChangesAsync();
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
                var existingUser = await context.Users.FindAsync(id);
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
                await context.SaveChangesAsync();
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
                await context.Users.Where(u => u.Id == id).ExecuteDeleteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting user: {ex.Message}");
                ErrorMessage = $"Error deleting user: {ex.Message}";
            }
        }

    }
}
