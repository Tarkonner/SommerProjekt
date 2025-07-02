using System.Net.Sockets;

namespace Connection
{
    public class TcpConnection : IConnection
    {
        public readonly Socket _socket;

        public TcpConnection()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }


        public void Connect(string host, int port)
        {
            try
            {
                _socket.Connect(host, port);
            }
            catch (SocketException e)
            {
                throw new Exception($"Failed to connect to {host}:{port}", e);
            }
        }

        public void Disconnect()
        {
            _socket?.Close();
        }

        public byte[] Receive(int bufferSize)
        {
            if (!IsSocketConnected())
                throw new InvalidOperationException("Not connected to server");

            try
            {
                var buffer = new byte[bufferSize];
                int totalBytesReceived = 0;

                // Receive data in chunks until no more data is available
                while (_socket.Poll(1000, SelectMode.SelectRead))
                {
                    if (totalBytesReceived == buffer.Length)
                    {
                        Array.Resize(ref buffer, buffer.Length * 2);
                    }

                    int bytesRead = _socket.Receive(buffer, totalBytesReceived, buffer.Length - totalBytesReceived, SocketFlags.None);

                    if (bytesRead == 0)
                        throw new SocketException((int)SocketError.ConnectionReset);

                    totalBytesReceived += bytesRead;
                }

                // Trim the buffer to actual received size
                byte[] result = new byte[totalBytesReceived];
                Array.Copy(buffer, result, totalBytesReceived);
                return result;
            }
            catch (SocketException ex)
            {
                throw new Exception("Failed to receive data", ex);
            }
        }

        public void Send(byte[] data)
        {
            if (!IsSocketConnected())
                throw new InvalidOperationException("Not connected to server");

            try
            {
                // Send the entire data array
                int totalBytesSent = 0;
                while (totalBytesSent < data.Length)
                {
                    int bytesSent = _socket.Send(data, totalBytesSent, data.Length - totalBytesSent, SocketFlags.None);
                    if (bytesSent == 0)
                        throw new SocketException((int)SocketError.ConnectionReset);
                    totalBytesSent += bytesSent;
                }
            }
            catch (SocketException ex)
            {
                throw new Exception("Failed to send data", ex);
            }
        }

        public bool IsSocketConnected()
        {
            try
            {
                return !(_socket.Poll(1, SelectMode.SelectRead) && _socket.Available == 0);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
