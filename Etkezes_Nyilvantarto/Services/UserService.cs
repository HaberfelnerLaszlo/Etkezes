using Etkezes_Models;

using Newtonsoft.Json;

namespace Etkezes_Nyilvantarto.Services
{
    public class UserService(ApiHelper api)
    {
        public event EventHandler<OnMessageEventArgs>? OnMessageUserServiceEvent;
        public async Task<List<User>> GetAll()
        {
            var users =await api.Get<List<User>>("/users");
            if (users == null) 
            {
                OnMessageUserServiceEvent?.Invoke(this, new("Nincs diák az adatbázisban!",304));
                return new(); }
            return users;
        }
        public async Task<List<User>> GetUsersByOsztaly(string osztaly) 
        {
            var users = await api.Get<List<User>>($"/users/{osztaly}");
            if (users == null) 
            {
                OnMessageUserServiceEvent?.Invoke(this, new("Nincs diák az adatbázisban!",304));
                return new();
            }
            return users;
        }
        public async Task<User?> CreateAsync(User user)
        {
            if (user == null) 
            {
                return null;
            }
            return await api.Post<User>("/user", user);
        }
        public async Task<List<User>?> CreateUsersAsync(List<User> users)
        {
            if (users == null || users.Count == 0) 
            {
                return null;
            }
            return await api.Post<List<User>>("/users", users);
        }
        public async Task<User?> Update(User user)
        {
            var resp = await api.Put<User>($"/user/{user.Id}",user);
            if (resp == null)
            {
                OnMessageUserServiceEvent?.Invoke(this, new($"Nem sikerült a frissítés! Hiba: {api.ErrorMessage}",303));
                return null; }
            return resp;
        }
        public async void Delete(User user)
        {
            await api.Delete($"/user/{user.Id}");
        }
    }
}
