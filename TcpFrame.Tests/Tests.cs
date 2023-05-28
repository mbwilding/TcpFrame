namespace TcpFrame.Tests;

public class Tests
{
    private (TcpFrameServer Server, TcpFrameClient Client) GetPair(ushort port)
    {
        const string host = "127.0.0.1";
        return (new TcpFrameServer{ Port = port }, new TcpFrameClient { Host = host, Port = port });
    }

    [Fact]
    public async Task Server()
    {
        var pair = GetPair(9001);
        
        Assert.True(await pair.Server.StartAsync());
        Assert.True(pair.Server.IsActive);

        await pair.Server.StopAsync();
        Assert.False(pair.Server.IsActive);
    }
    
    [Fact]
    public async Task Client()
    {
        var pair = GetPair(9002);
        
        Assert.False(await pair.Client.ConnectAsync());
        
        await pair.Server.StartAsync();
        Assert.True(await pair.Client.ConnectAsync());
        Assert.True(pair.Client.IsActive);

        await pair.Client.DisconnectAsync();
        Assert.False(pair.Client.IsActive);
        
        await pair.Server.StopAsync();
        Assert.False(pair.Client.IsActive);
    }
}