using Microsoft.EntityFrameworkCore;

namespace MTOGO.IntegrationTests
{
    public static class DatabaseHelper
    {
        public static void ResetDatabase(DbContext context)
        {
            context.Database.ExecuteSqlRaw("EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'");
            context.Database.ExecuteSqlRaw("EXEC sp_MSForEachTable 'DELETE FROM ?'");
            context.Database.ExecuteSqlRaw("EXEC sp_MSForEachTable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'");
        }

        public static void ApplyMigrations(DbContext context)
        {
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }
        }
    }

}
