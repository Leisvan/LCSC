using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LCSC.Discord.Services;
using System.Threading.Tasks;

namespace LCSC.App.ViewModels;

public partial class DiscordBotViewModel : ObservableObject
{
    private readonly DiscordBotService _botService;

    public DiscordBotViewModel(DiscordBotService botService)
    {
        _botService = botService;
    }

    [RelayCommand]
    private Task ConnectBot()
        => _botService.ConnectAsync();
}