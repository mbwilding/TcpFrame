using System.Text;
using Microsoft.Extensions.Logging;
using Serilog;
using TcpFrame;

// Logging (Optional)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

var loggerFactory = new LoggerFactory().AddSerilog();
var logger = loggerFactory.CreateLogger<TcpFrameServer>();

// Setup
var tcpFrame = new TcpFrameServer(logger);

tcpFrame.Started += () => logger.LogInformation("Started: Listening on port {Port}", tcpFrame.Port);
tcpFrame.Stopped += () => logger.LogInformation("Stopped");
tcpFrame.ClientConnected += async channel =>
{
    string id = channel.Id.AsShortText();
    await tcpFrame.UnicastAsync(channel, "[Server] Connected");
    await tcpFrame.BroadcastAsync($"[{id}] Client connected");
    logger.LogInformation("[{Id}] Client connected: {Ip}", id, channel.RemoteAddress.ToString());
};
tcpFrame.ClientDisconnected += async channel =>
{
    string id = channel.Id.AsShortText();
    await tcpFrame.BroadcastAsync($"[{id}] Client disconnected");
    logger.LogInformation("[{Id}] Client disconnected: {Ip}", id, channel.RemoteAddress.ToString());
};
tcpFrame.MessageReceived += async (channel, bytes) =>
{
    string id = channel.Id.AsShortText();
    string message = Encoding.UTF8.GetString(bytes);
    logger.LogInformation("[{Id}] {Message}", id, message);
    await tcpFrame.BroadcastAsync($"[{id}] {message}");
};

// Connect
await tcpFrame.StartAsync();

// Chat loop
while (true)
{
    var message = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(message)) continue;
    await tcpFrame.BroadcastAsync($"[Server] {message}");
}
