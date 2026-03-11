using Etkezes_Models;

using FingerPrintService;

namespace Etkezes_Ellenor.Services
{
    public class DataService(LoginUserService loginService, IFPService fPService)
    {
        private User _user = new();
        private LoginUser _loginUser = new();
        public User GetUser() => _user;
        public void SetUser(User user) => _user = user;
        public LoginUser GetLoginUser() => _loginUser;
        public void SetLoginUser(LoginUser loginUser) => _loginUser = loginUser;
        public IQueryable<User> GetUsers()
        {
            IList<User> users = [];
            users.Add(new() { Id = 78965412311, Name = "Diák 11", Osztaly = "5.b", Etkezik = true });
            users.Add(new() { Id = 78965412312, Name = "Diák 12", Osztaly = "5.b", Etkezik = true });
            users.Add(new() { Id = 78965412313, Name = "Diák 13", Osztaly = "6.b", Etkezik = false });
            users.Add(new() { Id = 78965412314, Name = "Diák 14", Osztaly = "6.b", Etkezik = true });
            users.Add(new() { Id = 78965412315, Name = "Diák 15", Osztaly = "7.b", Etkezik = false });
            users.Add(new() { Id = 78965412316, Name = "Diák 16", Osztaly = "7.c", Etkezik = true });
            return users.AsQueryable();
        }
        public async Task<bool> LoginUsersLoad()
        {
            try
            {
                IList<LoginUser> users = loginService.GetAllUsers();
                if (users.Count == 0)
                {
                    Console.WriteLine("No users found.");
                    return false;
                }
                foreach (var item in users)
                {
                    if (await fPService.AddFingerprintAsync(item.FingerPrint1, item.FpId))
                    {
                        Console.WriteLine($"Fingerprint for user {item.Name} added successfully.");

                    }
                    else
                    {
                        Console.WriteLine($"Failed to add fingerprint for user {item.Name}.");
                    }
                    if (string.IsNullOrEmpty(item.FingerPrint2))
                    {
                        Console.WriteLine($"No second fingerprint for user {item.Name}.");
                        continue;
                    }
                    if (await fPService.AddFingerprintAsync(item.FingerPrint2, item.FpId + 1000))
                    {
                        Console.WriteLine($"Second fingerprint for user {item.Name} added successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to add second fingerprint for user {item.Name}.");
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
