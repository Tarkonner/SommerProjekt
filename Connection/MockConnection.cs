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

        // Async Connect
        public Task Connect(string host, int port)
        {
            IsConnected = true;
            return Task.CompletedTask;
        }

        // Async Send
        public Task SendAsync(byte[] data)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Not connected");

            SentData.Add(data);
            return Task.CompletedTask;
        }

        // Async Receive
        public Task<byte[]> ReceiveAsync(int bufferSize)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Not connected");

            return Task.FromResult(LastReceivedData ?? Array.Empty<byte>());
        }

        // Async Dispose
        public ValueTask DisposeAsync()
        {
            IsDisposed = true;
            IsConnected = false;
            return ValueTask.CompletedTask;
        }
    }
}
