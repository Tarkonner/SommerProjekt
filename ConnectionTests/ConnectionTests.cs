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
            var testServer = new ServerToConnectionTest();
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
            var testServer = new ServerToConnectionTest();
            var tcpConnection = new TcpConnection();

            try
            {
                await testServer.StartListeningAsync(port);
                await tcpConnection.Connect("127.0.0.1", port);

                string testMessage = "Hello from client";
                byte[] messageBytes = Encoding.UTF8.GetBytes(testMessage);

                await tcpConnection.SendAsync(messageBytes);
                var data = await tcpConnection.ReceiveAsync();
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
            var testServer = new ServerToConnectionTest();
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

            await Assert.ThrowsAsync<Exception>(() =>
                connection.SendAsync(Encoding.UTF8.GetBytes("Test")));
        }

        [Fact]
        public async Task ReceiveWithoutConnecting_ThrowsInvalidOperationException()
        {
            var connection = new TcpConnection();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                connection.ReceiveAsync());
        }

        [Fact]
        public async Task ReceiveAfterServerDisconnect_ReturnsEmptyArray()
        {
            int port = GetAvailablePort();
            var testServer = new ServerToConnectionTest();
            var connection = new TcpConnection();

            try
            {
                await testServer.StartListeningAsync(port);
                await connection.Connect("127.0.0.1", port);

                await connection.DisposeAsync();

                var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    connection.ReceiveAsync());

                Assert.Equal("Connection canceled", ex.Message);
            }
            finally
            {
                await connection.DisposeAsync();
            }
        }

        [Fact]
        public async Task Connect_PerformsSuccessfulHandshake()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;

            // Server task: simulate correct handshake
            var serverTask = Task.Run(async () =>
            {
                using var serverClient = await listener.AcceptTcpClientAsync();
                using var stream = serverClient.GetStream();

                byte[] expectedClientMagic = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
                byte[] response = new byte[] { 0xCA, 0xFE, 0xBA, 0xBE };

                var buffer = new byte[expectedClientMagic.Length];
                int offset = 0;
                while (offset < buffer.Length)
                    offset += await stream.ReadAsync(buffer, offset, buffer.Length - offset);

                // Optional: assert client sent correct handshake
                Assert.Equal(expectedClientMagic, buffer);

                await stream.WriteAsync(response, 0, response.Length);
                await stream.FlushAsync();
            });

            var client = new TcpConnection();
            await client.Connect("127.0.0.1", port);

            await client.DisposeAsync();
            listener.Stop();
            await serverTask;
        }

        [Fact]
        public async Task Connect_ThrowsIfServerHandshakeFails()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;

            // Server sends wrong response
            var serverTask = Task.Run(async () =>
            {
                using var serverClient = await listener.AcceptTcpClientAsync();
                using var stream = serverClient.GetStream();

                byte[] dummy = new byte[4];
                await stream.ReadAsync(dummy, 0, dummy.Length); // just read whatever client sends
                await stream.WriteAsync(new byte[] { 0x00, 0x00, 0x00, 0x00 }); // wrong response
                await stream.FlushAsync();
            });

            var client = new TcpConnection();

            var ex = await Assert.ThrowsAsync<IOException>(() => client.Connect("127.0.0.1", port));
            Assert.Contains("Invalid handshake response", ex.Message);

            await client.DisposeAsync();
            listener.Stop();
            await serverTask;
        }
    }
}