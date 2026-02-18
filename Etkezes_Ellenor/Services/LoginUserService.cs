using Etkezes_Ellenor.Data;

using System.Security.Cryptography;

namespace Etkezes_Ellenor.Services
{
    public class LoginUserService
    {
        private readonly EtkezesDBcontext _context;
        public LoginUserService(EtkezesDBcontext context)
        {
            _context = context;
        }
        public List<LoginUser> GetAllUsers()
        {
            return _context.LoginUsers.ToList();
        }
        public LoginUser? GetUserById(Guid id)
        {
            return _context.LoginUsers.FirstOrDefault(u => u.Id == id);
        }
        public LoginUser GetUserByName(string name)
        {
            return _context.LoginUsers.FirstOrDefault(u => u.Name == name) ?? new LoginUser();
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
            var user = GetUserByName(name);
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
    }
}
