using Connection;
using Server;
using System.Text;

namespace ServerTests
{
    public class ServerTest
    {
        [Fact]
        public void MultipleClientTest()
        {
            using (var server = new TcpServer())
            {
                server.Start();

                // Create multiple clients
                int numberOfClients = 2;
                var clients = new List<TcpConnection>();
                for (int i = 0; i < numberOfClients; i++)
                {
                    var client = new TcpConnection();
                    client.Connect("127.0.0.1", 50000);
                    clients.Add(client);
                }

                // Send messages from each client
                foreach (var client in clients)
                {
                    client.SendAsync(Encoding.UTF8.GetBytes($"Message from client {clients.IndexOf(client)}"));
                }

                Thread.Sleep(100);

                // Verify messages
                Assert.Equal(numberOfClients, server.numberOfClient);
            }
        }

        [Fact]
        public void A_ClientSentA_Messages()
        {
            using (var server = new TcpServer())
            {
                server.Start();

                var client = new TcpConnection();
                client.Connect("127.0.0.1", 50000);

                client.SendAsync(Encoding.UTF8.GetBytes($"Message from client"));

                Thread.Sleep(100);

                // Verify messages
                Assert.Equal(1, server.messages.Count);
            }
        }

        [Fact]
        public void ClientsSentMessages()
        {
            using (var server = new TcpServer())
            {
                server.Start();

                // Create multiple clients
                int numberOfClients = 2;
                var clients = new List<TcpConnection>();
                for (int i = 0; i < numberOfClients; i++)
                {
                    var client = new TcpConnection();
                    client.Connect("127.0.0.1", 50000);
                    clients.Add(client);
                }

                // Send messages from each client
                foreach (var client in clients)
                {
                    client.SendAsync(Encoding.UTF8.GetBytes($"Message from client {clients.IndexOf(client)}"));
                }

                Thread.Sleep(100);

                // Verify messages
                Assert.Equal(numberOfClients, server.messages.Count);
            }
        }
    }
}