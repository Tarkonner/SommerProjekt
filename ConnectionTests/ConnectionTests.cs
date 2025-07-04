using Connection;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ConnectionTests
{
    public class ConnectionTests
    {
        private int GetAvailablePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        [Fact]
        public async Task ConnectToTestServerHasMessage()
        {
            int port = GetAvailablePort();
            var testServer = new TestServer();
            var tcpConnection = new TcpConnection();

            try
            {
                await testServer.StartListeningAsync(port);
                await tcpConnection.Connect("127.0.0.1", port);

                string testMessage = "Hello from client";
                byte[] messageBytes = Encoding.UTF8.GetBytes(testMessage);

                // Act
                await tcpConnection.SendAsync(messageBytes);
                await Task.Delay(500); // Give the server time to process

                // Assert
                Assert.Equal(testMessage, testServer.messages[0]);
            }
            finally
            {
                await tcpConnection.DisposeAsync();
                await testServer.DisposeAsync();
            }
        }

        [Fact]
        public async Task GetMessageFromTestserver()
        {
            int port = GetAvailablePort();
            var testServer = new TestServer();
            var tcpConnection = new TcpConnection();

            try
            {
                await testServer.StartListeningAsync(port);
                await tcpConnection.Connect("127.0.0.1", port);

                string testMessage = "Hello from client";
                byte[] messageBytes = Encoding.UTF8.GetBytes(testMessage);

                await tcpConnection.SendAsync(messageBytes);
                var data = await tcpConnection.ReceiveAsync(1024);
                string message = Encoding.UTF8.GetString(data);

                Assert.Equal(testMessage, message);
            }
            finally
            {
                await tcpConnection.DisposeAsync();
                await testServer.DisposeAsync();
            }
        }

        [Fact]
        public async Task ConnectToInvalidPort()
        {
            var connection = new TcpConnection();
            int port = GetAvailablePort() + 10000; // Ensure port is unused

            await Assert.ThrowsAsync<Exception>(() =>
                connection.Connect("127.0.0.1", port));
        }

        [Fact]
        public async Task ConnectTwice_ThrowsInvalidOperationException()
        {
            int port = GetAvailablePort();
            var testServer = new TestServer();
            var connection = new TcpConnection();

            try
            {
                await testServer.StartListeningAsync(port);
                await connection.Connect("127.0.0.1", port);

                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    connection.Connect("127.0.0.1", port));
            }
            finally
            {
                await connection.DisposeAsync();
                await testServer.DisposeAsync();
            }
        }

        [Fact]
        public async Task SendWithoutConnecting_ThrowsInvalidOperationException()
        {
            var connection = new TcpConnection();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                connection.SendAsync(Encoding.UTF8.GetBytes("Test")));
        }

        [Fact]
        public async Task ReceiveWithoutConnecting_ThrowsInvalidOperationException()
        {
            var connection = new TcpConnection();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                connection.ReceiveAsync(1024));
        }

        [Fact]
        public async Task ReceiveAfterServerDisconnect_ReturnsEmptyArray()
        {
            int port = GetAvailablePort();
            var testServer = new TestServer();
            var connection = new TcpConnection();

            try
            {
                await testServer.StartListeningAsync(port);
                await connection.Connect("127.0.0.1", port);

                await testServer.DisposeAsync(); // simulate server disconnect

                var result = await connection.ReceiveAsync(1024);
                Assert.Empty(result);
            }
            finally
            {
                await connection.DisposeAsync();
            }
        }
    }
}