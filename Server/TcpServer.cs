using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class TcpServer : IDisposable
    {
        static TcpListener server = null;

        int port = 60000;
        static IPAddress localAddress = IPAddress.Parse("127.0.0.1");

        bool running = true;

        // Thread-safe list of connected clients
        static readonly List<TcpClient> clients = new List<TcpClient>();
        static readonly object clientsLock = new object();

        public List<string> messages = new List<string>();

        public int numberOfClient
        {
            get { return clients.Count; }
        }

        Thread serverThread;
        Thread acceptThread;
        Thread broadcastThread;

        public void Start()
        {
            //Make thread for server
            serverThread = new Thread(ServerWork);
            serverThread.Start();
        }

        private void ServerWork()
        {
            try
            {
                server = new TcpListener(localAddress, port);
                server.Start();
                Console.WriteLine($"Server started on {localAddress}:{port}");

                // Start a thread to accept clients
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
                Dispose();
            }
        }

        void AcceptClients()
        {
            while (running) 
            {

            }
        }

        void Brodcast()
        {
            while (running)
            {

            }
        }


        public void BroadcastMessage(string message)
        {
        }

        public void Dispose()
        {
            running = false;

            acceptThread.Join();
            broadcastThread.Join();
            serverThread.Join();
        }
    }
}
