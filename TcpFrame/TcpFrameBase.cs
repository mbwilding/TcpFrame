using System;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;

// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace TcpFrame;

public abstract class TcpFrameBase : IAsyncDisposable
{
    internal readonly ILogger? Logger;
    internal readonly IEventLoopGroup Group;
    public IChannel? Channel;

    public ushort Port { get; set; } = 9000;

    public bool IsActive => Channel is {Active: true};
    public Configuration Config { get; set; } = new();

    protected TcpFrameBase(ILogger? logger = null)
    {
        Logger = logger;
        Group = Config.EventLoopGroup;
    }

    public async ValueTask DisposeAsync()
    {
        await Group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1))
            .ConfigureAwait(false);
    }
}