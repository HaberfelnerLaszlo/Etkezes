namespace Etkezes_API.Endpoints
{
    public static class RootEndpoint
    {
        public static void MapRootEndpoints(this WebApplication app)
        {
            app.MapGet("/", () => "Welcome to the Etkezes API! Verzió: 1.0 Dátum: 2026-02-19").WithName("Root");
        }
    }
}
