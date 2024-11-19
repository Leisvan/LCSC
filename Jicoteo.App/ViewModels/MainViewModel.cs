using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jicoteo.App.Bot;
using Jicoteo.App.Services;
using System;
using System.Threading.Tasks;

namespace Jicoteo.App.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private BotService _botService;

        public MainViewModel(BotService botService)
        {
            _botService = botService;
        }

        [RelayCommand]
        private Task ConnectBot()
        => _botService.ConnectAsync();
    }
}