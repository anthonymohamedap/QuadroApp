using Avalonia;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuadroApp.Data;

namespace QuadroApp
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // 1️⃣ Configureer dependency injection
            var services = new ServiceCollection();

            // ✅ Gebruik een context factory (belangrijk voor Avalonia threading)
            services.AddDbContextFactory<AppDbContext>(options =>
                options.UseSqlServer(
                    "Server=tcp:quadroserver.database.windows.net,1433;" +
                    "Initial Catalog=QuadroDB;" +
                    "Persist Security Info=False;" +
                    "User ID=AdminStudent;" +
                    "Password=Quadro11;" +
                    "MultipleActiveResultSets=False;" +
                    "Encrypt=True;" +
                    "TrustServerCertificate=True;" +  // ✅ Vermijdt SSL-problemen met Azure
                    "Connection Timeout=30;"));

            // 2️⃣ Bouw de ServiceProvider
            var serviceProvider = services.BuildServiceProvider();

            // 3️⃣ Eventueel: voer automatisch migraties uit (optioneel, kan verwijderd worden)
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
                    using var db = dbFactory.CreateDbContext();
                    db.Database.Migrate();
                    Console.WriteLine("✅ Database connected en gemigreerd");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️  Database connectie of migratie mislukt:");
                Console.WriteLine(ex.Message);
            }

            // 4️⃣ Start Avalonia met de serviceprovider
            BuildAvaloniaApp(serviceProvider).StartWithClassicDesktopLifetime(args);
        }

        // ✅ Bouw AvaloniaApp en geef de DI-services door aan de App-klasse
        public static AppBuilder BuildAvaloniaApp(IServiceProvider services)
            => AppBuilder.Configure(() => new App(services))
                         .UsePlatformDetect()
                         .WithInterFont()
                         .LogToTrace();
    }
}
