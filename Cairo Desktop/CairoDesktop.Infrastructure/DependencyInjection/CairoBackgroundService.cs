using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace CairoDesktop.Infrastructure.DependencyInjection
{
    public abstract class CairoBackgroundService : BackgroundService
    {
        protected Task ServiceStartTask;

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!stoppingToken.IsCancellationRequested)
            {
                ServiceStartTask.Start();
            }

            return ServiceStartTask;
        }
    }
}
