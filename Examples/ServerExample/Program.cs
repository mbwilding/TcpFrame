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

tcpFrame.Started += () => logger.LogInformation("Started");
tcpFrame.Stopped += () => logger.LogInformation("Stopped");
tcpFrame.ClientConnected += async channel =>
{
    await tcpFrame.SendToAllAsync("Client connected"u8.ToArray());
    logger.LogInformation("Client connected: {Ip}", channel.RemoteAddress.ToString());
};
tcpFrame.ClientDisconnected += async channel =>
{
    await tcpFrame.SendToAllAsync("Client disconnected"u8.ToArray());
    logger.LogInformation("Client disconnected: {Ip}", channel.RemoteAddress.ToString());
};
tcpFrame.MessageReceived += async (channel, bytes) =>
{
    string id = channel.Id.AsShortText();
    string message = Encoding.UTF8.GetString(bytes);
    logger.LogInformation("Received [{Id}]: {Message}", id, message);
    await tcpFrame.SendAsync(channel, "Ack"u8.ToArray());
};

// Connect
await tcpFrame.StartAsync();

// Prevent exit
Console.ReadKey();
