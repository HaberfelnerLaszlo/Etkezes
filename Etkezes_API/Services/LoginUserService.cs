using Etkezes_API.Data;
using Etkezes_Models;

using Microsoft.EntityFrameworkCore;

namespace Etkezes_API.Services
{
    public class LoginUserService(EtkezesDbContext dbContext)
    {
        private readonly EtkezesDbContext dbContext = dbContext;
        public string ErrorMessage { get; private set; } = string.Empty;
        public async Task<List<LoginUser>> GetAllLoginUsers()
        {
            return await dbContext.LoginUsers.AsNoTracking().ToListAsync();
        }
        public async Task<LoginUser?> GetLoginUserByIdAsync(Guid id)
        {
            return await dbContext.LoginUsers.FindAsync(id);
        }
        public async Task<List<LoginUser>?> GetLoginUsersByUpDateAsync(DateTime datum)
        {
            return await dbContext.LoginUsers.Where(u => u.UpdatedAt > datum).AsNoTracking().ToListAsync();
        }
        internal async Task<LoginUser?> GetLoginUserByUserNameAsync(string username)
        {
            return await dbContext.LoginUsers.FindAsync(username);
        }
        public async Task<bool> CreateLoginUserAsync(LoginUser loginUser)
        {
            try
            {
                dbContext.LoginUsers.Add(loginUser);
                await dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error creating user: {ex.Message}";
                return false;
            }
        }
        public async Task<bool> UpdateLoginUserAsync(LoginUser loginUser,Guid Id)
        {
            try
            {
                var existingUser = await dbContext.LoginUsers.FindAsync(Id);
                if (existingUser == null)
                {
                    ErrorMessage = "User not found.";
                    return false;
                }
                existingUser.UserName = loginUser.UserName;
                existingUser.Password = loginUser.Password;
                existingUser.UpdatedAt = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating user: {ex.Message}";
                return false;
            }
        }
        public async Task DeleteLoginUserAsync(Guid id)
        {
                await dbContext.LoginUsers.Where(l=>l.Id==id).ExecuteDeleteAsync();
        }

    }
}