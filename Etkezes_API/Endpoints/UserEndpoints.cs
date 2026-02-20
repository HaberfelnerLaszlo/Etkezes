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
            app.MapGet("/user/{id}", (long id, UserService userService) => GetUserById(id, userService)).WithName("GetUserById");
            app.MapGet("/user/update/{date}", (DateTime date, UserService userService) => GetUsersByUpDate(date, userService)).WithName("GetUserByUpDate");
            app.MapPost("/user", (UserService userService, User user) => CreateUser(userService, user)).WithName("CreateUser");
            app.MapPut("/user/{id}", (UserService userService, long id, User user) => UpdateUser(userService, id, user)).WithName("UpdateUser");
            app.MapDelete("/user/{id}", (UserService userService, long id) => DeleteUser(userService, id)).WithName("DeleteUser");
        }
        private static Task<IResult> GetUsers(UserService userService)
        {
            response.Clear();
            var users = userService.GetAllUsers();
            response.Success = true;
            response.Data = users;
            return Task.FromResult(Results.Ok(response));
        }
        private static Task<IResult> GetUserById(long id, UserService userService)
        {
            response.Clear();
            var user = userService.GetUserById(id);
            response.Success = user != null;
            response.Data = user;
            return Task.FromResult(Results.Ok(response));
        }
        private static Task<IResult> GetUsersByUpDate(DateTime date, UserService userService)
        {
            response.Clear();
            var users = userService.GetUsersByUpDate(date);
            response.Success = users != null && users.Count > 0;
            response.Data = users;
            return Task.FromResult(Results.Ok(response));
        }
        private static Task<IResult> CreateUser(UserService userService, User user)
        {
            response.Clear();
            if (userService.CreateUser(user))
            {
                response.Success = true;
                response.Data = user;
                return Task.FromResult(Results.Ok(response));
            }
            response.Success = false;
            response.Message = userService.ErrorMessage;
            return Task.FromResult(Results.BadRequest(response));
        }
        private static Task<IResult> UpdateUser(UserService userService, long id, User user)
        {
            response.Clear();
            if (userService.UpdateUser(id, user))
            {
                response.Success = true;
                response.Data = user;
                return Task.FromResult(Results.Ok(response));
            }
            response.Success = false;
            response.Message = userService.ErrorMessage;
            return Task.FromResult(Results.BadRequest(response));
        }
        private static Task<IResult> DeleteUser(UserService userService, long id)
        {
            response.Clear();
            if (userService.DeleteUser(id))
            {
                response.Success = true;
                return Task.FromResult(Results.Ok(response));
            }
            response.Success = false;
            response.Message = userService.ErrorMessage;
            return Task.FromResult(Results.BadRequest(response));
        }
    }
}
