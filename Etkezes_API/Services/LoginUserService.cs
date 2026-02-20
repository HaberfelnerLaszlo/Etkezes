using Etkezes_API.Data;
using Etkezes_Models;

namespace Etkezes_API.Services
{
    public class LoginUserService(EtkezesDbContext dbContext)
    {
        private readonly EtkezesDbContext dbContext = dbContext;
        public string ErrorMessage { get; private set; } = string.Empty;
        public List<LoginUser> GetAllLoginUsers()
        {
            return dbContext.LoginUsers.ToList();
        }
        public LoginUser? GetLoginUserById(Guid id)
        {
            return dbContext.LoginUsers.Find(id);
        }
        public List<LoginUser>? GetLoginUsersByUpDate(DateTime datum)
        {
            return dbContext.LoginUsers.Where(u => u.UpdatedAt > datum).ToList();
        }
        public bool CreateLoginUser(LoginUser loginUser)
        {
            try
            {
                dbContext.LoginUsers.Add(loginUser);
                dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error creating user: {ex.Message}";
                return false;
            }
        }
        public bool UpdateLoginUser(LoginUser loginUser,Guid Id)
        {
            try
            {
                var existingUser = dbContext.LoginUsers.Find(Id);
                if (existingUser == null)
                {
                    ErrorMessage = "User not found.";
                    return false;
                }
                existingUser.UserName = loginUser.UserName;
                existingUser.Password = loginUser.Password;
                existingUser.UpdatedAt = DateTime.UtcNow;
                dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating user: {ex.Message}";
                return false;
            }
        }
        public bool DeleteLoginUser(Guid id)
        {
            try
            {
                var user = dbContext.LoginUsers.Find(id);
                if (user == null)
                {
                    ErrorMessage = "User not found.";
                    return false;
                }
                dbContext.LoginUsers.Remove(user);
                dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting user: {ex.Message}";
                return false;
            }
        }
    }
}