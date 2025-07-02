using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connection
{
    public interface IConnection
    {
        void Connect(string host, int port);
        void Send(byte[] data);
        byte[] Receive(int bufferSize);
        void Disconnect();
    }
}
