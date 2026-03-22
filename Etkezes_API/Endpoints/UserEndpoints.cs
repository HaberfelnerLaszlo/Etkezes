using Etkezes_API.Services;

using Etkezes_Models;

namespace Etkezes_API.Endpoints
{
    public static class UserEndpoints
    {
        static MainResponse response = new MainResponse();
        public static void MapUserEndpoints(this WebApplication app)
        {
            app.MapGet("/users", (UserService userService) => GetUsers(userService)).WithName("GetUsers");
            app.MapGet("/users/{osztaly}", (string osztaly, UserService userService) => GetUsersByOsztalyAsync(osztaly, userService));
            app.MapGet("/user/{id}", (long id, UserService userService) => GetUserById(id, userService)).WithName("GetUserByIdAsync");
            app.MapGet("/user/update/{date}", (DateTime date, UserService userService) => GetUsersByUpDate(date, userService)).WithName("GetUserByUpDate");
            app.MapPost("/user", (UserService userService, User user) => CreateUser(userService, user)).WithName("CreateUserAsync");
            app.MapPut("/user/{id}", (UserService userService, long id, User user) => UpdateUser(userService, id, user)).WithName("UpdateUserAsync");
            app.MapDelete("/user/{id}", (UserService userService, long id) => DeleteUser(userService, id)).WithName("DeleteUserAsync");
        }

        private static async Task<IResult> GetUsersByOsztalyAsync(string osztaly, UserService userService)
        {
            response.Clear();
            var list = await userService.GetUsersByOsztalyAsync(osztaly);
            if (list == null) { return await Task.FromResult(Results.NotFound()); }
            response.Success = true;
            response.Data = list;
            return await Task.FromResult(Results.Ok(response));
        }

        private static async Task<IResult> GetUsers(UserService userService)
        {
            response.Clear();
            var users = await userService.GetAllUsersAsync();
            if (users == null) { return await Task.FromResult(Results.NotFound()); }
            response.Success = true;
            response.Data = users;
            return await Task.FromResult(Results.Ok(response));
        }
        private static async Task<IResult> GetUserById(long id, UserService userService)
        {
            response.Clear();
            var user = await userService.GetUserByIdAsync(id);
            response.Success = user != null;
            response.Data = user;
            return await Task.FromResult(Results.Ok(response));
        }
        private static async Task<IResult> GetUsersByUpDate(DateTime date, UserService userService)
        {
            response.Clear();
            var users = await userService.GetUsersByUpDateAsync(date);
            if (users == null) { return await Task.FromResult(Results.NotFound()); }
            response.Success = users != null && users.Count > 0;
            response.Data = users;
            return await Task.FromResult(Results.Ok(response));
        }
        private static async Task<IResult> CreateUser(UserService userService, User user)
        {
            response.Clear();
            if (await userService.CreateUserAsync(user))
            {
                response.Success = true;
                response.Data = user;
                return await Task.FromResult(Results.Ok(response));
            }
            response.Success = false;
            response.Message = userService.ErrorMessage;
            return await Task.FromResult(Results.BadRequest(response));
        }
        private static async Task<IResult> UpdateUser(UserService userService, long id, User user)
        {
            response.Clear();
            if (await userService.UpdateUserAsync(id, user))
            {
                response.Success = true;
                response.Data = user;
                return await Task.FromResult(Results.Ok(response));
            }
            response.Success = false;
            response.Message = userService.ErrorMessage;
            return await Task.FromResult(Results.BadRequest(response));
        }
        private static async Task<IResult> DeleteUser(UserService userService, long id)
        {
            await userService.DeleteUserAsync(id);
            return await Task.FromResult(Results.NoContent());
        }
    }
}
