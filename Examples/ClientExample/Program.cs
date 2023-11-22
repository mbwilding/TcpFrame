using System.Text;
using TcpFrame;

// Setup
var tcpFrame = new TcpFrameClient();

tcpFrame.Connected += () => Console.WriteLine("*** Connected ***");
tcpFrame.Disconnected += () => Console.WriteLine("*** Disconnected ***");
tcpFrame.Received += bytes =>
{
    string message = Encoding.UTF8.GetString(bytes);
    Console.WriteLine(message);
};

// Connect
await tcpFrame.ConnectAsync();

// Chat loop
while (true)
{
    var message = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(message)) continue;
    await tcpFrame.SendAsync(message);
}
