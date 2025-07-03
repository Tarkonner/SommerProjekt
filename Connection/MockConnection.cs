using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connection
{
    public class MockConnection : IConnection
    {
        public bool IsConnected { get; private set; }
        public List<byte[]> SentData { get; } = new();
        public byte[] LastReceivedData { get; set; }
        public bool IsDisposed { get; private set; }

        public void Connect(string host, int port)
        {
            IsConnected = true;
        }

        public void Send(byte[] data)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Not connected");

            SentData.Add(data);
        }

        public byte[] Receive(int bufferSize)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Not connected");

            return LastReceivedData ?? Array.Empty<byte>();
        }

        public void Disconnect()
        {
            IsConnected = false;
        }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
