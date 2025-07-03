using Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionTests
{
    public class TestServer : IDisposable
    {
        private readonly TcpListener _listener;
        public TcpConnection connection;
        private readonly CancellationTokenSource _cts;
        private Task _messageHandlingTask;

        public List<string> messages { get; private set; } = new List<string>();

        public TestServer()
        {
            _listener = new TcpListener(IPAddress.Loopback, 50000);
            connection = new TcpConnection();
            _cts = new CancellationTokenSource();
        }

        public void Start()
        {
            _listener.Start();
            connection.Connect("127.0.0.1", 50000);

            //Sending message
            _messageHandlingTask = Task.Run(async () =>
            {
                while (_listener.Server.IsBound && !_cts.Token.IsCancellationRequested)
                {
                    if (_cts.Token.IsCancellationRequested)
                        break;

                    var client = await _listener.AcceptTcpClientAsync(_cts.Token);
                    using (var stream = client.GetStream())
                    {
                        var buffer = new byte[1024];
                        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, _cts.Token);
                        messages.Add(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                    }
                }
            }, _cts.Token);
        }

        public void Stop()
        {
            _listener.Stop();
            connection.DisposeAsync();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
