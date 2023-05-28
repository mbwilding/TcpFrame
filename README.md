# TcpFrame C# Library [![NuGet](https://img.shields.io/nuget/v/TcpFrame?style=plastic)](https://www.nuget.org/packages/TcpFrame/)

TcpFrame is a C# library that provides event-driven TCP framing capabilities. It simplifies the process of handling framed data over TCP connections, allowing you to focus on processing the received frames.

## Installation

Search for the nuget `TcpFrame` and add it to your project.

Alternatively, via the CLI `dotnet add package TcpFrame`

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

## Rust - Tokio Framing Interop

I wrote this wrapper for [DotNetty](https://github.com/Azure/DotNetty) so I could communicate with my rust service that utilizes framing. The defaults of TcpFrame suits tokio framing with the defaults.

### Code fragment for Rust

```rust
use tokio::net::TcpStream;
use tokio_util::codec::{Framed, LengthDelimitedCodec};

// stream and tokio spawn code for handling clients was redacted for brevity

fn create_framed_socket(stream: TcpStream) -> Framed<TcpStream, LengthDelimitedCodec> {
    // Default settings
    let codec = LengthDelimitedCodec::new();
    
    // Default settings (Expanded)
    let codec = LengthDelimitedCodec::builder()
        .max_frame_length(8 * 1_024 * 1_024)
        .length_field_offset(0)
        .length_field_length(4)
        .length_adjustment(0)
        .big_endian()
        .new_codec();
    
    // Create the framed stream
    Framed::new(stream, codec)
}
```

You can use this with MessagePack serialization, which allows you to interop between [MessagePack (CSharp)](https://github.com/neuecc/MessagePack-CSharp) and [MessagePack (Rust)](https://github.com/3Hren/msgpack-rust).
As a side note, rust handles its enums as arrays compared to dotnet which handles it as an integer type, so you will need to deal with that in your code, either at the dotnet side or the rust side. I opted to handle it at the rust side.

## Contributing

Contributions to TcpFrame are welcome! If you find any issues or have ideas for improvements, please open an issue or submit a pull request on the Github repository: https://github.com/mbwilding/TcpFrame.

When contributing, please follow the existing code style and conventions. Additionally, make sure to thoroughly test your changes and provide appropriate documentation.

## License

TcpFrame is distributed under the MIT License. See the LICENSE file for more information.
