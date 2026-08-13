using Api;
using Api.Middlewares;
using Application;
using Infrastructure;
using Scalar.AspNetCore;
using Serilog;
using ConfigureServices = Api.ConfigureServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureService(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

builder.Services.AddOpenApi("v1", options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });

builder.Services.AddRazorPages();
builder.Services.AddControllers();

builder.Logging.ClearProviders();

Serilog.Debugging.SelfLog.Enable(Console.WriteLine);
builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);

    var seqUrl = builder.Configuration["SeqUrl"];

    if (seqUrl is not null)
    {
        configuration.WriteTo.Async(
            cfg => cfg.Seq(seqUrl),
            bufferSize: 5000,     
            blockWhenFull: true
        );
    }
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

await InitializeDatabaseAsync(app);

if (app.Environment.IsProduction() == false)
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Qalaat Al-Dhaman Api";
        options.WithTheme(ScalarTheme.BluePlanet);
        options.WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Fetch);
    });
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseStaticFiles();

if (app.Environment.IsProduction() == false)
    app.UseCors(ConfigureServices.DevelopmentCorsPolicy);
else
    app.UseCors(ConfigureServices.ProductionCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

// do not touch options parameter please
app.UseExceptionHandler(options => { });

app.MapRazorPages();
app.MapControllers();

app.Run();

static async Task InitializeDatabaseAsync(IApplicationBuilder app)
{
    await using var scope = app.ApplicationServices.CreateAsyncScope();

    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();

    await initializer.CreateDatabaseAsync();

    await initializer.SeedDataAsync();
}