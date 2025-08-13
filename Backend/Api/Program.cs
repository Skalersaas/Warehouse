using Api.Middleware;
using Application.Interfaces;
using Application.Services;
using Application.Services.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Persistence.Data.Interfaces;
using Persistence.Data.Repositories;
using Utilities;

var builder = WebApplication.CreateBuilder(args);

ConfigureBuilder(builder);

var app = builder.Build();

ConfigureApp(app);

app.Run();
static void ConfigureBuilder(WebApplicationBuilder builder)
{
#if DEBUG
    EnvLoader.LoadEnvFile("../.env");
#endif
    // Db
    ConfigureDatabase(builder.Services);

    // Routes
    ConfigureRoutes(builder.Services);

    // Swagger
    ConfigureSwagger(builder.Services);

    // Repos
    AddScoped(builder.Services);
    builder.Services.AddAuthorization();
}
static void ConfigureApp(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        context.Database.Migrate();
    }
    app.UseCors(builder =>
        builder.WithOrigins("http://localhost:3000",
                            "https://crm-dashboard-umber-sigma.vercel.app")
                .AllowAnyMethod()
                .AllowAnyHeader());
    app.UseRewriter(new RewriteOptions()
        .AddRedirect("^$", "swagger/index.html"));

    app.UseMiddleware<LoggingMiddleware>();

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHttpsRedirection();


    app.MapControllers();
}
#region Configures
static void ConfigureDatabase(IServiceCollection services)
{
    string? cs = Environment.GetEnvironmentVariable("ConnectionString");
    services.AddDbContext<ApplicationContext>(options => options.UseNpgsql(cs));
}
static void AddScoped(IServiceCollection services)
{
    // Repositories
    services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    services.AddScoped(typeof(IArchivableRepository<>), typeof(ArchivableRepository<>));
    
    // Base Services
    services.AddScoped(typeof(IModelService<,,>), typeof(ModelService<,,>));
    services.AddScoped(typeof(IArchiveService<,,>), typeof(ArchiveService<,,>));
    
    // Specific Services
    services.AddScoped<BalanceService>();
    services.AddScoped<ResourceService>();
    services.AddScoped<UnitService>();
    services.AddScoped<ClientService>();
    services.AddScoped<ReceiptDocumentService>();
    services.AddScoped<ShipmentDocumentService>();
}
static void ConfigureRoutes(IServiceCollection services)
{
    services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
    services.Configure<ApiBehaviorOptions>(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });
    services.AddControllers();
}
static void ConfigureSwagger(IServiceCollection services)
{
       services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Warehouse API",
            Version = "v1"
        });
    });
}
#endregion