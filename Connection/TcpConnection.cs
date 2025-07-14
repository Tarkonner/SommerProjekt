using System.Net.Sockets;

namespace Connection
{
    public class TcpConnection : IConnection
    {
        public event EventHandler<TcpConnectionEventArgs>? goingToDisconnect;

        public NetworkStream stream { get; private set; }
        public TcpClient client { get; private set; }


        public TcpConnection(TcpClient existingClient)
        {
            client = existingClient;
            stream = client.GetStream();
        }

        public TcpConnection()
        {
        }

        public async Task Connect(string host, int port)
        {
            if (client != null)
                throw new InvalidOperationException("Already connected.");

            try
            {
                client = new TcpClient();
                await client.ConnectAsync(host, port);
                stream = client.GetStream();
            }
            catch (Exception ex)
            {
                goingToDisconnect?.Invoke(this, new TcpConnectionEventArgs(this));
                throw new Exception(ex.Message);
            }
        }


        public async Task SendAsync(byte[] data)
        {
            try
            {
                if (stream == null)
                    throw new InvalidOperationException("Not connected");

                await stream.WriteAsync(data, 0, data.Length);
                await stream.FlushAsync();
            }
            catch (Exception ex)
            {
                goingToDisconnect?.Invoke(this, new TcpConnectionEventArgs(this));
                throw new Exception(ex.Message);
            }
        }

        public async Task<byte[]> ReceiveAsync(int bufferSize)
        {
            if (stream == null)
                throw new InvalidOperationException("Not connected");

            try
            {
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
            catch (Exception ex)
            {
                goingToDisconnect?.Invoke(this, new TcpConnectionEventArgs(this));
                throw new Exception(ex.Message);
            }
        }


        public async ValueTask DisposeAsync()
        {
            goingToDisconnect?.Invoke(this, new TcpConnectionEventArgs(this));

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

    public class TcpConnectionEventArgs : EventArgs
    {
        public TcpConnection Connection { get; }

        public TcpConnectionEventArgs(TcpConnection connection)
        {
            Connection = connection;
        }
    }
}
