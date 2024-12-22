using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LSCC.App.Services;
using System.Threading.Tasks;

namespace LSCC.App.ViewModels;

public partial class BotViewModel : ObservableObject
{
    private readonly BotService _botService;

    public BotViewModel(BotService botService)
    {
        _botService = botService;
    }

    [RelayCommand]
    private Task ConnectBot()
        => _botService.ConnectAsync();
}