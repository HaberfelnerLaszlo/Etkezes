using Etkezes_Ellenor.Data;

using Etkezes_Models;
using Microsoft.EntityFrameworkCore;

namespace Etkezes_Ellenor.Services
{
    public class EtkezesService(EtkezesDBcontext etkezesDB, ApiHelper apiHelper)
    {
        public event EventHandler<ErrorEventArgs>? OnError;
        public Etkezok? GetEtkezo(long userId)
        {
            try
            {
                if (etkezesDB.Etkezesek.Any(e => e.UserId == userId && e.Darab>0))
                {
                    var etkezo = etkezesDB.Etkezesek.First(e => e.UserId == userId);
                    //if (etkezo == null)
                    //{
                    //    
                    //    return null;
                    //}
                    return etkezo;
                }
                return null;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new(ex.Message, ex.HResult.ToString()));
                return null;
            }
        }
        public async Task<IQueryable<Etkezok>> GetEtkezesek()
        {
            try
            {
                var etkezesek = etkezesDB.Etkezesek.AsNoTracking().AsQueryable();
                if (etkezesek == null)
                {
                    return new List<Etkezok>().AsQueryable();
                }
                return etkezesek;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new(ex.Message, ex.HResult.ToString()));
                return new List<Etkezok>().AsQueryable();
            }
        }
        public async Task IsElfogyasztva(long userId) 
        {
            try
            {
                etkezesDB.Etkezesek.Find(GetEtkezo(userId)?.Id)?.Elfogyasztva = true;
                etkezesDB.SaveChanges();
                await apiHelper.Get<bool>($"etkezes/{userId}");
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new(ex.Message, ex.HResult.ToString()));
            }
        }
    }
    public class ErrorEventArgs:EventArgs
    {
            public string ErrorMessage { get; set; }
            public string ErrorCode { get; set; }
            public ErrorEventArgs(string errorMessage, string errorCode)
            {
                ErrorMessage = errorMessage;
                ErrorCode = errorCode;
            }
    }
}
