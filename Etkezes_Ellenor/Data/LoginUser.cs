namespace Etkezes_Ellenor.Data
{
    public class LoginUser
    {
        public Guid Id { get; set; }  = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public int FpId { get; set; }
        public string Role { get; set; } = "User";
        public string FingerPrint1 { get; set; } = string.Empty;
        public string FingerPrint2 { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public byte[] Password { get; set; } = [];
    }
}
