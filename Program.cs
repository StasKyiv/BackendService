using BackendService.Configuration;
using BackendService.Repository;
using BackendService.ServiceBus.Implementation;
using DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IHost = Microsoft.Extensions.Hosting.IHost;

namespace BackendService;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Create a HostBuilder to manage dependencies and configuration
            using IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Load appsettings.json
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    // Bind settings from appsettings.json to the model
                    services.Configure<AzureServiceBusSettings>(
                        context.Configuration.GetSection("AzureServiceBus"));

                    // Add the hosted service to consume messages from Azure Service Bus
                    
                    // Get the SQL Server connection string from appsettings.json or environment variables
                    var connectionString = context.Configuration.GetConnectionString("SqlServer");

                    // Register DbContext for SQL Server
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(connectionString));
                    
                    services.AddHostedService<AzureServiceBusConsumer>();
                    services.InitializeServices();
                    services.AddAutoMapper(typeof(MappingProfile));
                    services.AddTransient<MigrationService>();
                })
                .Build();
            
            // Get the migration service and apply migrations
            var migrationService = host.Services.GetRequiredService<MigrationService>();
            migrationService.MigrateDatabase();
            
            // Run the application
            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}