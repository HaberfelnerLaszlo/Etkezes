using Etkezes_Models;

using System.Security.Cryptography;

namespace Etkezes_Nyilvantarto.Services
{
    public class LoginUserService   :IDisposable
    {
        private readonly ApiHelper api;
        public event EventHandler<OnMessageEventArgs>? OnMessage;
        public LoginUserService(ApiHelper api)
        {
            this.api = api;
            api.OnErrorMessage += OnErrorMessageChanged!;
        }

        private void OnErrorMessageChanged(object sender, OnMessageEventArgs arg)
        {
            OnMessage?.Invoke(this, arg);
        }

        public async Task<LoginUser?> ValidateUser(string username, string password)
        {
            var user =await api.Get<LoginUser>($"/loginuser/valid/{username}");
            if (user == null)
            {
                return null;
            }
            if (!string.IsNullOrEmpty(user.UserName))
            {
                if (!string.IsNullOrEmpty(password))
                {
                    byte[] hash = HashPassword(password, user.Id);
                    if (hash.SequenceEqual(user.Password)) return user;
                }
            }
            return null;
        }
        public byte[] HashPassword(string password, Guid userId)
        {
            try
            {
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
                OnMessage?.Invoke(this, new OnMessageEventArgs($"Error hashing password: {ex.Message}", 101));
                return Array.Empty<byte>();
            }
        }
        public async Task<List<LoginUser>> GetAllUsers()
        {
            try
            {
                var users = await api.Get<List<LoginUser>>("/loginusers");
                return users ?? new List<LoginUser>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching users: {ex.Message}");
                OnMessage?.Invoke(this, new OnMessageEventArgs($"Error fetching users: {ex.Message}", 102));
                return new List<LoginUser>();
            }
        }
        public async Task<bool> CreateUser(string username, string password)
        {
            try
            {
                var newUser = new LoginUser
                {
                    Id = Guid.NewGuid(),
                    UserName = username,
                    Password = HashPassword(password, Guid.NewGuid())
                };
                var result = await api.Post("/loginuser", newUser);
                return result != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                OnMessage?.Invoke(this, new OnMessageEventArgs($"Error creating user: {ex.Message}", 103));
                return false;
            }
        }
        public async Task<bool> UpdateUser(LoginUser updatedUser)
        {
            try
            {
                var result = await api.Put($"/loginuser/{updatedUser.Id}", updatedUser);
                return result != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                OnMessage?.Invoke(this, new OnMessageEventArgs($"Error updating user: {ex.Message}", 105));
                return false;
            }
        }
        public async Task<bool> DeleteUser(Guid userId)
        {
            try
            {
                var result = await api.Delete($"/loginuser/{userId}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting user: {ex.Message}");
                OnMessage?.Invoke(this, new OnMessageEventArgs($"Error deleting user: {ex.Message}", 104));
                return false;
            }
        }
        public void Dispose()
        {
            api.OnErrorMessage -= OnErrorMessageChanged!;
        }
    }
    public class MessageEventArg:EventArgs
    {
        public string Message { get; set; }
        public MessageEventArg(string message)
        {
            Message = message;
        }
    }
}
