using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LCSC.Discord.Models;
using LCSC.Discord.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LCSC.App.ViewModels;

public partial class DiscordBotViewModel(DiscordBotService botService) : ObservableObject
{
    private readonly DiscordBotService _botService = botService;

    private bool _isConnected;

    private DiscordGuildModel? _selectedGuild;

    public event EventHandler? OnDisconnected;

    public ObservableCollection<DiscordGuildModel> Guilds { get; } = [];

    public bool IsConnected
    {
        get => _isConnected;
        set => SetProperty(ref _isConnected, value);
    }

    [ObservableProperty]
    public partial bool IsRankingBusy { get; set; } = false;

    public DiscordGuildModel? SelectedGuild
    {
        get => _selectedGuild;
        set => SetProperty(ref _selectedGuild, value);
    }

    [RelayCommand]
    public void CancelUpdateRank()
        => _botService.CancelUpdateMemberRegions();

    [RelayCommand]
    public Task UpdateAndDisplayRank(bool includeBanned)
        => ExcecuteRankingActionAsync(true, true, false, includeBanned);

    [RelayCommand]
    public Task UpdateRank()
        => ExcecuteRankingActionAsync(true, false);

    [RelayCommand]
    public Task UpdateRankForced()
        => ExcecuteRankingActionAsync(true, false, true);

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

    [RelayCommand]
    private Task DisplayRank(bool includeBanned)
    => ExcecuteRankingActionAsync(false, true, false, includeBanned);

    private async Task ExcecuteRankingActionAsync(
        bool updateRegions,
        bool displayRank,
        bool forceUpdate = false,
        bool includeBanned = false)
    {
        if (SelectedGuild == null)
        {
            return;
        }
        IsRankingBusy = true;
        if (updateRegions)
        {
            await _botService.UpdateMemberRegionsAsync(forceUpdate, SelectedGuild.Id);
        }
        if (displayRank)
        {
            await _botService.DisplayRankAsync(includeBanned, SelectedGuild.Id);
        }
        IsRankingBusy = false;
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