using Etkezes_API.Services;

using Etkezes_Models;

namespace Etkezes_API.Endpoints
{
    public static class LoginUserEndpoint
    {
        private static MainResponse response = new MainResponse();
        public static void MapLoginUserEndpoints(this WebApplication app)
        {
            app.MapGet("/loginusers",(LoginUserService loginUserService) => GetLocalUsers(loginUserService));
            app.MapGet("/loginuser/{id}", (string id, LoginUserService loginUserService) => GetLoginUserById(loginUserService, id));
            app.MapGet("/loginusers/{datum}", (DateTime datum, LoginUserService loginUserService) => GetLoginUsersByUpDate(loginUserService, datum));
            app.MapPost("/loginuser",(LoginUser loginUser, LoginUserService loginUserService) =>  CreateLoginUser(loginUserService, loginUser));
            app.MapPut("/loginuser/{id}", (LoginUser loginUser, LoginUserService loginUserService, string id) => UpdateLoginUser(loginUserService, id,loginUser));
            app.MapDelete("/loginuser/{id}", (LoginUserService loginUserService, string id) => DeleteLoginUser(loginUserService, id));
        }
        private static Task<IResult> GetLocalUsers(LoginUserService loginUserService)
        {
            response.Clear();
            var users = loginUserService.GetAllLoginUsers();
            if (users == null || users.Count == 0)
            {
                return Task.FromResult(Results.NotFound("No users found."));
            }
            response.Success = true;
            response.Data = users;
            return Task.FromResult(Results.Ok(response));
        }
        private static Task<IResult> GetLoginUsersByUpDate(LoginUserService loginUserService, DateTime datum)
        { 
            response.Clear();
            var users = loginUserService.GetLoginUsersByUpDate(datum);
            if (users == null || users.Count == 0)
            {
                return Task.FromResult(Results.NotFound("User not found."));
            }
            response.Success = true;
            response.Data = users;
            return Task.FromResult(Results.Ok(response));
        }
        private static Task<IResult> GetLoginUserById(LoginUserService loginUserService, string id)
        {   
            response.Clear();
            var user = loginUserService.GetLoginUserById(Guid.Parse(id));
            if (user == null)
            {
                return Task.FromResult(Results.NotFound("User not found."));
            }
            response.Success = true;
            response.Data = user;
            return Task.FromResult(Results.Ok(response));
        }
        private static Task<IResult> CreateLoginUser(LoginUserService loginUserService, LoginUser loginUser)
        {
            response.Clear();
            if (loginUserService.CreateLoginUser(loginUser))
            {
                response.Success = true;
                response.Data = loginUser;
                return Task.FromResult(Results.Created($"/loginusers/{loginUser.Id}", response));
            }
            return Task.FromResult(Results.BadRequest(loginUserService.ErrorMessage));
        }
        private static Task<IResult> UpdateLoginUser(LoginUserService loginUserService,string id, LoginUser loginUser)
        {
            response.Clear();
            if (Guid.Parse(id) != loginUser.Id)
            {
                return Task.FromResult(Results.BadRequest("ID mismatch."));
            }
            if (loginUserService.UpdateLoginUser(loginUser, Guid.Parse(id)))
            {
                response.Success = true;
                response.Data = loginUser;
                return Task.FromResult(Results.Ok(response));
            }
            return Task.FromResult(Results.BadRequest(loginUserService.ErrorMessage));
        }
        private static Task<IResult> DeleteLoginUser(LoginUserService loginUserService, string id)
        {
            if (loginUserService.DeleteLoginUser(Guid.Parse(id)))
            {
                response.Success = true;
                return Task.FromResult(Results.NoContent());
            }
            return Task.FromResult(Results.BadRequest(loginUserService.ErrorMessage));
        }
    }
}
