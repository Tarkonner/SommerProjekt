using Client.Interfaces;
using System.Net.Sockets;

public class ClientProgram : INetworkClient, IThreadWrapper
{

    // --- Threads ---
    private INetworkClient networkClient;
    private IThreadWrapper receiveThread;
    private IThreadWrapper sendThread;

    private bool _running = true;

    static void Main()
    {

    }

    public bool Connect(string host, int port)
    {
        throw new NotImplementedException();
    }

    public Task<int> ReadAsync(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }

    public Task WriteAsync(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }

    public void Close()
    {
        throw new NotImplementedException();
    }

    public void Start(Action action)
    {
        throw new NotImplementedException();
    }

    public void Join()
    {
        throw new NotImplementedException();
    }
}

