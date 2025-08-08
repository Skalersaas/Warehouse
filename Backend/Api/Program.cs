using Api.Middleware;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Persistence.Data.Interfaces;
using Persistence.Data.Repositories;
using Utilities;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureBuilder(builder);

        var app = builder.Build();

        ConfigureApp(app);

        app.Run();

        static void ConfigureBuilder(WebApplicationBuilder builder)
        {
#if DEBUG
            foreach (var path in new []{ ".env", "../.env" })
            {
                if (File.Exists(path))
                {
                    EnvLoader.LoadEnvFile(path);
                    break;
                }
            }
#endif
            // Database
            ConfigureDatabase(builder.Services);

            // Routes
            ConfigureRoutes(builder.Services);

            // Swagger
            ConfigureSwagger(builder.Services);

            // Repositories
            AddRepositories(builder.Services);

            // Services
            AddServices(builder.Services);

            builder.Services.AddAuthorization();
        }

        static void ConfigureApp(WebApplication app)
        {
            // Apply migrations
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                context.Database.Migrate();
            }

            // CORS
            app.UseCors(builder =>
                builder.WithOrigins("http://localhost:3000",
                                    "https://crm-dashboard-umber-sigma.vercel.app")
                        .AllowAnyMethod()
                        .AllowAnyHeader());

            // Redirect root to swagger
            app.UseRewriter(new RewriteOptions()
                .AddRedirect("^$", "swagger/index.html"));

            // Middleware
            app.UseMiddleware<LoggingMiddleware>();
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Swagger
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.MapControllers();
        }

        #region Configuration Methods

        static void ConfigureDatabase(IServiceCollection services)
        {
            string? cs = Environment.GetEnvironmentVariable("ConnectionString");
            services.AddDbContext<ApplicationContext>(options => options.UseNpgsql(cs));
        }

        static void AddRepositories(IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IArchivableRepository<>), typeof(ArchivableRepository<>));
        }

        static void AddServices(IServiceCollection services)
        {
            // Core business services
            services.AddScoped<IBalanceService, BalanceService>();

            // Entity services
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<IResourceService, ResourceService>();
            services.AddScoped<IUnitService, UnitService>();
            services.AddScoped<IReceiptDocumentService, ReceiptDocumentService>();
            services.AddScoped<IShipmentDocumentService, ShipmentDocumentService>();
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
                    Version = "v1",
                    Description = "Warehouse Management System API"
                });
            });
        }
        #endregion
    }
}


