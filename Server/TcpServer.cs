using Connection;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class TcpServer : IAsyncDisposable
    {
        //Threads
        public Thread serverThread { get; private set; }
        public Task acceptTask { get; private set; }
        public Task broadcastTask { get; private set; }

        private readonly TaskCompletionSource<bool> _acceptReady = new();
        private readonly TaskCompletionSource<bool> _broadcastReady = new();
        private Task ServerReady => Task.WhenAll(_acceptReady.Task, _broadcastReady.Task);


        TcpListener listerner = null;
        List<TcpConnection> clients = new List<TcpConnection>();

        public int Port { get; private set; }
        public IPAddress defaultPortLocalAddress { get; private set; } = IPAddress.Parse("127.0.0.1");

        bool running = true;

        public int numberOfClient
        {
            get
            {
                lock (clients)
                {
                    return clients.Count;
                }
            }
        }



        public async Task StartAsync()
        {
            listerner = new TcpListener(defaultPortLocalAddress, 0);
            listerner.Start();
            Port = ((IPEndPoint)listerner.LocalEndpoint).Port;

            //Make thread for server
            serverThread = new Thread(ServerWork);
            serverThread.Start();

            await ServerReady;
        }

        private async void ServerWork()
        {
            // StartAsync a thread to accept clients
            acceptTask = Task.Run(AcceptClients);
            broadcastTask = Task.Run(Brodcast);

            while (running)
            {

            }
        }

        async Task AcceptClients()
        {

            while (running) 
            {
                try
                {
                    _acceptReady.TrySetResult(true);
                    TcpClient tcpClient = await listerner.AcceptTcpClientAsync();

                    TcpConnection connection = new TcpConnection(tcpClient);
                    connection.goingToDisconnect += DisconnectClient;
                    lock (clients)
                    {
                        clients.Add(connection);
                    }

                    _ = HandleClientAsync(connection);
                }
                catch (SocketException ex) when (!running)
                {
                    // Expected when listener is stopped
                    break;
                }
                catch (ObjectDisposedException) when (!running)
                {
                    // Also expected if listener is disposed
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Accept failed: {ex.Message}");
                }

            }
        }

        private async Task HandleClientAsync(TcpConnection connection)
        {
            try
            {
                var data = await connection.ReceiveAsync(1024);

                string message = Encoding.UTF8.GetString(data);

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
                lock (clients)
                {
                    clients.Remove(connection);
                }
                Console.WriteLine("Client disconnected.");
            }
        }

        async Task Brodcast()
        {

            while (running)
            {
                _broadcastReady.TrySetResult(true);

            }
        }


        public void BroadcastMessage(string message)
        {

        }

        public async ValueTask DisposeAsync()
        {
            running = false;

            listerner?.Stop();

            if (acceptTask != null) await acceptTask;
            if (broadcastTask != null) await broadcastTask;

            foreach (var client in clients.ToList())
            {
                await client.DisposeAsync();
            }

            clients.Clear();
        }

        public async void DisconnectClient(object? sender, TcpConnectionEventArgs e)
        {
            lock (clients)
            {
                clients.Remove(e.Connection);
            }
            e.Connection.goingToDisconnect -= DisconnectClient;
        }


        public async Task Stop ()
        {
            await DisposeAsync();

            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        
    }
}
