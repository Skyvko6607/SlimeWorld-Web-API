using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SlimeWorldWebAPI.Repositories.Interfaces;

namespace SlimeWorldWebAPI.Tasks
{
    public class SlimeWorldTask
    {
        public ISlimeWorldRepository SlimeWorldRepository { get; set; }

        public SlimeWorldTask(IServiceScopeFactory serviceScopeFactory,
            IHostApplicationLifetime applicationLifetime)
        {
            var scope = serviceScopeFactory.CreateScope();
            SlimeWorldRepository = scope.ServiceProvider.GetService<ISlimeWorldRepository>();
            StartSaveTask();

            applicationLifetime.ApplicationStopping.Register(async () => { await SlimeWorldRepository.SaveChangesAsync(); });
        }

        public void StartSaveTask()
        {
            var cts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    await SlimeWorldRepository.SaveChangesAsync();
                    await Task.Delay(TimeSpan.FromMinutes(1), cts.Token);
                }
            }, cts.Token);
        }
    }
}
