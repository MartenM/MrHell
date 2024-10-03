using Microsoft.Extensions.Hosting;

namespace MrHell;

/// <summary>
/// Wrapper to enable working as a background service.
/// </summary>
public class HostedHellBot : BackgroundService
{
    private HellBot _hellBot;
    private IHostApplicationLifetime _applicationLifetime;

    public HostedHellBot(HellBot hellBot, IHostApplicationLifetime applicationLifetime)
    {
        _hellBot = hellBot;
        _applicationLifetime = applicationLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _hellBot.Start();
        await _hellBot.Run(stoppingToken);
        
        // When this completes shut down the complete service.
        _applicationLifetime.StopApplication();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _hellBot.Stop();
        await base.StopAsync(cancellationToken);
    }
}