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
                var clients = new List<TcpConnection>();
                for (int i = 0; i < 3; i++)
                {
                    var client = new TcpConnection();
                    client.Connect("127.0.0.1", 50000);
                    clients.Add(client);
                }

                // Send messages from each client
                foreach (var client in clients)
                {
                    client.Send(Encoding.UTF8.GetBytes($"Message from client {clients.IndexOf(client)}"));
                }

                Thread.Sleep(100);

                // Verify messages
                Assert.Equal(3, server.messages.Count);
            }
        }
    }
}