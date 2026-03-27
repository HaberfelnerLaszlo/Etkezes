using Etkezes_Ellenor.Data;

using Etkezes_Models;

namespace Etkezes_Ellenor.Services
{
    public class EtkezesService(EtkezesDBcontext etkezesDB)
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
