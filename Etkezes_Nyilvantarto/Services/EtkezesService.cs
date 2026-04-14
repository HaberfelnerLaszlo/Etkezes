using Etkezes_Models;
using Etkezes_Models.ViewModels;

namespace Etkezes_Nyilvantarto.Services
{
    public class EtkezesService(ApiHelper api)
    {
        public event EventHandler<OnMessageEventArgs>? OnMessage;
        public async Task<List<Etkezes>> GetEtkezesek()
        {
            var etkezesek = await api.Get<List<Etkezes>>("/etkezesek");
            if (etkezesek == null) {
                OnMessage?.Invoke(this, new OnMessageEventArgs("Nincsenek étkezések!",104));
                return new List<Etkezes>();
            }
            return etkezesek;
        }
        public async Task<List<EtkezokView>> GetEtkezesekByToday()
        {
            var etkezesek = await api.Get<List<EtkezokView>>($"/maietkezesek");
            if (etkezesek == null)
            {
                OnMessage?.Invoke(this, new OnMessageEventArgs("Nincsenek étkezések a megadott dátumra!", 104));
                return new();
            }
            return etkezesek;
        }

        public async Task<List<EtkezesView>> GetEtkezesekByDate(DateTime date)
        {
            var etkezesek = await api.Get<List<EtkezesView>>($"/etkezesek/{date:yyyy-MM-dd}");
            if (etkezesek == null)
            {
                OnMessage?.Invoke(this, new OnMessageEventArgs("Nincsenek étkezések a megadott dátumra!", 104));
                return new();
            }
            return etkezesek;
        }
        public async Task<List<EtkezesView>> GetEtkezesekByDateByOsztaly(DateTime date, string osztaly)
        {
            var etkezesek = await api.Get<List<EtkezesView>>($"/etkezesek/{date:yyyy-MM-dd}/osztaly/{osztaly}");
            if (etkezesek == null)
            {
                OnMessage?.Invoke(this, new OnMessageEventArgs("Nincsenek étkezések a megadott dátumra!",104));
                return new ();
            }
            return etkezesek;
        }
        public async Task<List<EtkezesView>> GetUsersByEtkezesekDateOsztaly(DateTime date, string osztaly)
        {
            var etkezesek = await api.Get<List<EtkezesView>>($"/etkezesek/{date:yyyy-MM-dd}/{osztaly}");
            if (etkezesek == null)
            {
                OnMessage?.Invoke(this, new OnMessageEventArgs("Nincsenek étkezések a megadott dátumra!",104));
                return new ();
            }
            return etkezesek;
        }
        public async Task<List<EtkezesView>> GetEtkezesekByOsztaly(string osztaly)
        {
            var etkezesek = await api.Get<List<EtkezesView>>($"/etkezesek/osztaly/{osztaly}");
            if (etkezesek == null)
            {
                OnMessage?.Invoke(this, new OnMessageEventArgs("Nincsenek étkezések a megadott osztályra!",104));
                return new List<EtkezesView>();
            }
            return etkezesek;
        }

       public async Task<Etkezes?> GetEtkezes(int id)
        {
            var etkezes = await api.Get<Etkezes>($"/etkezesek/{id}");
            if (etkezes == null) {
                OnMessage?.Invoke(this, new OnMessageEventArgs("Az étkezés nem található!",104));
                return null;
            }
            return etkezes;
        }
        public async Task<Etkezes?> CreateEtkezes(Etkezes etkezes)
        {
            var createdEtkezes = await  api.Post("/etkezes", etkezes);
            if (createdEtkezes == null) {
                OnMessage?.Invoke(this, new OnMessageEventArgs("Az étkezés létrehozása sikertelen!",103));
                return null;
            }
            return createdEtkezes;
        }
        public async Task<List<Etkezes>?> CreateEtkezesek(List<Etkezes> etkezesek)
        {
            try
            {
                var createdEtkezesek = await api.Post("/etkezesek", etkezesek);
                if (createdEtkezesek == null) {
                    OnMessage?.Invoke(this, new OnMessageEventArgs("Az étkezések létrehozása sikertelen!",103));
                    return null;
                }
                if (createdEtkezesek.Count == 0) {
                    OnMessage?.Invoke(this, new OnMessageEventArgs("Az étkezések létrehozása sikertelen!",103));
                    return null;
                }
                if (createdEtkezesek.Count != etkezesek.Count) {
                    OnMessage?.Invoke(this, new OnMessageEventArgs("Nem minden étkezés létrehozása sikerült!",103));
                    createdEtkezesek.ForEach(e => {etkezesek.RemoveAll(e2 => e2.Id == e.Id);});
                    return etkezesek;
                }
                return etkezesek;
            }
            catch (Exception ex)
            {
                OnMessage?.Invoke(this, new OnMessageEventArgs(ex.Message));
                return null;
            }
        }
        public async Task<Etkezes?> UpdateEtkezes(Etkezes etkezes)
        {
            var updatedEtkezes = await api.Put<Etkezes>($"/etkezes/{etkezes.Id}", etkezes);
            if (updatedEtkezes == null) {
                OnMessage?.Invoke(this, new OnMessageEventArgs("Az étkezés frissítése sikertelen!",100));
                return null;
            }
            return updatedEtkezes;
        }
        public async Task<bool> DeleteEtkezes(int id)
        {
            return await api.Delete($"/etkezes/{id}");
        }
        public async Task<bool> DeleteMaiEtkezes(long id)
        {
            return await api.Delete($"/maietkezes/{id}");
        }
    }
    //public class OnMessageEtkezesEventArgs : EventArgs
    //{
    //    public string Message { get; set; }
    //    public OnMessageEtkezesEventArgs(string message)
    //    {
    //        Message = message;
    //    }
    //}
}
