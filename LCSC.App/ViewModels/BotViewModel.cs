using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DSharpPlus.Entities;
using LCSC.Core.Helpers;
using LCSC.Discord.Services;
using LCSC.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LCSC.App.ViewModels;

public partial class DiscordBotViewModel(DiscordBotService botService) : ObservableObject
{
    private readonly DiscordBotService _botService = botService;

    private bool _isConnected;

    private GuildSettingsModel? _selectedGuild;

    public ObservableCollection<GuildSettingsModel> Guilds { get; } = [];

    public bool IsConnected
    {
        get => _isConnected;
        set => SetProperty(ref _isConnected, value);
    }

    #region Ranking commands

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRankingIdle))]
    public partial bool IsRankingBusy { get; set; } = false;

    public bool IsRankingIdle => !IsRankingBusy;

    public GuildSettingsModel? SelectedGuild
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
            await _botService.UpdateMemberRegionsAsync(forceUpdate, SelectedGuild.GuildId);
        }
        if (displayRank)
        {
            await _botService.DisplayRankAsync(includeBanned, SelectedGuild.GuildId);
        }
        IsRankingBusy = false;
    }

    #endregion Ranking commands

    #region Message commands

    [RelayCommand]
    private Task SendChannelMessage()
    {
        return Task.CompletedTask;
    }

    #endregion Message commands

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
        var allGuilds = _botService.GetSettingServers(BuildHelper.IsDebugBuild);
        foreach (var item in allGuilds)
        {
            Guilds.Add(item);
        }
        var first = Guilds.FirstOrDefault(x => !x.Record.IsDebugGuild);
        if (first != null)
        {
            SelectedGuild = first;
        }
    }
}