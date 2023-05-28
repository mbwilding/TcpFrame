using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace TcpFrame;

public class TcpFrameServer : TcpFrameBase
{
    private readonly ServerBootstrap _bootstrap;

    public readonly List<IChannel> ClientChannels = new();

    public event Action? Started;
    public event Action? Stopped;
    public event Action<IChannel>? ClientConnected;
    public event Action<IChannel>? ClientDisconnected;
    public event Action<IChannel, byte[]>? MessageReceived;

    public TcpFrameServer(ILogger<TcpFrameServer>? logger = null) : base(logger)
    {
        _bootstrap = new ServerBootstrap()
            .Group(Group)
            .Channel<TcpServerSocketChannel>()
            .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
            {
                IChannelPipeline pipeline = channel.Pipeline;
                if (Config.Certificate != null)
                    pipeline.AddLast("tls", Config.GetEncryption());
                pipeline.AddLast("framing-enc", Config.GetEncoder());
                pipeline.AddLast("framing-dec", Config.GetDecoder());
                pipeline.AddLast("handler", new TcpHandlerServer(this));
            }));
    }

    public async Task<bool> StartAsync()
    {
        try
        {
            Channel = await _bootstrap.BindAsync(Port).ConfigureAwait(false);
            Started?.Invoke();
            Logger?.LogDebug("Server started: {Port}", Port);
            return true;
        }
        catch (Exception ex)
        {
            Stopped?.Invoke();
            Logger?.LogError(ex, "Failed to start server: {Port} | {Error}", Port, ex.Message);
            return false;
        }
    }

    public async Task StopAsync()
    {
        try
        {
            if (Channel != null)
            {
                await Channel.CloseAsync().ConfigureAwait(false);
                await Channel.CloseCompletion.ConfigureAwait(false);
                Channel = null;
                Stopped?.Invoke();
                Logger?.LogDebug("Server stopped");
            }
            else
            {
                Logger?.LogWarning("Server is already stopped");
            }
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Failed to stop server");
        }
    }

    public async Task SendAsync(IChannel channel, byte[] data)
    {
        if (!channel.Active)
        {
            Logger?.LogWarning("Cannot send dat as channel is not active");
            return;
        }

        try
        {
            IByteBuffer buffer = Unpooled.WrappedBuffer(data);
            await channel.WriteAndFlushAsync(buffer).ConfigureAwait(false);
            Logger?.LogDebug("Sent {IpAddress} | {Bytes} bytes", channel.RemoteAddress.ToString(), data.Length);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Failed to send data");
        }
    }

    public async Task SendToAllAsync(byte[] data)
    {
        var sendTasks = ClientChannels.Select(channel => SendAsync(channel, data));
        await Task.WhenAll(sendTasks).ConfigureAwait(false);
    }

    private class TcpHandlerServer : SimpleChannelInboundHandler<IByteBuffer>
    {
        private readonly TcpFrameServer _tcpFrame;

        public TcpHandlerServer(TcpFrameServer tcpFrame)
        {
            _tcpFrame = tcpFrame;
        }

        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            var channel = ctx.Channel;
            _tcpFrame.ClientChannels.Add(channel);
            base.ChannelActive(ctx);
            _tcpFrame.ClientConnected?.Invoke(channel);
            _tcpFrame.Logger?.LogDebug("Connected {IpAddress}", channel.RemoteAddress.ToString());
        }

        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            var channel = ctx.Channel;
            _tcpFrame.ClientChannels.Remove(channel);
            base.ChannelInactive(ctx);
            _tcpFrame.ClientDisconnected?.Invoke(channel);
            _tcpFrame.Logger?.LogDebug("Disconnected {IpAddress}", channel.RemoteAddress.ToString());
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, IByteBuffer msg)
        {
            var channel = ctx.Channel;
            var byteArray = new byte[msg.ReadableBytes];
            msg.ReadBytes(byteArray);
            _tcpFrame.MessageReceived?.Invoke(channel, byteArray);
            _tcpFrame.Logger?.LogDebug("Received {IpAddress} | {Bytes} bytes", channel.RemoteAddress.ToString(),
                msg.ReadableBytes);
        }

        public override async void ExceptionCaught(IChannelHandlerContext ctx, Exception ex)
        {
            await ctx.CloseAsync().ConfigureAwait(false);
            // _tcpFrame.Logger?.LogError(ex, "Read failure");
        }
    }

    public new async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        await base.DisposeAsync().ConfigureAwait(false);
    }
}
