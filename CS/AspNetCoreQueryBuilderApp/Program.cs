using AspNetCoreQueryBuilderApp.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreQueryBuilderApp {
    public class Program {
        public static void Main(string[] args) {
            var host = CreateWebHostBuilder(args).Build();
            InitializeDb(host);
            host.Run();

        }

        static void InitializeDb(IWebHost host) {
            using(var scope = host.Services.CreateScope()) {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ApplicationDbContext>();
                DbInitializer.Initialize(context);
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
        }
    }
}
