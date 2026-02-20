using Etkezes_API.Data;
using Etkezes_API.Services;

using Etkezes_Models;

using MySqlX.XDevAPI.Common;

namespace Etkezes_API.Endpoints
{
    public static class EtkezesEndpoint
    {
        static readonly MainResponse response = new();
        public static void MapEtkezesEndpoint(this WebApplication app)
        {
            app.MapGet("/etkezesek", (EtkezesService etkezesService) => GetAll(etkezesService)).WithName("GetEtkezesAll");
            app.MapGet("/etkezesek/{date}", (DateTime date, EtkezesService etkezesService) => GetEtkezesByDate(date, etkezesService)).WithName("GetEtkezesByDate");
            app.MapPost("/etkezes", (Etkezes etkezes, EtkezesService etkezesService) => AddEtkezes(etkezes, etkezesService)).WithName("AddEtkezes");
            app.MapPut("etkezes/{id}", (int id, Etkezes etkezes, EtkezesService etkezesService) => UpdateEtkezes(id, etkezes, etkezesService)).WithName("UpdateEtkezes");
            app.MapDelete("/etkezes/{id}",(int id, EtkezesService etkezesService)=>DeleteEtkezes(id, etkezesService)).WithName("DeleteEtkezes");
        }
        private static Task<IResult> GetAll(EtkezesService service)
        {
            response.Clear();
            var etkezesek = service.GetAll();
            if (etkezesek == null || etkezesek.Count == 0)
            {
                response.Success = false;
                response.Message = service.ErrorMessage;
                return Task.FromResult(Results.NotFound(response));
            }
            response.Success = true;
            response.Data = etkezesek;
            return Task.FromResult(Results.Ok(response));
        }
        private static Task<IResult> GetEtkezesByDate(DateTime date, EtkezesService service)
        {
            response.Clear();
            var etkezesek = service.GetAllByDatum(date);
            if (etkezesek == null || etkezesek.Count == 0)
            {
                response.Success = false;
                response.Message = service.ErrorMessage;
                return Task.FromResult(Results.NotFound(response));
            }
            response.Success = true;
            response.Data = etkezesek;
            return Task.FromResult(Results.Ok(response));
        }
        private static Task<IResult> AddEtkezes(Etkezes etkezes, EtkezesService service)
        {
            response.Clear();
            if (service.Create(etkezes))
            {
                response.Success = true;
                response.Data = etkezes;
                return Task.FromResult(Results.Ok(response));
            }
            response.Success = false;
            response.Message = service.ErrorMessage;
            return Task.FromResult(Results.BadRequest(response));
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
        private static Task<IResult> DeleteEtkezes(int id, EtkezesService etkezesService)
        {
            response.Clear();
            if (etkezesService.Delete(id))
            {
                response.Success = true;
                return Task.FromResult(Results.Ok(response));
            }
            else
            {
                response.Success = false;
                response.Data = null;
                response.Message = etkezesService.ErrorMessage;
                return Task.FromResult(Results.BadRequest(response));
            }
        }
    }
}
