using CommenCompunents;
using Connection;
using Server;
using System.Text;

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
            await Task.Delay(100);
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
            await Task.Delay(100);
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

            Thread.Sleep(100);

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

            Thread.Sleep(100);

            // Assert - Check thread states before disposal
            Assert.False(server.serverThread.IsAlive);
            Assert.True(server.acceptTask.IsCompleted);
            Assert.True(server.broadcastTask.IsCompleted);
            Assert.True(server.heartbeat.heartbeatTask.IsCompleted);

            // Cleanup
            await server.DisposeAsync();
        }

        [Fact]
        async Task BroadcastMessageToOneClient()
        {
            string message = "Hello world";

            TcpServer tcpServer = new TcpServer();
            await tcpServer.StartAsync();

            TcpConnection client = new TcpConnection();
            await client.Connect(tcpServer.defaultPortLocalAddress.ToString(), tcpServer.Port);

            await tcpServer.BroadcastMessage(message);

            byte[] gottenMessage = await client.ReceiveAsync();
        }

        [Fact]
        async Task BroadcastMessageToTwoClient()
        {
            string message = "Hello world";

            TcpServer tcpServer = new TcpServer();
            await tcpServer.StartAsync();

            TcpConnection c1 = new TcpConnection();
            await c1.Connect(tcpServer.defaultPortLocalAddress.ToString(), tcpServer.Port);
            TcpConnection c2 = new TcpConnection();
            await c2.Connect(tcpServer.defaultPortLocalAddress.ToString(), tcpServer.Port);


            var receiveTasks = new[]
            {
                c1.ReceiveAsync(),
                c2.ReceiveAsync()
            };

            await tcpServer.BroadcastMessage(message);

            byte[][] payloads = await Task.WhenAll(receiveTasks);

            Assert.All(payloads,
                bytes => Assert.Equal(message, Encoding.UTF8.GetString(bytes)));
        }

        [Fact]
        public async Task RemoveDisconnectetClient()
        {
            Assert.True(false);
        }

        [Fact]
        public async Task ReconnectDisconnectClient()
        {
            Assert.True(false);
        }

        [Fact]
        public async Task HearHearthbeat()
        {
            TcpServer tcpServer = new TcpServer();
            await tcpServer.StartAsync();

            int listenTimeMul = 5;
            int beats = 0;
            int maxExpectedBeats = listenTimeMul + 1;

            tcpServer.heartbeat.OnHeartbeat += () => { beats++; };

            await Task.Delay(tcpServer.heartbeat.heartbeatIntervalInMiliseconds * listenTimeMul + 200);

            Assert.True(beats <= maxExpectedBeats,
                $"Expected {listenTimeMul} or {listenTimeMul + 1} beats, but got {beats}");

            await tcpServer.Stop();
        }
    }
}