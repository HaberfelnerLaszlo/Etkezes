using Etkezes_Models;
using Etkezes_Models.ViewModels;

namespace Etkezes_Nyilvantarto.Services
{
    public class EtkezesService(ApiHelper api)
    {
        public event EventHandler<OnMessageEtkezesEventArgs> OnMessage; 
        public async Task<List<Etkezes>> GetEtkezesek()
        {
            var etkezesek = await api.Get<List<Etkezes>>("/etkezesek");
            if (etkezesek == null) {
                OnMessage?.Invoke(this, new OnMessageEtkezesEventArgs("Nincsenek étkezések!"));
                return new List<Etkezes>();
            }
            return etkezesek;
        }
        public async Task<List<EtkezesView>> GetEtkezesekByDate(DateTime date)
        {
            var etkezesek = await api.Get<List<EtkezesView>>($"/etkezesek/{date:yyyy-MM-dd}");
            if (etkezesek == null)
            {
                OnMessage?.Invoke(this, new OnMessageEtkezesEventArgs("Nincsenek étkezések a megadott dátumra!"));
                return new();
            }
            return etkezesek;
        }
        public async Task<List<EtkezesView>> GetEtkezesekByDateByOsztaly(DateTime date, string osztaly)
        {
            var etkezesek = await api.Get<List<EtkezesView>>($"/etkezesek/{date:yyyy-MM-dd}/osztaly/{osztaly}");
            if (etkezesek == null)
            {
                OnMessage?.Invoke(this, new OnMessageEtkezesEventArgs("Nincsenek étkezések a megadott dátumra!"));
                return new ();
            }
            return etkezesek;
        }
        public async Task<List<EtkezesView>> GetUsersByEtkezesekDateOsztaly(DateTime date, string osztaly)
        {
            var etkezesek = await api.Get<List<EtkezesView>>($"/etkezesek/{date:yyyy-MM-dd}/{osztaly}");
            if (etkezesek == null)
            {
                OnMessage?.Invoke(this, new OnMessageEtkezesEventArgs("Nincsenek étkezések a megadott dátumra!"));
                return new ();
            }
            return etkezesek;
        }
        public async Task<List<EtkezesView>> GetEtkezesekByOsztaly(string osztaly)
        {
            var etkezesek = await api.Get<List<EtkezesView>>($"/etkezesek/osztaly/{osztaly}");
            if (etkezesek == null)
            {
                OnMessage?.Invoke(this, new OnMessageEtkezesEventArgs("Nincsenek étkezések a megadott osztályra!"));
                return new List<EtkezesView>();
            }
            return etkezesek;
        }

       public async Task<Etkezes?> GetEtkezes(int id)
        {
            var etkezes = await api.Get<Etkezes>($"/etkezesek/{id}");
            if (etkezes == null) {
                OnMessage?.Invoke(this, new OnMessageEtkezesEventArgs("Az étkezés nem található!"));
                return null;
            }
            return etkezes;
        }
        public async Task<Etkezes?> CreateEtkezes(Etkezes etkezes)
        {
            var createdEtkezes = await  api.Post("/etkezes", etkezes);
            if (createdEtkezes == null) {
                OnMessage?.Invoke(this, new OnMessageEtkezesEventArgs("Az étkezés létrehozása sikertelen!"));
                return null;
            }
            return createdEtkezes;
        }
        public async Task<List<Etkezes>?> CreateEtkezesek(List<Etkezes> etkezesek)
        {
            try
            {
                foreach (var etkezes in etkezesek)
                {
                    var createdEtkezes = await api.Post("/etkezes", etkezes);
                    if (createdEtkezes == null) {
                        OnMessage?.Invoke(this, new OnMessageEtkezesEventArgs("Az étkezés létrehozása sikertelen!"));
                        return null;
                    }
                }
                return etkezesek;
            }
            catch (Exception ex)
            {
                OnMessage.Invoke(this, new OnMessageEtkezesEventArgs(ex.Message));
                return null;
            }
        }
        public async Task<Etkezes?> UpdateEtkezes(Etkezes etkezes)
        {
            var updatedEtkezes = await api.Put<Etkezes>($"/etkezes/{etkezes.Id}", etkezes);
            if (updatedEtkezes == null) {
                OnMessage?.Invoke(this, new OnMessageEtkezesEventArgs("Az étkezés frissítése sikertelen!"));
                return null;
            }
            return updatedEtkezes;
        }
        public async Task<bool> DeleteEtkezes(int id)
        {
            return await api.Delete($"/etkezes/{id}");
        }
    }
    public class OnMessageEtkezesEventArgs : EventArgs
    {
        public string Message { get; set; }
        public OnMessageEtkezesEventArgs(string message)
        {
            Message = message;
        }
    }
}
