using Etkezes_Ellenor.Data;
using Etkezes_Models;
using FingerPrintService;
using System.Security.Cryptography;


namespace Etkezes_Ellenor.Services
{
    public class LoginUserService(EtkezesDBcontext context, IFPService fPService)
    {
        private readonly EtkezesDBcontext _context = context;
        private readonly IFPService _fPService = fPService;

        public List<LoginUser> GetAllUsers()
        {
            return _context.LoginUsers.ToList();
        }
        public LoginUser? GetUserById(Guid id)
        {
            return _context.LoginUsers.FirstOrDefault(u => u.Id == id);
        }
        public LoginUser GetUserByFpId(int fpId)
        {
            return _context.LoginUsers.FirstOrDefault(u => u.FpId == fpId || u.FpId + 1000 == fpId) ?? new LoginUser();
        }
        public LoginUser GetUserByUserName(string name)
        {
            return _context.LoginUsers.FirstOrDefault(u => u.UserName == name) ?? new LoginUser();
        }
        public bool AddUser(LoginUser user,string password) 
        {
            try
            {
                user.Password = HashPassword(password, user.Id);
                _context.LoginUsers.Add(user); 
                _context.SaveChanges(); 
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }
        public bool ValidateUser(string name, string password)
        {
            var user = GetUserByUserName(name);
            if (user == null)
            {
                return false;
            }
            byte[] hash = HashPassword(password, user.Id);
            return hash.SequenceEqual(user.Password);
        }
        public bool Exists(string name)
        {
            return _context.LoginUsers.Any(u => u.UserName == name);
        }
        public bool HandleSuccessfulIdentification(int fId)
        {
            return _context.LoginUsers.Any(u => u.FpId == fId || u.FpId + 1000 == fId);
        }
        private byte[] HashPassword(string password, Guid userId)
        {
            try { 
                if (string.IsNullOrEmpty(password))
                {
                    throw new ArgumentException("Password cannot be null or empty.");
                }
                byte[] salt = userId.ToByteArray();
                byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                    password: password,
                    salt: salt,
                    iterations: 1000,
                    hashAlgorithm: HashAlgorithmName.SHA256,
                    outputLength: 32
                );
                return hash;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Array.Empty<byte>();
            }
        }
                public async Task<bool> LoginUsersLoad()
        {
            try
            {
                IList<LoginUser> users = _context.LoginUsers.ToList();
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
