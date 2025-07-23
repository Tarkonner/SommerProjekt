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
                    TcpClient tcpClient = await _listener.AcceptTcpClientAsync(token);

                    var connection = new TcpConnection(tcpClient);
                    clients.Add(connection);

                    //Handshake
                    byte[] buffer = new byte[HandshakeMessage.clientMessage.Length];
                    // 1. Read handshake
                    int offset = 0;
                    while (offset < buffer.Length)
                    {
                        int read = await connection.stream.ReadAsync(buffer, offset, buffer.Length - offset);
                        if (read == 0)
                            throw new IOException("Client disconnected during handshake");
                        offset += read;
                    }

                    // 2. Verify magic bytes
                    for (int i = 0; i < HandshakeMessage.clientMessage.Length; i++)
                    {
                        if (buffer[i] != HandshakeMessage.clientMessage[i])
                            throw new IOException("Invalid client handshake");
                    }

                    // 3. Send confirmation
                    await connection.stream.WriteAsync(HandshakeMessage.serverMessage, 0, HandshakeMessage.serverMessage.Length);
                    await connection.stream.FlushAsync();

                    _ = HandleClientAsync(connection);
                }
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
                await item.DisposeAsync();

            _listener.Stop();

            if (acceptTask != null)
                await acceptTask;

            _cts?.Dispose();
        }
    }
}
