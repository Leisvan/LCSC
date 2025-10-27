using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LCSC.Core.Helpers;
using LCSC.Discord.Services;
using LCSC.Models;
using LCTWorks.Common;
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
    private readonly ScheduleTimer _timer = new();
    private TimeSpan _scheduleTime1 = TimeSpan.FromHours(12);
    private TimeSpan _scheduleTime2 = TimeSpan.FromHours(0);

    private GuildSettingsModel? _selectedGuild;

    public ObservableCollection<GuildSettingsModel> Guilds { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDisconnected))]
    public partial bool IsConnected { get; set; }

    public bool IsDisconnected => !IsConnected;

    public bool IsTimerRunning
    {
        get => _timer?.IsRunning ?? false;
        set
        {
            if (value)
            {
                _timer?.Start();
            }
            else
            {
                _timer?.Stop();
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
                UpdateTimer();
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
                UpdateTimer();
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
            StartTimer();
        }
    }

    [RelayCommand]
    private async Task DisconnectBot()
    {
        CancelUpdateRank();
        await _botService.DisconnectAsync();
        IsConnected = false;
        _timer.Stop();
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

    private void StartTimer()
    {
        _timer.Tick -= TimerTick;
        _timer.Tick += TimerTick;
        IsTimerRunning = true;
        _timer.ClearCheckPoints();
        _timer.Start();

        _scheduleTime1 = LocalSettingsHelper.ReadSetting(nameof(ScheduleTime1), TimeSpan.FromHours(12));
        _scheduleTime2 = LocalSettingsHelper.ReadSetting(nameof(ScheduleTime2), TimeSpan.FromHours(0));
        OnPropertyChanged(nameof(ScheduleTime1));
        OnPropertyChanged(nameof(ScheduleTime2));
        UpdateTimer();
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

    private void UpdateTimer()
    {
        _timer.ClearCheckPoints();
        _timer.AddCheckPoint(ScheduleTime1);
        _timer.AddCheckPoint(ScheduleTime2);
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