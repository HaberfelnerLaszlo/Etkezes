namespace Etkezes_Ellenor.Data
{
    public class Etkezok
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Menu { get; set; } = string.Empty;
        public string Adag { get; set; } = string.Empty;
        public int Darab { get; set; }
        public bool Elfogyasztva { get; set; }=false;
        public DateTime Updated { get; set; } = DateTime.Now;
   }
}