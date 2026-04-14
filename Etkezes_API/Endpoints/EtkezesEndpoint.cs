using Etkezes_API.Services;

using Etkezes_Models;
using Etkezes_Models.ViewModels;

namespace Etkezes_API.Endpoints
{
    public static class EtkezesEndpoint
    {
        static readonly MainResponse response = new();
        public static void MapEtkezesEndpoint(this WebApplication app)
        {
            app.MapGet("/etkezesek", async (EtkezesService etkezesService) => await GetAll(etkezesService));
            app.MapGet("/etkezes/{id}", async (long id, EtkezesService etkezesService) => await GetEtkezesById(id, etkezesService));
            app.MapGet("/maietkezesek", async (EtkezesService etkezesService) => await GetEtkezesByToday(etkezesService));
            app.MapGet("/etkezesek/{date}", async (DateTime date, EtkezesService etkezesService) => await GetEtkezesByDate(date, etkezesService));
            app.MapGet("/etkezesek/osztaly/{osztaly}", async (string osztaly, EtkezesService etkezesService) => await GetEtkezesByOsztaly(osztaly, etkezesService));
            app.MapGet("/etkezesek/{date}/osztaly/{osztaly}", async (DateTime date, string osztaly, EtkezesService etkezesService) => await GetEtkezesByDateByOsztaly(date, osztaly, etkezesService));
            app.MapGet("/etkezesek/{date}/{osztaly}", async (DateTime date, string osztaly, EtkezesService etkezesService, UserService userService) => await GetUsersByEtkezesDateOsztaly(date, osztaly, etkezesService, userService));
            app.MapPost("/etkezes",async (Etkezes etkezes, EtkezesService etkezesService,UserService userService) => await AddEtkezes(etkezes, etkezesService, userService));
            app.MapPost("/etkezesek",async (List<Etkezes> etkezesek, EtkezesService etkezesService,UserService userService) => await AddEtkezesek(etkezesek, etkezesService, userService));
            app.MapPut("etkezes/{id}",async (int id, Etkezes etkezes, EtkezesService etkezesService) => await UpdateEtkezes(id, etkezes, etkezesService));
            app.MapDelete("/etkezes/{id}",async (int id, EtkezesService etkezesService)=>await DeleteEtkezes(id, etkezesService));
            app.MapDelete("/maietkezes/{id}",async (long id, EtkezesService etkezesService)=>await DeleteMaiEtkezes(id, etkezesService));
        }
        private static async Task<IResult> GetEtkezesById(long id, EtkezesService etkezesService)
        {
            try
            {
                response.Clear();
                etkezesService.IsElfogyasztva(id);
                response.Success = true;
                response.Data = true;
                return await Task.FromResult(Results.Ok(response));
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Hiba történt az étkezés lekérése közben: {ex.Message}";
                return await Task.FromResult(Results.BadRequest(response));
            }
        }
        private static async Task<IResult> GetEtkezesByToday(EtkezesService etkezesService)
        {
            response.Clear();
            var etkezesek = await etkezesService.GetAllByToday();
            if (etkezesek == null || etkezesek.Count == 0)
            {
                response.Success = false;
                response.Message = etkezesService.ErrorMessage;
                return await Task.FromResult(Results.NotFound(response));
            }
            response.Success = true;
            response.Data = etkezesek;
            return await Task.FromResult(Results.Ok(response));
        }

        private static async Task<IResult> GetUsersByEtkezesDateOsztaly(DateTime date, string osztaly, EtkezesService etkezesService, UserService userService)
        {
            response.Clear();
            List<EtkezesView> etkezokViews = new List<EtkezesView>();
            var users = await userService.GetUsersByOsztalyAsync(osztaly);
            var etkezesek = await etkezesService.GetEtkezesByDateByOsztaly(date, osztaly);
            foreach (var user in users)
            {
                if (etkezesek.Any(e => e.UserId == user.Id && e.Datum == date))
                {
                   etkezokViews.Add(etkezesek.Find(e=>e.UserId==user.Id && e.Datum == date)!);
                }
                else
                {
                    etkezokViews.Add(new EtkezesView
                    {
                        UserId = user.Id,
                        Name = user.Name,
                        Osztaly = user.Osztaly,
                        Datum = date,
                        Menu = string.Empty,
                        Adag = string.Empty,
                        Darab = 0 ,
                        Elfogyasztva = false
                    });
                }
            }
            if (etkezokViews == null || etkezokViews.Count == 0)
            {
                response.Success = false;
                response.Message = etkezesService.ErrorMessage;
                return await Task.FromResult(Results.NotFound(response));
            }
            response.Success = true;
            response.Data = etkezokViews;
            return await Task.FromResult(Results.Ok(response));
        }

        private static async Task<IResult> GetEtkezesByOsztaly(string osztaly, EtkezesService etkezesService)
        {
            response.Clear();
             var etkezesek = await etkezesService.GetAllByOsztaly(osztaly);
            if (etkezesek == null || etkezesek.Count == 0)
            {
                response.Success = false;
                response.Message = etkezesService.ErrorMessage;
                return await Task.FromResult(Results.NotFound(response));
            }
            response.Success = true;
            response.Data = etkezesek;
            return await Task.FromResult(Results.Ok(response));
        }

        private static async Task<IResult> GetEtkezesByDateByOsztaly(DateTime date, string osztaly, EtkezesService etkezesService)
        {
            response.Clear();
             var etkezesek = await etkezesService.GetEtkezesByDateByOsztaly(date, osztaly);
            if (etkezesek == null || etkezesek.Count == 0)
            {
                response.Success = false;
                response.Message = etkezesService.ErrorMessage;
                return await Task.FromResult(Results.NotFound(response));
            }
            response.Success = true;
            response.Data = etkezesek;
            return await Task.FromResult(Results.Ok(response));
        }
        private static async Task<IResult> GetAll(EtkezesService service)
        {
            response.Clear();
            var etkezesek = await service.GetAll();
            if (etkezesek == null || etkezesek.Count == 0)
            {
                response.Success = false;
                response.Message = service.ErrorMessage;
                return  await Task.FromResult(Results.NotFound(response));
            }
            response.Success = true;
            response.Data = etkezesek;
            return await Task.FromResult(Results.Ok(response));
        }
        private static async Task<IResult> GetEtkezesByDate(DateTime date, EtkezesService service)
        {
            response.Clear();
            var etkezesek = await  service.GetAllByDatum(date);
            if (etkezesek == null || etkezesek.Count == 0)
            {
                response.Success = false;
                response.Message = service.ErrorMessage;
                return await Task.FromResult(Results.NotFound(response));
            }
            response.Success = true;
                 List<EtkezesView> etkezokViews = new List<EtkezesView>();
                foreach (var item in etkezesek)
                {
                    etkezokViews.Add(new EtkezesView
                    {
                        UserId = item.UserId,
                        Name = item.User.Name,
                        Menu = item.Menu,
                        Adag = item.Adag,
                        Darab = item.Darab,
                        Osztaly = item.User.Osztaly,
                        Datum = item.Datum,
                        Id = item.Id,
                        Elfogyasztva = item.Elfogyasztva
                    });
                }
           response.Data = etkezokViews;
            return await Task.FromResult(Results.Ok(response));
        }
        private static async Task<IResult> AddEtkezes(Etkezes etkezes, EtkezesService service, UserService userService)
        {
            try
            {
                response.Clear();
                var user = userService.GetUserByIdAsync(etkezes.UserId);
                if (user == null)
                {
                    response.Success = false;
                    response.Message = $"Nincs ilyen id-val rendelkező felhasználó: {etkezes.UserId}";
                    return await Task.FromResult(Results.BadRequest(response));
                }
                etkezes.User = user;

                if (await service.Create(etkezes))
                {
                    response.Success = true;
                    response.Data = etkezes;
                    return await Task.FromResult(Results.Ok(response));
                }
                response.Success = false;
                response.Message = service.ErrorMessage;
                return await Task.FromResult(Results.BadRequest(response));
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Hiba történt az étkezés hozzáadása közben: {ex.Message}";
                return await Task.FromResult(Results.BadRequest(response));
            }
        }
        private static async Task<IResult> AddEtkezesek(List<Etkezes> etkezesek, EtkezesService etkezesService, UserService userService)
        {
            response.Clear();
            List<Etkezes> addedEtkezesek = new List<Etkezes>();
            foreach (var etkezes in etkezesek)
            {
                var user = userService.GetUserByIdAsync(etkezes.UserId);
                if (user == null)
                {
                    response.Message = $"Nincs ilyen id-val rendelkező felhasználó: {etkezes.UserId}\n ";
                    continue;
                }
                user.Etkezik = true;
                etkezes.User = user;
                if (etkezesService.Exists(etkezes.UserId, etkezes.Datum, out int id))
                {
                    await etkezesService.Update(etkezes, id);
                    addedEtkezesek.Add(etkezes);
                }
                if (await etkezesService.Create(etkezes))
                {
                    addedEtkezesek.Add(etkezes);
                }
                if(string.IsNullOrEmpty(etkezesService.ErrorMessage)) continue;
                response.Message += $"Hiba: {etkezesService.ErrorMessage} \n";

            }
            if (addedEtkezesek.Count == etkezesek.Count)
            {
                response.Success = true;
                response.Data = addedEtkezesek;
                return await Task.FromResult(Results.Ok(response));
            }
            else
            {
                response.Success = false;
                response.Message += "Néhány étkezés hozzáadása nem sikerült!";
                response.Data = etkezesek.Where(e => !addedEtkezesek.Contains(e)).ToList();
                return await Task.FromResult(Results.BadRequest(response));
            }
        }

        private static Task<IResult> UpdateEtkezes(int id, Etkezes et, EtkezesService service)
        {
            response.Clear();
            if (et == null) return Task.FromResult(Results.BadRequest("A köldött étkezes null érték volt!"));
            var etkezes = service.Update(et,id);
            if (etkezes != null)
            {
                response.Success = true;
                response.Data = etkezes;
                return Task.FromResult(Results.Ok(response));
            }
            else
            {
                response.Success = false;
                response.Data = null;
                response.Message = service.ErrorMessage;
                return Task.FromResult(Results.BadRequest(response));
            }
        }
        private static async Task<IResult> DeleteEtkezes(int id, EtkezesService etkezesService)
        {
            await etkezesService.DeleteAsync(id);
            return await Task.FromResult(Results.NoContent());
        }
        private static async Task<IResult> DeleteMaiEtkezes(long id, EtkezesService etkezesService)
        {
            if (await etkezesService.DeleteMaiEtkezesAsync(id)) return await Task.FromResult(Results.NoContent());
            return await Task.FromResult(Results.BadRequest());
        }

    }
}
