using Connection;
using Server;
using System.Text;

namespace ServerTests
{
    public class ServerTest
    {
        [Fact]
        public async Task MultipleClientTest()
        {
            using (var server = new TcpServer())
            {
                server.Start();

                // Create multiple clients
                int numberOfClients = 2;
                var clients = new List<TcpConnection>();

                // Connect all clients
                for (int i = 0; i < numberOfClients; i++)
                {
                    var client = new TcpConnection();
                    await client.Connect("127.0.0.1", 50000);
                    clients.Add(client);
                }

                // Send messages from each client
                foreach (var client in clients)
                {
                    string message = $"Message from client {clients.IndexOf(client)}";
                    await client.SendAsync(Encoding.UTF8.GetBytes(message));

                    // Wait briefly to ensure message processing
                    await Task.Delay(100);
                }

                // Verify messages after all sends complete
                Assert.Equal(numberOfClients, server.messages.Count);
            }
        }

        [Fact]
        public async Task A_ClientSentA_Messages()
        {
            using (var server = new TcpServer())
            {
                server.Start();

                var client = new TcpConnection();
                await client.Connect("127.0.0.1", 50000);

                string message = "Message from client";
                await client.SendAsync(Encoding.UTF8.GetBytes(message));

                // Wait briefly to ensure message processing
                await Task.Delay(100);

                Assert.Equal(1, server.messages.Count);
            }
        }

        [Fact]
        public async Task ClientsSentMessages()
        {
            using (var server = new TcpServer())
            {
                server.Start();

                int numberOfClients = 2;
                var clients = new List<TcpConnection>();

                // Connect all clients
                for (int i = 0; i < numberOfClients; i++)
                {
                    var client = new TcpConnection();
                    await client.Connect("127.0.0.1", 50000);
                    clients.Add(client);
                }

                // Send messages from each client
                foreach (var client in clients)
                {
                    string message = $"Message from client {clients.IndexOf(client)}";
                    await client.SendAsync(Encoding.UTF8.GetBytes(message));

                    // Wait briefly to ensure message processing
                    await Task.Delay(100);
                }

                Assert.Equal(numberOfClients, server.messages.Count);
            }
        }
    }
}