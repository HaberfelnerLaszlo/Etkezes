using Etkezes_Models;

namespace Etkezes_Nyilvantarto.Services
{
    public class UserService(ApiHelper api)
    {
        public event EventHandler<MessageUserServiceEventArg> OnMessageUserServiceEvent;
        public async Task<List<User>> GetAll()
        {
            var users =await api.Get<List<User>>("/users");
            if (users == null) 
            {
                OnMessageUserServiceEvent.Invoke(this, new("Nincs diák az adatbázisban!"));
                return new(); }
            return users;
        }
        public async Task<List<User>> GetUsersByOsztaly(string osztaly) 
        {
            var users = await api.Get<List<User>>($"/users/{osztaly}");
            if (users == null) 
            {
                OnMessageUserServiceEvent.Invoke(this, new("Nincs diák az adatbázisban!"));
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
        public async Task<User?> Update(User user)
        {
            var resp = await api.Put<User>($"/user/{user.Id}",user);
            if (resp == null)
            {
                OnMessageUserServiceEvent.Invoke(this, new($"Nem sikerült a frissítés! Hiba: {api.ErrorMessage}"));
                return null; }
            return resp;
        }
        public async void Delete(User user)
        {
            await api.Delete($"/user/{user.Id}");
        }
    }
    public class MessageUserServiceEventArg : EventArgs
    {
        public string Message { get; set; }
        public MessageUserServiceEventArg(string message) { this.Message = message; }

    }
}
