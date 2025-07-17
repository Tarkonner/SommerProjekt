using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommenCompunents
{
    public class Heartbeat : IHeartbeatHandler
    {
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();

    public Task heartbeatTask { get; private set; }
    public int heartbeatIntervalInMiliseconds => 200;
    public event Action OnHeartbeat;
    private bool _running;

    public void Start()
    {
        _running = true;
        heartbeatTask = HeartbeatLogic(_cts.Token);
    }

    public async Task StopAsync()
    {
        _cts.Cancel();
        _running = false;
        try
        {
            await heartbeatTask;
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping
        }
        finally
        {
            await _semaphore.WaitAsync();
        }
    }

    public async Task HeartbeatLogic(CancellationToken cancellationToken)
    {
        try
        {
            while (_running && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(heartbeatIntervalInMiliseconds, cancellationToken);
                OnHeartbeat?.Invoke();
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
    }
}
