using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MrHell;
using MrHell.Configuration;
using MrHell.Data;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        // Load configuration from appsettings.json
        config.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        
        // Add config variables.
        config.AddEnvironmentVariables();
    })
    .ConfigureLogging((context, logging) =>
    {
        logging.AddConsole();
    })
    .ConfigureServices((context, services) =>
    {
        // Bind the configuration section "MyServiceOptions" to MyServiceOptions class
        services.Configure<BotOptions>(context.Configuration.GetSection("BotSettings"));
        
        
        services.AddSingleton<HellBot>();
        services.AddHostedService<HostedHellBot>();
    })
    .Build();

// Create a scope to create a migration
using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    await using var db = new HellContext();

    logger.LogInformation("Checking migrations... ({PATH})", db.DbPath);
    var requiredMigrations = (await db.Database.GetPendingMigrationsAsync()).ToArray();
    logger.LogInformation($"Pending migrations: {string.Join(", ", requiredMigrations)}");
    if (requiredMigrations.Length != 0)
    {
        logger.LogInformation("Doing those migrations now...");
        await db.Database.MigrateAsync();
    }
}

await host.RunAsync();

Console.WriteLine("Application stopped.");