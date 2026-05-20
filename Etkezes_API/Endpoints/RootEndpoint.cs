namespace Etkezes_API.Endpoints
{
    public static class RootEndpoint
    {
        public static void MapRootEndpoints(this WebApplication app)
        {
            app.MapGet("/", () => "Welcome to the Etkezes API! Verzió: 0.6.8 Dátum: 2026-05-18");
        }
    }
}
