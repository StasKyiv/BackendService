using DataBase;
using Microsoft.Extensions.DependencyInjection;

namespace BackendService.Repository;

public class MigrationService
{
    private readonly IServiceProvider _serviceProvider;

    public MigrationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void MigrateDatabase()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        context.Database.EnsureCreated();
        Console.WriteLine("Database migration applied successfully!");
    }
}