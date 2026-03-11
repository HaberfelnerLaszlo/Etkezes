using Etkezes_API.Services;

using Etkezes_Models;
using Etkezes_Models.ViewModels;

using MySqlX.XDevAPI.Common;

namespace Etkezes_API.Endpoints
{
    public static class EtkezesEndpoint
    {
        static readonly MainResponse response = new();
        public static void MapEtkezesEndpoint(this WebApplication app)
        {
            app.MapGet("/etkezesek", async (EtkezesService etkezesService) => await GetAll(etkezesService)).WithName("GetEtkezesAll");
            app.MapGet("/etkezesek/{date}", async (DateTime date, EtkezesService etkezesService) => await GetEtkezesByDate(date, etkezesService)).WithName("GetEtkezesByDate");
            app.MapGet("/etkezesek/osztaly/{osztaly}", async (string osztaly, EtkezesService etkezesService) => await GetEtkezesByOsztaly(osztaly, etkezesService)).WithName("GetEtkezesByOsztaly");
            app.MapGet("/etkezesek/{date}/osztaly/{osztaly}", async (DateTime date, string osztaly, EtkezesService etkezesService) => await GetEtkezesByDateByOsztaly(date, osztaly, etkezesService)).WithName("GetEtkezesByDateByOsztaly");
            app.MapPost("/etkezes",async (Etkezes etkezes, EtkezesService etkezesService) => await AddEtkezes(etkezes, etkezesService));
            app.MapPut("etkezes/{id}",async (int id, Etkezes etkezes, EtkezesService etkezesService) => await UpdateEtkezes(id, etkezes, etkezesService));
            app.MapDelete("/etkezes/{id}",async (int id, EtkezesService etkezesService)=>await DeleteEtkezes(id, etkezesService));
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
                 List<EtkezokView> etkezokViews = new List<EtkezokView>();
                foreach (var item in etkezesek)
                {
                    etkezokViews.Add(new EtkezokView
                    {
                        UserId = item.UserId,
                        Name = item.User.Name,
                        Menu = item.Menu,
                        Adag = item.Adag,
                        Darab = item.Darab
                    });
                }
           response.Data = etkezokViews;
            return await Task.FromResult(Results.Ok(response));
        }
        private static async Task<IResult> AddEtkezes(Etkezes etkezes, EtkezesService service)
        {
            response.Clear();
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
    }
}
