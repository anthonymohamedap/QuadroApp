using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;

namespace QuadroApp.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            // 🔹 Zorg dat dit pad klopt — SQLite-bestand in hoofdmap van je project
            optionsBuilder.UseSqlServer("Server=tcp:quadroserver.database.windows.net,1433;Initial Catalog=QuadroDB;Persist Security Info=False;User ID=AdminStudent;Password=Quadro11;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
