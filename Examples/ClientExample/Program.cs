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

// tcpFrame.Config.SetCertificateFromPemFile(@"C:\Users\mbwil\RiderProjects\rusty-rat\certificates\out\combined.pem");

tcpFrame.Connected += () => logger.LogInformation("Connected");
tcpFrame.Disconnected += () => logger.LogInformation("Disconnected");
tcpFrame.MessageReceived += bytes =>
{
    string message = Encoding.UTF8.GetString(bytes);
    logger.LogInformation("Received: {Message}", message);
};

// Connect
await tcpFrame.ConnectAsync();

// Wait
while (!tcpFrame.IsActive)
    await Task.Delay(100);

// Send data
await tcpFrame.SendAsync(Encoding.UTF8.GetBytes($"Hello from client: {tcpFrame.Channel?.Id.AsShortText()}"));

// Prevent exit
Console.ReadKey();
