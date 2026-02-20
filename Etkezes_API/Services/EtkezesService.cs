using Etkezes_API.Data;

namespace Etkezes_API.Services
{
    public class EtkezesService
    {
        private readonly EtkezesDbContext _context;
        public string ErrorMessage { get; set; } = string.Empty;

        public EtkezesService(EtkezesDbContext context)
        {
            _context = context;
        }
        public List<Etkezes> GetAll()
        {
            return _context.Etkezesek.ToList();
        }
        public List<Etkezes> GetAllByDatum(DateTime date)
        {
            return _context.Etkezesek.Where(e => e.Datum == date).ToList();
        }
        public bool Create(Etkezes e)
        {
            try
            {
                _context.Etkezesek.Add(e);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Hiba lépet fel az etkezés mentésekor: {ex.Message}";
                return false;
            }
        }
        public Etkezes? Update(Etkezes et, int id)
        {
            try
            {
                var exitsEtkezes = _context.Etkezesek.FirstOrDefault(e=>e.Id == id);
                if (exitsEtkezes == null)
                {
                    ErrorMessage = "Nem található az adtbázisban";
                    return null;
                }
                exitsEtkezes.Menu = et.Menu;
                exitsEtkezes.Datum = et.Datum;
                exitsEtkezes.UserId = et.UserId;
                exitsEtkezes.Adag=et.Adag;
                exitsEtkezes.Darab=et.Darab;
                exitsEtkezes.Updated=DateTime.Now;
                _context.Update<Etkezes>(exitsEtkezes);
                _context.SaveChanges();
                return exitsEtkezes;
            }
            catch (Exception ex)
            {
                ErrorMessage=$"Hiba történt az étkezés módosításakor: {ex.Message}";
                return null;
            }
        }
        public bool Delete(int id)
        {
            var etkezes = _context.Etkezesek.FirstOrDefault(e=> e.Id == id);
            if (etkezes == null)
            {
                ErrorMessage = "Nem található az adatbázisban.";
                return false;
            }
            _context.Etkezesek.Remove(etkezes);
            _context.SaveChanges();
            return true;
        }
    }
}
