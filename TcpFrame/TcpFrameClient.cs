using System;
using System.Linq;
using System.Net;
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
// ReSharper disable RedundantDefaultMemberInitializer
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace TcpFrame;

public class TcpFrameClient : TcpFrameBase
{
    private readonly Bootstrap _bootstrap;

    public event Action? Connected;
    public event Action? Disconnected;
    public event Action<byte[]>? MessageReceived;

    public string Host { get; set; } = "127.0.0.1";

    public bool AutoReconnect { get; set; } = true;
    public int ReconnectDelay { get; set; } = 1000;
    public int ReconnectInitialDelay { get; set; } = 0;

    public TcpFrameClient(ILogger<TcpFrameClient>? logger = null) : base(logger)
    {
        _bootstrap = new Bootstrap()
            .Group(Group)
            .Channel<TcpSocketChannel>()
            .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
            {
                IChannelPipeline pipeline = channel.Pipeline;
                if (Config.Certificate != null)
                {
                    // pipeline.AddLast("tls", TlsHandler.Client("127.0.0.1", Config.Certificate));
                }
                pipeline.AddLast("framing-enc", Config.GetEncoder());
                pipeline.AddLast("framing-dec", Config.GetDecoder());
                pipeline.AddLast("handler", new TcpHandlerClient(this));
            }));
    }

    public async Task<bool> ConnectAsync(string host, ushort port)
    {
        Host = host;
        Port = port;
        return await ConnectAsync();
    }
    
    public async Task<bool> ConnectAsync()
    {
        try
        {
            var ipAddress = await GetIPv4AddressAsync(Host);
            if (ipAddress == null)
            {
                Logger?.LogError("Cannot resolve hostname to IP address: {Host}", Host);
                return false;
            }

            Logger?.LogDebug("Attempting connection: {Host}:{Port}", ipAddress, Port);
            Channel = await _bootstrap.ConnectAsync(ipAddress, Port).ConfigureAwait(false);
            return true;
        }
        catch (Exception)
        {
            Logger?.LogWarning("Failed to connect: {Host}:{Port}", Host, Port);
            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            if (Channel != null)
            {
                await Channel.CloseAsync().ConfigureAwait(false);
                await Channel.CloseCompletion.ConfigureAwait(false);
                Channel = null;
            }
            else
            {
                Logger?.LogWarning("Channel is already null");
            }
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Failed to disconnect");
        }
    }

    public async Task SendAsync(byte[] message)
    {
        IByteBuffer buffer = Unpooled.WrappedBuffer(message);

        if (!IsActive)
        {
            Logger?.LogWarning("Not sending as not connected");
            return;
        }

        if (Channel != null)
        {
            await Channel.WriteAndFlushAsync(buffer).ConfigureAwait(false);
            Logger?.LogDebug("Sent: {Bytes} bytes", message.Length);
        }
        else
        {
            Logger?.LogError("Channel is null");
        }
    }

    private class TcpHandlerClient : SimpleChannelInboundHandler<IByteBuffer>
    {
        private readonly TcpFrameClient _tcpFrame;

        public TcpHandlerClient(TcpFrameClient tcpFrame)
        {
            _tcpFrame = tcpFrame;
        }

        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            var channel = ctx.Channel;
            base.ChannelActive(ctx);
            _tcpFrame.Connected?.Invoke();
            _tcpFrame.Logger?.LogDebug("Connected {IpAddress}", channel.RemoteAddress.ToString());
        }

        public override async void ChannelInactive(IChannelHandlerContext ctx)
        {
            var channel = ctx.Channel;
            base.ChannelInactive(ctx);
            _tcpFrame.Disconnected?.Invoke();
            _tcpFrame.Logger?.LogDebug("Disconnected {IpAddress}", channel.RemoteAddress.ToString());

            if (_tcpFrame.AutoReconnect)
            {
                await Task.Delay(_tcpFrame.ReconnectInitialDelay).ConfigureAwait(false);

                while (!_tcpFrame.IsActive)
                {
                    _tcpFrame.Logger?.LogDebug("Attempting reconnection");
                    await _tcpFrame.ConnectAsync().ConfigureAwait(false);
                    await Task.Delay(_tcpFrame.ReconnectDelay).ConfigureAwait(false);
                }
            }
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, IByteBuffer msg)
        {
            var channel = ctx.Channel;
            var byteArray = new byte[msg.ReadableBytes];
            msg.ReadBytes(byteArray);
            _tcpFrame.MessageReceived?.Invoke(byteArray);
            _tcpFrame.Logger?.LogDebug("Received {IpAddress} | {Bytes} bytes", channel.RemoteAddress.ToString(),
                msg.ReadableBytes);
        }

        public override async void ExceptionCaught(IChannelHandlerContext ctx, Exception ex)
        {
            await ctx.CloseAsync().ConfigureAwait(false);
            // _tcpFrame.Logger?.LogError(ex, "Read failure");
        }
    }

    private async Task<IPAddress?> GetIPv4AddressAsync(string host)
    {
        if (string.Equals(Host, Dns.GetHostName(), StringComparison.CurrentCultureIgnoreCase))
        {
            return IPAddress.Parse("127.0.0.1");
        }
        
        var addresses = await Dns.GetHostAddressesAsync(host).ConfigureAwait(false);
        var address = addresses.FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        return address;
    }
    
    public new async ValueTask DisposeAsync()
    {
        await DisconnectAsync().ConfigureAwait(false);
        await base.DisposeAsync().ConfigureAwait(false);
    }
}
