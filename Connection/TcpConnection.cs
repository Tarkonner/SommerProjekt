using System.Net.Sockets;

namespace Connection
{
    public class TcpConnection : IConnection
    {
        public NetworkStream stream { get; private set; }
        public TcpClient client { get; private set; }


        public bool IsSocketConnected()
        {
            try
            {
                Socket socket = client.Client;
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task Connect(string host, int port)
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync(host, port);
                stream = client.GetStream();
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to connect to {host}:{port}", e);
            }
        }

        public async Task SendAsync(byte[] data)
        {
            if (stream == null)
                throw new InvalidOperationException("Not connected");

            await stream.WriteAsync(data, 0, data.Length);
            await stream.FlushAsync();
        }

        public async Task<byte[]> ReceiveAsync(int bufferSize)
        {
            if (stream == null)
                throw new InvalidOperationException("Not connected");

            var buffer = new byte[bufferSize];
            int bytesRead = await stream.ReadAsync(buffer, 0, bufferSize);

            if (bytesRead == 0)
            {
                // Connection closed by remote host
                return Array.Empty<byte>();
            }

            // Return the exact data read
            if (bytesRead == bufferSize)
                return buffer;

            var result = new byte[bytesRead];
            Array.Copy(buffer, result, bytesRead);
            return result;
        }

        public async ValueTask DisposeAsync()
        {
            if (client?.Connected ?? false)
            {
                client.Close();
                stream = null;
                client = null;
            }

            if (stream != null)
            {
                await stream.DisposeAsync();
                stream = null;
            }

            if (client != null)
            {
                client.Close();     // Gracefully close connection
                client.Dispose();   // Dispose unmanaged resources
                client = null;
            }
        }
    }
}
