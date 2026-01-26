using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LCSC.Core.Helpers;
using LCSC.Discord.Services;
using LCSC.Models;
using LCTWorks.Core;
using LCTWorks.WinUI.Helpers;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LCSC.App.ViewModels;

public partial class DiscordBotViewModel(DiscordBotService botService) : ObservableObject
{
    private readonly DiscordBotService _botService = botService;
    private readonly DispatcherQueue _dispatcher = DispatcherQueue.GetForCurrentThread();

    private GuildSettingsModel? _selectedGuild;

    [ObservableProperty]
    public partial bool AutoScroll { get; set; } = true;

    public ObservableCollection<GuildSettingsModel> Guilds { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDisconnected))]
    public partial bool IsConnected { get; set; }

    public bool IsDisconnected => !IsConnected;

    [RelayCommand]
    private async Task ConnectBot()
    {
        if (await _botService.ConnectAsync())
        {
            IsConnected = true;
            await LoadAsync();
        }
    }

    [RelayCommand]
    private async Task DisconnectBot()
    {
        CancelUpdateRank();
        await _botService.DisconnectAsync();
        IsConnected = false;
    }

    private async Task LoadAsync(bool forceRefresh = false)
    {
        var allGuilds = await _botService.GetSettingServersAsync(BuildHelper.IsDebugBuild, forceRefresh);
        if (allGuilds == null || allGuilds.Count == 0)
        {
            return;
        }
        Guilds.Clear();
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

    [RelayCommand]
    private Task Refresh()
    {
        if (IsConnected)
        {
            return LoadAsync(true);
        }
        return Task.CompletedTask;
    }

    private async void TimerTick(object? sender, EventArgs e)
    {
        try
        {
            await UpdateAndDisplayRank(false).ConfigureAwait(false);
        }
        catch
        {
        }
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
        await UIDispatchAsync(() => IsRankingBusy = true);
        if (updateRegions)
        {
            if (!await _botService.UpdateMemberRegionsAsync(forceUpdate, SelectedGuild.GuildId))
            {
                await UIDispatchAsync(() => IsRankingBusy = false);
                return;
            }
        }
        if (displayRank)
        {
            await _botService.DisplayRankAsync(includeBanned, SelectedGuild.GuildId);
        }
        await UIDispatchAsync(() => IsRankingBusy = false);
    }

    [RelayCommand]
    private async Task PruneRegionsAsync()
    {
        await UIDispatchAsync(() => IsRankingBusy = true);

        await _botService.PruneRegionsAsync();

        await UIDispatchAsync(() => IsRankingBusy = false);
    }

    private async Task UIDispatchAsync(Action action)
    {
        _dispatcher.TryEnqueue(() =>
        {
            action();
        });
    }

    #endregion Ranking commands

    #region Message commands

    [RelayCommand]
    private Task SendChannelMessage()
    {
        return Task.CompletedTask;
    }

    #endregion Message commands
}