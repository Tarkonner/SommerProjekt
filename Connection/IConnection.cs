using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connection
{
    public interface IConnection : IAsyncDisposable
    {
        Task SendAsync(byte[] data);
        Task<byte[]> ReceiveAsync(int bufferSize);
    }
}
