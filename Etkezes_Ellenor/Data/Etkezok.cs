namespace Etkezes_Ellenor.Data
{
    public class Etkezok
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Menu { get; set; } = string.Empty;
        public string Adag { get; set; } = string.Empty;
        public int Darab { get; set; }
        public DateTime Updated { get; set; } = DateTime.Now;
   }
}