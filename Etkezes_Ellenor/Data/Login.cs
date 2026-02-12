namespace Etkezes_Ellenor.Data
{
    public class Login
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
