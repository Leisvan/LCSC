﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LCSC.Discord.Models;
using LCSC.Discord.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LCSC.App.ViewModels;

public partial class DiscordBotViewModel(DiscordBotService botService) : ObservableObject
{
    private readonly DiscordBotService _botService = botService;

    private bool _isConnected;
    private DiscordGuildModel? _selectedGuild;

    public ObservableCollection<DiscordGuildModel> Guilds { get; } = [];

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
    public void CancelUpdateRank()
        => _botService.CancelUpdateMemberRegions();

    [RelayCommand]
    public async Task UpdateAndDisplayRank(bool includeBanned)
    {
        if (SelectedGuild == null)
        {
            return;
        }
        await _botService.UpdateMemberRegionsAsync(false, SelectedGuild.Id);
        await _botService.DisplayRankAsync(includeBanned, SelectedGuild.Id);
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

    [RelayCommand]
    private Task DisplayRank(bool includeBanned)
    {
        if (SelectedGuild == null)
        {
            return Task.CompletedTask;
        }
        return _botService.DisplayRankAsync(includeBanned, SelectedGuild.Id);
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