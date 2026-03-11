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
            return await _context.Etkezesek.Where(e => e.Datum == date).AsNoTracking().ToListAsync();
        }
        public async Task<bool> Create(Etkezes e)
        {
            try
            {
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
    }
}
