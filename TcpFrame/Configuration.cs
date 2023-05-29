using System.Security.Cryptography.X509Certificates;
using DotNetty.Buffers;
using DotNetty.Codecs;
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
    public X509Certificate2? Certificate { get; set; }

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

    // public void SetCertificateFromPemFile(string filePath) => Certificate = X509Certificate2.CreateFromPemFile(filePath);
    // public void SetCertificateFromPemFiles(string certPath, string keyPath) => Certificate = X509Certificate2.CreateFromPemFile(certPath, keyPath);

    #endregion
}
