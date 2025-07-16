using Server;

namespace E2E_Testing
{
    public class ConnectionTests
    {
        [Fact]
        public async Task ServerAndClientConnect()
        {
            TcpServer tcpServer = new TcpServer();
            ClientProgram clientProgram = new ClientProgram();

            await tcpServer.StartAsync();

            

            Assert.True(false);
        }
    }
}