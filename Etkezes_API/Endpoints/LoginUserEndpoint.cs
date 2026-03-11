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
            app.MapGet("/loginuser/valid/{username}",(string username, LoginUserService loginUserService)=>GetLoginUserByUserName(loginUserService, username));
            app.MapGet("/loginusers/{datum}", (DateTime datum, LoginUserService loginUserService) => GetLoginUsersByUpDate(loginUserService, datum));
            app.MapPost("/loginuser",(LoginUser loginUser, LoginUserService loginUserService) =>  CreateLoginUser(loginUserService, loginUser));
            app.MapPut("/loginuser/{id}", (LoginUser loginUser, LoginUserService loginUserService, string id) => UpdateLoginUser(loginUserService, id,loginUser));
            app.MapDelete("/loginuser/{id}", (LoginUserService loginUserService, string id) => DeleteLoginUser(loginUserService, id));
        }

        private static async Task<IResult> GetLoginUserByUserName(LoginUserService loginUserService, string username)
        {
            response.Clear();
            var user= await loginUserService.GetLoginUserByUserNameAsync(username);
            if (user != null) { 
                response.Data = user;
                return await Task.FromResult(Results.Ok(response));
            }
            response.Success = false;
            response.Message = "A felhasználó nem található.";
            return await Task.FromResult(Results.NotFound(response));
        }

        private static async Task<IResult> GetLocalUsers(LoginUserService loginUserService)
        {
            response.Clear();
            var users =await loginUserService.GetAllLoginUsers();
            if (users == null || users.Count == 0)
            {
                return await Task.FromResult(Results.NotFound("No users found."));
            }
            response.Success = true;
            response.Data = users;
            return await Task.FromResult(Results.Ok(response));
        }
        private static async Task<IResult> GetLoginUsersByUpDate(LoginUserService loginUserService, DateTime datum)
        { 
            response.Clear();
            var users = await loginUserService.GetLoginUsersByUpDateAsync(datum);
            if (users == null || users.Count == 0)
            {
                return await Task.FromResult(Results.NotFound("User not found."));
            }
            response.Success = true;
            response.Data = users;
            return await Task.FromResult(Results.Ok(response));
        }
        private static async Task<IResult> GetLoginUserById(LoginUserService loginUserService, string id)
        {   
            response.Clear();
            var user = await loginUserService.GetLoginUserByIdAsync(Guid.Parse(id));
            if (user == null)
            {
                return await Task.FromResult(Results.NotFound("User not found."));
            }
            response.Success = true;
            response.Data = user;
            return await Task.FromResult(Results.Ok(response));
        }
        private static async Task<IResult> CreateLoginUser(LoginUserService loginUserService, LoginUser loginUser)
        {
            response.Clear();
            if (await loginUserService.CreateLoginUserAsync(loginUser))
            {
                response.Success = true;
                response.Data = loginUser;
                return await Task.FromResult(Results.Created($"/loginusers/{loginUser.Id}", response));
            }
            return await Task.FromResult(Results.BadRequest(loginUserService.ErrorMessage));
        }
        private static async Task<IResult> UpdateLoginUser(LoginUserService loginUserService,string id, LoginUser loginUser)
        {
            response.Clear();
            if (Guid.Parse(id) != loginUser.Id)
            {
                return await Task.FromResult(Results.BadRequest("ID mismatch."));
            }
            if (await loginUserService.UpdateLoginUserAsync(loginUser, Guid.Parse(id)))
            {
                response.Success = true;
                response.Data = loginUser;
                return await Task.FromResult(Results.Ok(response));
            }
            return await Task.FromResult(Results.BadRequest(loginUserService.ErrorMessage));
        }
        private static async Task<IResult> DeleteLoginUser(LoginUserService loginUserService, string id)
        {
            await loginUserService.DeleteLoginUserAsync(Guid.Parse(id));
            return await Task.FromResult(Results.NoContent());
        }
    }
}
