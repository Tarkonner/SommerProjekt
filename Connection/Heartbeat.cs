namespace CommenCompunents
{
    public class Heartbeat : IHeartbeatHandler
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public Task heartbeatTask { get; private set; }
        public int heartbeatIntervalInMiliseconds { get; set; }

        public event Action OnHeartbeat;
        private bool _running;

        public void Start(int timeBetweenBeats = 200)
        {
            heartbeatIntervalInMiliseconds = timeBetweenBeats;
            _running = true;
            heartbeatTask = HeartbeatLogic(_cts.Token);
        }

        public async Task StopAsync()
        {
            _cts.Cancel();
            _running = false;

            if (heartbeatTask == null)
                return;

            try
            {
                await heartbeatTask;
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Heartbeat task threw unexpected exception: {ex}");
                throw;
            }

            // Ensure we don't deadlock if task exited before reaching _semaphore.Release()
            try
            {
                await _semaphore.WaitAsync(TimeSpan.FromSeconds(2));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error waiting on semaphore: {ex}");
            }
        }

        public async Task HeartbeatLogic(CancellationToken cancellationToken)
        {
            try
            {
                while (_running && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(heartbeatIntervalInMiliseconds, cancellationToken);

                    try
                    {
                        OnHeartbeat?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error during heartbeat callback: {ex}");
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
