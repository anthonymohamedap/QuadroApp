using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuadroApp.Data;

namespace QuadroApp
{
    public static class AppServices
    {
        private static readonly ServiceProvider _provider;
        private static readonly AppDbContext _db;

        public static AppDbContext Db => _db;

        static AppServices()
        {
            var services = new ServiceCollection();

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    "Server=tcp:quadroserver.database.windows.net,1433;" +
                    "Initial Catalog=QuadroDB;" +
                    "Persist Security Info=False;" +
                    "User ID=AdminStudent;" +
                    "Password=Quadro11;" +
                    "MultipleActiveResultSets=False;" +
                    "Encrypt=True;" +
                    "TrustServerCertificate=False;" +
                    "Connection Timeout=30;"));

            _provider = services.BuildServiceProvider();

            // ✅ Maak en bewaar één enkele DbContext-instantie
            _db = _provider.GetRequiredService<AppDbContext>();
        }
    }
}
