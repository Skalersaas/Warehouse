using Api.Middleware;
using Application.Interfaces;
using Application.Models.ReceiptDocument;
using Application.Models.ReceiptItem;
using Application.Models.ShipmentDocument;
using Application.Models.ShipmentItem;
using Application.Services;
using Application.Services.Base;
using Application;
using Domain.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Utilities;
using Utilities.DataManipulation;
using Application.Models.Balance;

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

    // Mapper
    RegisterMappings();
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
static void RegisterMappings()
{
    Mapper.RegisterMapping<ReceiptDocument, ReceiptDocumentResponseDto>(map => map
        .Map(dest => dest.Items, src => src.Items.Select(item => item.ToResponseDto()))
    );

    Mapper.RegisterMapping<ReceiptItem, ReceiptItemResponseDto>(map => map
        .Map(dest => dest.ResourceName, src => src.Resource.Name)
        .Map(dest => dest.UnitName, src => src.Unit.Name)
    );

    Mapper.RegisterMapping<ShipmentDocument, ShipmentDocumentResponseDto>(map => map
        .Map(dest => dest.Items, src => src.Items.Select(item => item.ToResponseDto()))
        .Map(dest => dest.ClientName, src => src.Client.Name)
    );

    Mapper.RegisterMapping<ShipmentItem, ShipmentItemResponseDto>(map => map
        .Map(dest => dest.ResourceName, src => src.Resource.Name)
        .Map(dest => dest.UnitName, src => src.Unit.Name)
    );

    Mapper.RegisterMapping<Balance, BalanceResponseDto>(map => map
        .Map(dest => dest.ResourceName, src => src.Resource.Name)
        .Map(dest => dest.UnitName, src => src.Unit.Name)
    );
}

#endregion