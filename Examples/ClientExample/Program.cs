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
var logger = loggerFactory.CreateLogger<TcpFrameClient>();

// Setup
var tcpFrame = new TcpFrameClient(logger);

tcpFrame.Connected += () => logger.LogInformation("Connected");
tcpFrame.Disconnected += () => logger.LogInformation("Disconnected");
tcpFrame.MessageReceived += bytes =>
{
    string message = Encoding.UTF8.GetString(bytes);
    logger.LogInformation("{Message}", message);
};

// Connect
await tcpFrame.ConnectAsync();

// Chat loop
while (true)
{
    var message = Console.ReadLine()!;
    await tcpFrame.SendAsync(message);
}
