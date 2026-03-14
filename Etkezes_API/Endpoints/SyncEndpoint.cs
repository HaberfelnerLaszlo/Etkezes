using Etkezes_API.Services;

using Etkezes_Models;

namespace Etkezes_API.Endpoints
{
    public static class SyncEndpoint
    {
        private static MainResponse response = new();
        public static void MapSyncEndpoints(this WebApplication app)
        {
            app.MapGet("/sync/dates", (SyncService syncService) => GetSyncDates(syncService));
            app.MapGet("/sync/loginusers/{syncDate}", (DateTime syncDate, SyncService syncService) => SyncLoginUsers(syncDate, syncService));
            app.MapGet("/sync/users/{syncDate}", (DateTime syncDate, SyncService syncService) => SyncUsers(syncDate, syncService));
            app.MapGet("/sync/etkezesek/{syncDate}", (DateTime syncDate, SyncService syncService) => SyncEtkezesek(syncDate, syncService));
            app.MapPost("/sync/loginusers", (List<LoginUser> loginUsers, SyncService syncService) => SyncLoginUsersToServer(loginUsers, syncService));
            app.MapPost("/sync/users", (List<User> users, SyncService syncService) => SyncUsersToServer(users, syncService));
            app.MapPost("/sync/etkezesek", (List<Etkezes> etkezesek, SyncService syncService) => SyncEtkezesToServer(etkezesek, syncService));
        }

        private static async Task<IResult> SyncEtkezesToServer(List<Etkezes> etkezesek, SyncService syncService)
        {
            response.Clear();
            var res= await syncService.SyncEtkezesToServer(etkezesek);
            if (res != null)
            {
                response.Success = true;
                response.Data = res;
                return Results.Ok(response);
            }
            response.Success = false;
            response.Message = syncService.ErrorMessage;
            return Results.BadRequest(response);
        }

        private static async Task<IResult> SyncUsersToServer(List<User> users, SyncService syncService)
        {
            response.Clear();
            var res= await syncService.SyncUserToServer(users);
            if (res != null)
            {
                response.Success = true;
                response.Data = res;
                return Results.Ok(response);
            }
            response.Success = false;
            response.Message = syncService.ErrorMessage;
            return Results.BadRequest(response);
        }

        private static async Task<IResult> SyncLoginUsersToServer(List<LoginUser> loginUsers, SyncService syncService)
        {
            response.Clear();
            var res= await syncService.SyncLoginUserToServer(loginUsers);
            if (res != null)
            {
                response.Success = true;
                response.Data = res;
                return Results.Ok(response);
            }
            response.Success = false;
            response.Message = syncService.ErrorMessage;
            return Results.BadRequest(response);
        }

        private static async Task<IResult> SyncEtkezesek(DateTime syncDate, SyncService syncService)
        {
            response.Clear();
            var res= await syncService.SyncEtkezes(syncDate);
            if (res != null)
            {
                response.Success = true;
                response.Data = res;
                return Results.Ok(response);
            }
            response.Success = false;
            response.Message = syncService.ErrorMessage;
            return Results.BadRequest(response);
        }

        private static async Task<IResult> SyncUsers(DateTime syncDate, SyncService syncService)
        {
            response.Clear();
            var res= await syncService.SyncUser(syncDate);
            if (res != null)
            {
                response.Success = true;
                response.Data = res;
                return Results.Ok(response);
            }
            response.Success = false;
            response.Message = syncService.ErrorMessage;
            return Results.BadRequest(response);
        }

        private static async Task<IResult> SyncLoginUsers(DateTime syncDate, SyncService syncService)
        {
            response.Clear();
            var res= await syncService.SyncLoginUser(syncDate);
            if (res != null)
            {
                response.Success = true;
                response.Data = res;
                return Results.Ok(response);
            }
            response.Success = false;
            response.Message = syncService.ErrorMessage;
            return Results.BadRequest(response);
        }

        private static async Task<IResult> GetSyncDates(SyncService syncService)
        {
            response.Clear();
            var res= await syncService.GetLastSyncDates();
            if (res != null)
            {
                response.Success = true;
                response.Data = res;
                return Results.Ok(response);
            }
            response.Success = false;
            response.Message = syncService.ErrorMessage;
            return Results.BadRequest(response);
        }
    }
}
