using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Persistence.Data;

namespace Tests.Infrastructure;

public class WarehouseTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove any existing database provider services
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType.Name.Contains("IDbContextOptionsExtension"));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
            // Build service provider and seed the database
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            SeedTestData(context);
        });

        builder.UseEnvironment("Testing");
    }


    private static void SeedTestData(ApplicationContext context)
    {
        // Add basic test data
        var unit = new Domain.Models.Entities.Unit
        {
            Id = 1,
            Name = "kg",
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        var resource = new Domain.Models.Entities.Resource
        {
            Id = 1,
            Name = "Steel Bars",
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        var client = new Domain.Models.Entities.Client
        {
            Id = 1,
            Name = "Test Client",
            Address = "123 Test Street",
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        context.Units.Add(unit);
        context.Resources.Add(resource);
        context.Clients.Add(client);
        context.SaveChanges();
    }
}
