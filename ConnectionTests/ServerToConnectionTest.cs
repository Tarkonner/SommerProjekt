using Connection;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ConnectionTests
{
    public class ServerToConnectionTest : IAsyncDisposable
    {
        private TcpListener _listener;
        private CancellationTokenSource _cts;
        private Task? acceptTask;

        public List<TcpConnection> clients { get; private set; } = new List<TcpConnection>();
        public List<string> messages { get; private set; } = new List<string>();

        public ServerToConnectionTest()
        {
            _listener = new TcpListener(IPAddress.Loopback, 0); // Initialize with any available port
            _cts = new CancellationTokenSource();
        }

        public Task StartListeningAsync(int port)
        {
            _listener = new TcpListener(IPAddress.Loopback, port);
            _listener.Start();

            acceptTask = AcceptClientsAsync(_cts.Token);

            return Task.CompletedTask;
        }

        private async Task AcceptClientsAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
#if NET8_0_OR_GREATER
                    TcpClient tcpClient = await _listener.AcceptTcpClientAsync(token);
#else
                    // Workaround for .NET < 8 since AcceptTcpClientAsync doesn't take CancellationToken
                    var acceptTask = _listener.AcceptTcpClientAsync();
                    using (token.Register(() => acceptTask.TrySetCanceled()))
                    {
                        TcpClient tcpClient = await acceptTask;
#endif
                    var connection = new TcpConnection(tcpClient);
                    clients.Add(connection);
                    _ = HandleClientAsync(connection);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown
            }
            catch (ObjectDisposedException)
            {
                // Listener disposed during shutdown
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled exception in AcceptClientsAsync: {ex}");
            }
        }

        private async Task HandleClientAsync(TcpConnection connection)
        {
            try
            {
                var data = await connection.ReceiveAsync();
                string message = Encoding.UTF8.GetString(data);
                messages.Add(message);

                var response = Encoding.UTF8.GetBytes(message);
                await connection.SendAsync(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");
            }
            finally
            {
                await connection.DisposeAsync();
                clients.Remove(connection);
                Console.WriteLine("Client disconnected.");
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cts?.Cancel();

            foreach (TcpConnection item in clients.ToList())
            {
                await item.DisposeAsync();
            }

            _listener.Stop();

            if (acceptTask != null)
                await acceptTask;

            _cts?.Dispose();
        }
    }
}
