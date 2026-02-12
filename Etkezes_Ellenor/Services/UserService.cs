using Etkezes_Ellenor.Data;

namespace Etkezes_Ellenor.Services
{
    public class UserService
    {
        private readonly EtkezesDBcontext _context;
        public UserService(EtkezesDBcontext context)
        {
            _context = context;
        }
        public List<User> GetAllUsers()
        {
            return _context.Users.ToList();
        }
        public User? GetUserById(int id)
        {
            return _context.Users.FirstOrDefault(u => u.Id == id);
        }
        public User GetUserByName(string name)
        {
            return _context.Users.FirstOrDefault(u => u.UserName == name) ?? new User();
        }
        public void AddUser(User user) 
        {
            _context.Users.Add(user); 
            _context.SaveChanges(); 
        }
    }
}
