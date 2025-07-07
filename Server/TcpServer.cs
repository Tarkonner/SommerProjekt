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
        public Thread acceptThread { get; private set; }
        public Thread broadcastThread { get; private set; }

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
            get { return clients.Count; }
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
            try
            {
                // StartAsync a thread to accept clients
                acceptThread = new Thread(AcceptClients);
                acceptThread.IsBackground = true;
                acceptThread.Start();

                broadcastThread = new Thread(Brodcast);
                broadcastThread.IsBackground = true;
                broadcastThread.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e);
            }
            finally
            {
                await DisposeAsync();
            }
        }

        async void AcceptClients()
        {
            _acceptReady.TrySetResult(true);

            while (running) 
            {
                // Accept client asynchronously
                TcpClient tcpClient = await listerner.AcceptTcpClientAsync();

                // Wrap in TcpConnection
                TcpConnection connection = new TcpConnection(tcpClient);

                // Track connection if needed
                clients.Add(connection);

                // Handle client asynchronously (no new thread, just a background task)
                _ = HandleClientAsync(connection);
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
                clients.Remove(connection);
                Console.WriteLine("Client disconnected.");
            }
        }

        async void Brodcast()
        {
            _broadcastReady.TrySetResult(true);

            while (running)
            {

            }
        }


        public void BroadcastMessage(string message)
        {

        }

        public async ValueTask DisposeAsync()
        {
            running = false;
        }


        public async Task Stop ()
        {
            await DisposeAsync();

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}
