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
    public class TestServer : IAsyncDisposable
    {
        private bool running = true;

        private TcpListener _listener;
        public List<TcpConnection> clients = new List<TcpConnection>();

        Thread accpetThread;


        public List<string> messages { get; private set; } = new List<string>();


        public Task StartListeningAsync(int port)
        {
            _listener = new TcpListener(IPAddress.Loopback, port);
            _listener.Start();

            accpetThread = new Thread(AcceptClients);
            accpetThread.Start();

            return Task.CompletedTask;
        }

        public async void AcceptClients()
        {
            while(running)
            {
                // Accept client asynchronously
                TcpClient tcpClient = await _listener.AcceptTcpClientAsync();

                // Wrap in TcpConnection
                var connection = new TcpConnection(tcpClient);

                // Track connection if needed
                clients.Add(connection);

                // Handle client asynchronously (no new thread, just a background task)
                _ = HandleClientAsync(connection);
            }
        }

        private async Task HandleClientAsync(TcpConnection connection)
        {
            try
            {
                var data = await connection.ReceiveAsync(1024);

                string message = Encoding.UTF8.GetString(data);
                messages.Add(message);

                var response = Encoding.UTF8.GetBytes(message);
                await connection.SendAsync(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");
            }
            finally
            {
                await connection.DisposeAsync();
                clients.Remove(connection);
                Console.WriteLine("Client disconnected.");
            }
        }

        public async ValueTask DisposeAsync()
        {
            running = false;

            foreach (TcpConnection item in clients)
            {
                await item.DisposeAsync();
            }

            _listener.Dispose();
            accpetThread.Join();
        }
    }
}
