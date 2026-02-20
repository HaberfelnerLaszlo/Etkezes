using Etkezes_Models;

using FingerPrintService;

namespace Etkezes_Ellenor.Services
{
    public class DataService(LoginUserService loginService, FPService fPService)
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
            users.Add(new() { Id = 78965412301, Name = "Diák 1", Osztaly = "5.a", Etkezik = true });
            users.Add(new() { Id = 78965412302, Name = "Diák 2", Osztaly = "5.a", Etkezik = true });
            users.Add(new() { Id = 78965412303, Name = "Diák 3", Osztaly = "6.a", Etkezik = false });
            users.Add(new() { Id = 78965412304, Name = "Diák 4", Osztaly = "6.a", Etkezik = true });
            users.Add(new() { Id = 78965412305, Name = "Diák 5", Osztaly = "7.a", Etkezik = false });
            users.Add(new() { Id = 78965412306, Name = "Diák 6", Osztaly = "7.b", Etkezik = true });
            return users.AsQueryable();
        }
        public bool LoginUsersLoad()
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
                    if (fPService.AddFingerprint(item.FingerPrint1, item.FpId))
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
                    if (fPService.AddFingerprint(item.FingerPrint2, item.FpId + 1000))
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
