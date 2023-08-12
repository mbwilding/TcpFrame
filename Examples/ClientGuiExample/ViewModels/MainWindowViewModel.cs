using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TcpFrame;

namespace ClientGuiExample.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly TcpFrameClient _tcpFrame;
    
    public MainWindowViewModel()
    {
        _tcpFrame = new TcpFrameClient();
        _tcpFrame.MessageReceived += bytes => ChatLog += Encoding.UTF8.GetString(bytes) + "\n";
        Task.Run(_tcpFrame.ConnectAsync);
    }
    
    [ObservableProperty]
    private string _chatLog = string.Empty;
    
    [ObservableProperty]
    private string _input = string.Empty;
    
    [RelayCommand]
    public async Task SendAsync()
    {
        if (string.IsNullOrWhiteSpace(Input)) return;
        var message = Input;
        Input = string.Empty;
        await _tcpFrame.SendAsync(message);
    }
}