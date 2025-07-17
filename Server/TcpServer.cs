using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using CommenCompunents;
using Connection;

namespace Server
{
    public class TcpServer : IAsyncDisposable
    {
        //Threads
        public Thread serverThread { get; private set; }
        public Task acceptTask { get; private set; }
        public Task broadcastTask { get; private set; }

        //Tells then Server is ready to take and sent
        private readonly TaskCompletionSource<bool> _acceptReady = new();
        private readonly TaskCompletionSource<bool> _broadcastReady = new();
        private Task ServerReady => Task.WhenAll(_acceptReady.Task, _broadcastReady.Task);

        //Clients
        TcpListener listerner = null;
        List<TcpConnection> clients = new List<TcpConnection>();
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

        public int Port { get; private set; }
        public IPAddress defaultPortLocalAddress { get; private set; } = IPAddress.Parse("127.0.0.1");

        bool running = true;

        //Heartbeat
        public Heartbeat heartbeat { get; private set; } = new();

        //Message database
        private readonly ConcurrentQueue<string> broadcastQueue = new();

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
            acceptTask = AcceptClients();
            broadcastTask = Brodcast();      
            heartbeat.Start();

            try
            {
                await Task.WhenAll(acceptTask, broadcastTask, heartbeat.heartbeatTask);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("One or more tasks were cancelled.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex}");
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
                var data = await connection.ReceiveAsync();

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

                if (broadcastQueue.TryDequeue(out var message))
                {
                    await BroadcastMessage(message);
                }

                // Throttle broadcasts to prevent overload
                await Task.Delay(100);
            }
        }


        public async Task BroadcastMessage(string message)
        {
            lock (clients)
            {
                foreach (var client in clients)
                {
                    try
                    {
                        var data = Encoding.UTF8.GetBytes(message);
                        client.SendAsync(data);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Broadcast failed to client: {ex.Message}");
                        // Consider removing failed client
                    }
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            running = false;

            listerner?.Stop();

            await heartbeat.StopAsync();

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
