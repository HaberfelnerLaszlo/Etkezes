using Etkezes_API.Data;

using Etkezes_Models;
using Etkezes_Models.ViewModels;

using Microsoft.EntityFrameworkCore;

using System.Threading.Tasks;

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
        public async Task<List<Etkezes>> GetAll()
        {
            return await _context.Etkezesek.AsNoTracking().ToListAsync();
        }
        public async Task<List<Etkezes>> GetAllByDatum(DateTime date)
        {
            return await _context.Etkezesek.Include(e => e.User).Where(e => e.Datum == date).AsNoTracking().ToListAsync();
        }
        public async Task<List<EtkezokView>?> GetAllByToday()
        {
            var etkezesek = await _context.Etkezesek.Include(e => e.User).Where(e => e.Datum == DateTime.Today && e.Elfogyasztva == false).AsNoTracking().Select(e => new EtkezokView
            {
                Menu = e.Menu,
                UserId = e.UserId,
                Adag = e.Adag,
                Darab = e.Darab,
                Name = e.User.Name,
            }).ToListAsync();
            return etkezesek;
        }
        public async Task<bool> Create(Etkezes e)
        {
            try
            {
                ErrorMessage=string.Empty;
                if (_context.Etkezesek.Any(et => et.UserId == e.UserId && et.Datum == e.Datum))
                {
                    var etkezes = await _context.Etkezesek.FirstAsync(et => et.UserId == e.UserId && et.Datum == e.Datum);
                    _context.Update<Etkezes>(etkezes);
                    _context.SaveChanges();
                    return true;
                }
                _context.Etkezesek.Add(e);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Hiba lépet fel az etkezés mentésekor: {ex.Message}";
                return false;
            }
        }

        public async Task<Etkezes?> Update(Etkezes et, int id)
        {
            try
            {
                ErrorMessage = string.Empty;
                var exitsEtkezes = await _context.Etkezesek.FindAsync(id);
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
                exitsEtkezes.Elfogyasztva=et.Elfogyasztva;
                exitsEtkezes.Updated=DateTime.Now;
                _context.Update<Etkezes>(exitsEtkezes);
                await _context.SaveChangesAsync();
                return exitsEtkezes;
            }
            catch (Exception ex)
            {
                ErrorMessage=$"Hiba történt az étkezés módosításakor: {ex.Message}";
                return null;
            }
        }
        public async Task DeleteAsync(int id)
        {
            var etkezes = await  _context.Etkezesek.Where(e=>e.Id==id).ExecuteDeleteAsync();
        }

        internal async Task<List<EtkezesView>> GetAllByOsztaly(string osztaly)
        {
            return await _context.Etkezesek.Include(e => e.User).Where(e => e.User.Osztaly == osztaly).AsNoTracking().Select(e => new EtkezesView
            {
                Id = e.Id,
                Menu = e.Menu,
                Datum = e.Datum,
                UserId = e.UserId,
                Adag = e.Adag,
                Darab = e.Darab,
                Elfogyasztva = e.Elfogyasztva,
                Name = e.User.Name,
                Osztaly = e.User.Osztaly
            }).ToListAsync();
        }
        internal async Task<List<EtkezesView>> GetEtkezesByDateByOsztaly(DateTime date, string osztaly)
        {
            return await _context.Etkezesek.Include(e => e.User).Where(e => e.Datum==date && e.User.Osztaly == osztaly).AsNoTracking().Select(e => new EtkezesView
            {
                Id = e.Id,
                Menu = e.Menu,
                Datum = e.Datum,
                UserId = e.UserId,
                Adag = e.Adag,
                Darab = e.Darab,
                Elfogyasztva = e.Elfogyasztva,
                Name = e.User.Name,
                Osztaly = e.User.Osztaly
            }).ToListAsync();
        }

        internal async Task<bool> DeleteMaiEtkezesAsync(long id)
        {
            return await _context.Etkezesek.Where(e => e.UserId == id && e.Datum == DateTime.Today).ExecuteDeleteAsync() >0;
        }

        internal bool Exists(long userId, DateTime datum, out int id)
        {
            var etkezes = _context.Etkezesek.FirstOrDefault(e => e.UserId == userId && e.Datum == datum);
            if (etkezes != null)
            {
                id = etkezes.Id;
                return true;
            }
            id = 0;
            return false;
        }

        internal void IsElfogyasztva(long id)
        {
            var etkezesek = _context.Etkezesek.Where(e => e.UserId == id && e.Datum == DateTime.Today);
            if(etkezesek.Count() > 0)
            {
                foreach (var e in etkezesek)
                {
                    e.Elfogyasztva = true;
                    e.Updated = DateTime.UtcNow;
                }
                _context.SaveChanges();
            }
            //await _context.SaveChangesAsync();
        }
    }
}
