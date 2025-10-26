using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LCSC.App.Helpers;
using LCSC.Core.Helpers;
using LCSC.Discord.Services;
using LCSC.Models;
using LCTWorks.WinUI.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LCSC.App.ViewModels;

public partial class DiscordBotViewModel(DiscordBotService botService) : ObservableObject
{
    private readonly DiscordBotService _botService = botService;
    private readonly DispatcherHelper _dispatcherHelper = new();
    private TimeSpan _scheduleTime1 = TimeSpan.FromHours(12);
    private TimeSpan _scheduleTime2 = TimeSpan.FromHours(0);

    private GuildSettingsModel? _selectedGuild;

    public ObservableCollection<GuildSettingsModel> Guilds { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDisconnected))]
    public partial bool IsConnected { get; set; }

    public bool IsDisconnected => !IsConnected;

    public bool IsDispatcherRunning
    {
        get => _dispatcherHelper?.IsRunning ?? false;
        set
        {
            if (value)
            {
                _dispatcherHelper?.Start();
            }
            else
            {
                _dispatcherHelper?.Stop();
            }
            OnPropertyChanged();
        }
    }

    public TimeSpan ScheduleTime1
    {
        get => _scheduleTime1;
        set
        {
            if (SetProperty(ref _scheduleTime1, value))
            {
                UpdateDispatcherTimes();
                LocalSettingsHelper.SaveSetting(nameof(ScheduleTime1), ScheduleTime1);
            }
        }
    }

    public TimeSpan ScheduleTime2
    {
        get => _scheduleTime2;
        set
        {
            if (SetProperty(ref _scheduleTime2, value))
            {
                UpdateDispatcherTimes();
                LocalSettingsHelper.SaveSetting(nameof(ScheduleTime2), ScheduleTime2);
            }
        }
    }

    [RelayCommand]
    private async Task ConnectBot()
    {
        if (await _botService.ConnectAsync())
        {
            IsConnected = true;
            await LoadAsync();
            StartDispatcher();
        }
    }

    [RelayCommand]
    private async Task DisconnectBot()
    {
        await _botService.DisconnectAsync();
        IsConnected = false;
        _dispatcherHelper.Stop();
    }

    private void DispatcherTick(object? sender, EventArgs e)
    {
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

    private void StartDispatcher()
    {
        _dispatcherHelper.Tick -= DispatcherTick;
        _dispatcherHelper.Tick += DispatcherTick;
        IsDispatcherRunning = true;
        _dispatcherHelper.ClearTickTimes();

        _scheduleTime1 = LocalSettingsHelper.ReadSetting(nameof(ScheduleTime1), TimeSpan.FromHours(12));
        _scheduleTime2 = LocalSettingsHelper.ReadSetting(nameof(ScheduleTime2), TimeSpan.FromHours(0));
        OnPropertyChanged(nameof(ScheduleTime1));
        OnPropertyChanged(nameof(ScheduleTime2));
        UpdateDispatcherTimes();
    }

    private void UpdateDispatcherTimes()
    {
        _dispatcherHelper.ClearTickTimes();
        _dispatcherHelper.AddTickTime(ScheduleTime1);
        _dispatcherHelper.AddTickTime(ScheduleTime2);
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
}