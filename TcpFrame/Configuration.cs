using System.IO;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Channels;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable RedundantDefaultMemberInitializer
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace TcpFrame;

public class Configuration
{
    public IEventLoopGroup EventLoopGroup { get; set; } = new MultithreadEventLoopGroup();
    public General Shared { get; set; } = new();
    public Encoding Encoder { get; set; } = new();
    public Decoding Decoder { get; set; } = new();
    public X509Certificate2? Certificate { get; private set; }

    public class General
    {
        public ByteOrder ByteOrder { get; set; } = ByteOrder.BigEndian;
        public int LengthFieldLength { get; set; } = 4;
        public int LengthAdjustment { get; set; } = 0;
    }

    public class Encoding
    {
        public bool LengthFieldIncludesLengthFieldLength { get; set; } = false;
    }

    public class Decoding
    {
        public int MaxFrameLength { get; set; } = 8 * 1_024 * 1_024;
        public int LengthFieldOffset { get; set; } = 0;
        public int InitialBytesToStrip { get; set; } = 4;
        public bool FailFast { get; set; } = false;
    }

    internal LengthFieldPrepender GetEncoder() => new(
        Shared.ByteOrder,
        Shared.LengthFieldLength,
        Shared.LengthAdjustment,
        Encoder.LengthFieldIncludesLengthFieldLength);

    internal LengthFieldBasedFrameDecoder GetDecoder() => new(
        Shared.ByteOrder,
        Decoder.MaxFrameLength,
        Decoder.LengthFieldOffset,
        Shared.LengthFieldLength,
        Shared.LengthAdjustment,
        Decoder.InitialBytesToStrip,
        Decoder.FailFast);

    #region Certificate
    
    internal IChannelHandler GetEncryption()
    {
        var tlsOptions = new ServerTlsSettings(Certificate);
        return new TlsHandler(tlsOptions);
    }

    public async Task<bool> SetCertificateFromFileAsync(string filePath)
    {
        var result = await GetFileAsync(filePath);
        if (!result.Success)
        {
            return false;
        }

        Certificate = new X509Certificate2(filePath);
        return true;
    }

    public async Task<bool> SetCertificateFromFileAsync(string filePath, string password)
    {
        var result = await GetFileAsync(filePath);
        if (!result.Success)
        {
            return false;
        }

        Certificate = new X509Certificate2(filePath, password);
        return true;
    }

    public async Task<bool> SetCertificateFromFileAsync(string filePath, SecureString password)
    {
        var result = await GetFileAsync(filePath);
        if (!result.Success)
        {
            return false;
        }

        Certificate = new X509Certificate2(filePath, password);
        return true;
    }

    private async Task<(byte[]? Bytes, bool Success)> GetFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return (null, false);

        try
        {
            var bytes = await File.ReadAllBytesAsync(filePath);
            return (bytes, true);
        }
        catch
        {
            return (null, false);
        }
    }

    public void SetCertificate(byte[] rawData) => Certificate = new X509Certificate2(rawData);
    public void SetCertificate(byte[] rawData, string password) => Certificate = new X509Certificate2(rawData, password);
    public void SetCertificate(X509Certificate2 certificate) => Certificate = certificate;
    public void SetCertificate(nint handle) => Certificate = new X509Certificate2(handle);

    #endregion
}