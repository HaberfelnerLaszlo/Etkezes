using Microsoft.EntityFrameworkCore;

namespace Etkezes_Ellenor.Data
{
    public static class DataExtension
    {
        public static void MigrationDb(this WebApplication app)
        {
            using var scope= app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<EtkezesDBcontext>();
            context.Database.Migrate();
        }
    }
}
