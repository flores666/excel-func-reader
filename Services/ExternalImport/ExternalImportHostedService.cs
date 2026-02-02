using Microsoft.Extensions.Options;

namespace ExcelFuncReader.Services.ExternalImport;

using Microsoft.Extensions.Hosting;

public sealed class ExternalImportHostedService : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly IOptions<ExternalImportOptions> _opt;
    private readonly ILogger<ExternalImportHostedService> _logger;

    public ExternalImportHostedService(
        IServiceProvider sp,
        IOptions<ExternalImportOptions> opt,
        ILogger<ExternalImportHostedService> logger)
    {
        _sp = sp;
        _opt = opt;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_opt.Value.Enabled)
        {
            _logger.LogInformation("External import is disabled.");
            return;
        }

        var interval = TimeSpan.FromSeconds(Math.Max(1, _opt.Value.IntervalSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<IExternalImportService>();

                await svc.ImportOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "External import loop iteration failed.");
            }

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
