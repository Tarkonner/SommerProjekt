using Connection;
using System.Net.Sockets;
using System.Text;

namespace ConnectionTests
{
    public class ConnectionTests
    {
        [Fact]
        public void InternelConnectTest()
        {
            TestServer testServer = new TestServer();

            testServer.Start();

            Assert.True(testServer.connection.IsSocketConnected());

            testServer.Stop();
        }

        [Fact]
        public void ExternelConnectTest()
        {
            TestServer testServer = new TestServer();
            testServer.Start();

            bool isConnected = testServer.connection._socket.Poll(0, SelectMode.SelectRead);
            Assert.False(isConnected);

            testServer.Stop();
        }

        [Fact]
        public void DisconnectTest()
        {
            TestServer testServer = new TestServer();

            testServer.connection.Disconnect();

            Assert.False(testServer.connection.IsSocketConnected());
        }

        [Fact]
        public void SendReceiveTest()
        {
            using (var server = new TestServer())
            {
                server.Start();

                var testData = Encoding.UTF8.GetBytes("Hello");
                server.connection.Send(testData);

                // Wait with timeout
                var timeout = TimeSpan.FromSeconds(5);
                var startTime = DateTime.UtcNow;

                while (server.messages.Count == 0)
                {
                    if (DateTime.UtcNow - startTime > timeout)
                    {
                        Assert.Fail($"Timed out waiting for message after {timeout.TotalSeconds} seconds");
                    }
                    Thread.Sleep(10);
                }

                Assert.Single(server.messages);
                Assert.Equal("Hello", server.messages[0]);
            }
        }
    }
}