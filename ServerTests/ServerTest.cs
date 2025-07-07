using Connection;
using Server;

namespace ServerTests
{
    public class ServerTest
    {
        private async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 2000, int pollIntervalMs = 50)
        {
            var start = DateTime.UtcNow;
            while (!condition())
            {
                if ((DateTime.UtcNow - start).TotalMilliseconds > timeoutMs)
                    throw new TimeoutException("Condition not met in time.");
                await Task.Delay(pollIntervalMs);
            }
        }

        [Fact]
        async Task ClientsCanConnect()
        {
            TcpServer tcpServer = new TcpServer();
            await tcpServer.StartAsync();

            TcpConnection client = new TcpConnection();
            await client.Connect(tcpServer.defaultPortLocalAddress.ToString(), tcpServer.Port);

            await WaitUntilAsync(() => tcpServer.numberOfClient == 1);

            Assert.Equal(1, tcpServer.numberOfClient);

            await tcpServer.DisposeAsync();
        }

        [Fact]
        async Task MultipulClientConnect()
        {
            TcpServer tcpServer = new TcpServer();
            await tcpServer.StartAsync();

            TcpConnection client1 = new TcpConnection();
            await client1.Connect(tcpServer.defaultPortLocalAddress.ToString(), tcpServer.Port);
            TcpConnection client2 = new TcpConnection();
            await client2.Connect(tcpServer.defaultPortLocalAddress.ToString(), tcpServer.Port);

            await WaitUntilAsync(() => tcpServer.numberOfClient == 2);

            Assert.Equal(2, tcpServer.numberOfClient);

            await tcpServer.DisposeAsync();
        }
        [Fact]
        async Task ClientCanDisconnect()
        {
            TcpServer tcpServer = new TcpServer();
            await tcpServer.StartAsync();

            TcpConnection client = new TcpConnection();
            await client.Connect(tcpServer.defaultPortLocalAddress.ToString(), tcpServer.Port);

            await client.DisposeAsync();

            Assert.Equal(0, tcpServer.numberOfClient);

            await tcpServer.DisposeAsync();
        }

        [Fact]
        public async Task AllThreadsShouldClose()
        {
            // Arrange
            var server = new TcpServer();
            await server.StartAsync();

            // Act
            await server.Stop();

            // Assert - Check thread states before disposal
            Assert.False(server.serverThread.IsAlive);
            Assert.False(server.acceptThread.IsAlive);
            Assert.False(server.broadcastThread.IsAlive);

            // Cleanup
            await server.DisposeAsync();
        }
    }
}