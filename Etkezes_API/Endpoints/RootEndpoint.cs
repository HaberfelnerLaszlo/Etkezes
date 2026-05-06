namespace Etkezes_API.Endpoints
{
    public static class RootEndpoint
    {
        public static void MapRootEndpoints(this WebApplication app)
        {
            app.MapGet("/", () => "Welcome to the Etkezes API! Verzió: 0.6.7 Dátum: 2026-04-25");
        }
    }
}
