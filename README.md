# TcpFrame C# Library

TcpFrame is a C# library that provides event-driven TCP framing capabilities. It simplifies the process of handling framed data over TCP connections, allowing you to focus on processing the received frames.

## Installation

To use TcpFrame in your C# project, you'll need to pull down the repository and add a reference to the library.

## Usage

Check the examples folder for a working example.

### Client

```csharp
var tcpFrame = new TcpFrameClient();
await tcpFrame.ConnectAsync();
```

### Client Advanced (Optional ILogger)

```csharp
var tcpFrame = new TcpFrameClient(logger)
{
    Host = "127.0.0.1",
    Port = 9000,
    AutoReconnect = true,
    ReconnectDelay = 1000,
    ReconnectInitialDelay = 0,
    Config = new Configuration
    {
        EventLoopGroup = new MultithreadEventLoopGroup(),
        Shared = new Configuration.General
        {
            ByteOrder = ByteOrder.BigEndian,
            LengthFieldLength = 4,
            LengthAdjustment = 0
        },
        Encoder = new Configuration.Encoding
        {
            LengthFieldIncludesLengthFieldLength = false
        },
        Decoder = new Configuration.Decoding
        {
            MaxFrameLength = 8 * 1_024 * 1_024,
            LengthFieldOffset = 0,
            InitialBytesToStrip = 4,
            FailFast = false
        }
    }
};

await tcpFrame.ConnectAsync();
```

### Server

```csharp
var tcpFrame = new TcpFrameServer();
await tcpFrame.StartAsync();
```

### Server Advanced (Optional ILogger)

```csharp
var tcpFrame = new TcpFrameClient(logger)
{
    Port = 9000,
    Config = new Configuration
    {
        EventLoopGroup = new MultithreadEventLoopGroup(),
        Shared = new Configuration.General
        {
            ByteOrder = ByteOrder.BigEndian,
            LengthFieldLength = 4,
            LengthAdjustment = 0
        },
        Encoder = new Configuration.Encoding
        {
            LengthFieldIncludesLengthFieldLength = false
        },
        Decoder = new Configuration.Decoding
        {
            MaxFrameLength = 8 * 1_024 * 1_024,
            LengthFieldOffset = 0,
            InitialBytesToStrip = 4,
            FailFast = false
        }
    }
};

await tcpFrame.StartAsync();
```

## Contributing

Contributions to TcpFrame are welcome! If you find any issues or have ideas for improvements, please open an issue or submit a pull request on the Github repository: https://github.com/mbwilding/TcpFrame.

When contributing, please follow the existing code style and conventions. Additionally, make sure to thoroughly test your changes and provide appropriate documentation.

## License

TcpFrame is distributed under the MIT License. See the LICENSE file for more information.
