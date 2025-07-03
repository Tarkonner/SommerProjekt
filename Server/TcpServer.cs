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

        public void Start()
        {
            try
            {
                server = new TcpListener(localAddress, port);
                server.Start();
                Console.WriteLine($"Server started on {localAddress}:{port}");

                // Start a thread to accept clients
                Thread acceptThread = new Thread(AcceptClients);
                acceptThread.IsBackground = true;
                acceptThread.Start();

                // Main thread handles sending messages from console to all clients
                while (running)
                {
                    string message = Console.ReadLine();

                    if (message.ToLower() == "exit")
                    {
                        running = false;
                        break;
                    }

                    BroadcastMessage(message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e);
            }
            finally
            {
                StopServer();
            }
        }

        void AcceptClients()
        {
            while(running)
            {

            }
        }


        static void BroadcastMessage(string message)
        {
        }


        public void StopServer()
        {
            running = false;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
