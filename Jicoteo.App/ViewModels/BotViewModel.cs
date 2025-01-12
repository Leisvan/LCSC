using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LCSC.Discord.Models;
using LCSC.Discord.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LCSC.App.ViewModels;

public partial class DiscordBotViewModel : ObservableObject
{
    private readonly DiscordBotService _botService;

    private bool _isConnected;
    private DiscordGuildModel? _selectedGuild;

    public DiscordBotViewModel(DiscordBotService botService)
    {
        _botService = botService;
        Guilds = [];
    }

    public ObservableCollection<DiscordGuildModel> Guilds { get; }

    public bool IsConnected
    {
        get => _isConnected;
        set => SetProperty(ref _isConnected, value);
    }

    public DiscordGuildModel? SelectedGuild
    {
        get => _selectedGuild;
        set => SetProperty(ref _selectedGuild, value);
    }

    [RelayCommand]
    public async Task UpdateRank()
    {
        if (SelectedGuild == null)
        {
            return;
        }
        await _botService.UpdateMemberRegionsAsync(false, SelectedGuild.Id);
    }

    [RelayCommand]
    public async Task UpdateRankForced()
    {
        if (SelectedGuild == null)
        {
            return;
        }
        await _botService.UpdateMemberRegionsAsync(true, SelectedGuild.Id);
    }

    [RelayCommand]
    private async Task ConnectBot()
    {
        await _botService.ConnectAsync();
        IsConnected = true;
        Load();
    }

    [RelayCommand]
    private async Task DisconnectBot()
    {
        await _botService.DisconnectAsync();
        IsConnected = false;
    }

    private void Load()
    {
        var allGuilds = _botService.GetSettingServers();
        foreach (var item in allGuilds)
        {
            Guilds.Add(item);
        }
        var first = Guilds.FirstOrDefault();
        if (first != null)
        {
            SelectedGuild = first;
        }
    }
}