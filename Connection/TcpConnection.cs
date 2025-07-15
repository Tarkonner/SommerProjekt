using System.Net.Sockets;

namespace Connection
{
    public class TcpConnection : IConnection
    {
        const int dataPipeSize = 4;

        public event EventHandler<TcpConnectionEventArgs>? goingToDisconnect;

        public CancellationTokenSource cancelToken { get; private set; } = new();

        public NetworkStream stream { get; private set; }
        public TcpClient client { get; private set; }

        private byte[] reciveBuffer;


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


        public async Task SendAsync(byte[] payload)
        {
            try
            {
                if(cancelToken.IsCancellationRequested)
                    throw new InvalidOperationException("Connection canclet");

                if (stream == null)
                    throw new InvalidOperationException("Not connected");

                //How long the messege to sent is.
                //So it know how long the sent message is
                var lenBuf = BitConverter.GetBytes(payload.Length);
                await stream.WriteAsync(lenBuf, 0, dataPipeSize);

                await stream.WriteAsync(payload, 0, payload.Length);
                await stream.FlushAsync();
            }
            catch (Exception ex)
            {
                goingToDisconnect?.Invoke(this, new TcpConnectionEventArgs(this));
                throw new Exception(ex.Message);
            }
        }

        public async Task<byte[]> ReceiveAsync()
        {
            if (stream == null)
                throw new InvalidOperationException("Not connected");

            if (cancelToken.IsCancellationRequested)
                throw new InvalidOperationException("Connection canclet");

            try
            {
                //How long the sent message is
                if(reciveBuffer == null || reciveBuffer.Length < dataPipeSize)
                    reciveBuffer = new byte[dataPipeSize];

                //Read the 4‑byte length prefix
                await ReadExactAsync(dataPipeSize);
                var lenBuf = reciveBuffer;                // see helper below
                int msgLen = BitConverter.ToInt32(lenBuf, 0);
                if (msgLen < 0) throw new IOException("Negative length in frame");

                // Enforce an upper bound so a rogue length can’t eat all memory
                const int MaxFrameSize = 1 * 1024 * 1024;   // 1 MB, pick what fits your app
                if (msgLen > MaxFrameSize)
                    throw new IOException($"Frame too large: {msgLen} bytes");

                // 2. Make sure the scratch buffer is big enough 
                if (reciveBuffer == null || reciveBuffer.Length < msgLen)
                    reciveBuffer = new byte[msgLen];

                // 3. Read the payload
                await ReadExactAsync(msgLen);

                // 4. Return an exact‑sized copy
                var payload = new byte[msgLen];
                Buffer.BlockCopy(reciveBuffer, 0, payload, 0, msgLen);
                return payload;
            }
            catch (Exception ex)
            {
                goingToDisconnect?.Invoke(this, new TcpConnectionEventArgs(this));
                throw new Exception(ex.Message);
            }
        }

        /// <summary>Reads exactly <paramref name="count"/> bytes into the internal scratch buffer.</summary>
        private async Task ReadExactAsync(int count)
        {
            if (reciveBuffer == null || reciveBuffer.Length < count)
                reciveBuffer = new byte[count];        // reuse to avoid many allocations

            int off = 0;
            while (off < count)
            {
                int n = await stream.ReadAsync(reciveBuffer, off, count - off);
                if (n == 0) throw new IOException("Connection closed prematurely");
                off += n;
            }
        }

        public async ValueTask DisposeAsync()
        {
            cancelToken.Cancel();

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
