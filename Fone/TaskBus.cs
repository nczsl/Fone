using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// 来自于msdn文档的，后台任务队列，应该可以直接使用
/// </summary>
namespace Fone {
    public interface IBackgroundTaskQueue {
        void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);

        Task<Func<CancellationToken, Task>> DequeueAsync(
            CancellationToken cancellationToken);
    }

    public class BackgroundTaskQueue : IBackgroundTaskQueue {
        private ConcurrentQueue<Func<CancellationToken, Task>> _workItems =
            new ConcurrentQueue<Func<CancellationToken, Task>>();
        private SemaphoreSlim _signal = new SemaphoreSlim(0);

        public void QueueBackgroundWorkItem(
            Func<CancellationToken, Task> workItem) {
            if (workItem == null) {
                throw new ArgumentNullException(nameof(workItem));
            }

            _workItems.Enqueue(workItem);
            _signal.Release();
        }

        public async Task<Func<CancellationToken, Task>> DequeueAsync(
            CancellationToken cancellationToken) {
            await _signal.WaitAsync(cancellationToken);
            _workItems.TryDequeue(out var workItem);

            return workItem;
        }
    }
    public class QueuedHostedService : BackgroundService {
        private readonly ILogger _logger;

        public QueuedHostedService(IBackgroundTaskQueue taskQueue,
            ILoggerFactory loggerFactory) {
            TaskQueue = taskQueue;
            _logger = loggerFactory.CreateLogger<QueuedHostedService>();
        }

        public IBackgroundTaskQueue TaskQueue { get; }

        protected async override Task ExecuteAsync(
            CancellationToken cancellationToken) {
            _logger.LogInformation("Queued Hosted Service is starting.");

            while (!cancellationToken.IsCancellationRequested) {
                var workItem = await TaskQueue.DequeueAsync(cancellationToken);

                try {
                    await workItem(cancellationToken);
                } catch (Exception ex) {
                    _logger.LogError(ex,
                       "Error occurred executing {WorkItem}.", nameof(workItem));
                }
            }

            _logger.LogInformation("Queued Hosted Service is stopping.");
        }
    }
}
