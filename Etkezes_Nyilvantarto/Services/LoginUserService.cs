using Etkezes_Models;

using System.Security.Cryptography;

namespace Etkezes_Nyilvantarto.Services
{
    public class LoginUserService   :IDisposable
    {
        private readonly ApiHelper api;

        public LoginUserService(ApiHelper api)
        {
            this.api = api;
            api.OnErrorMessage += OnErrorMessageChanged!;
        }

        private void OnErrorMessageChanged(object sender, ErrorMessageEventArg arg)
        {
            var err = arg.Message;
        }

        public async Task<bool> ValidateUser(string username, string password)
        {
            var user =await api.Get<LoginUser>($"/loginuser/valid/{username}");
            if (user == null)
            {
                return false;
            }
            if (!string.IsNullOrEmpty(user.UserName))
            {
                if (!string.IsNullOrEmpty(password))
                {
                    byte[] hash = HashPassword(password, user.Id);
                    return hash.SequenceEqual(user.Password);
                }
            }
            return false;
        }
        private byte[] HashPassword(string password, Guid userId)
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
                return Array.Empty<byte>();
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
