using System.IO;
using System.Threading.Tasks;

namespace TcpFrame;

public static class Helpers
{
    internal static async Task<(byte[]? Bytes, bool Success)> GetFileAsync(string filePath)
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
}
