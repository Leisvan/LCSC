using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LCSC.App.Helpers;
using LCSC.App.Models.Messages;
using LCSC.App.ObservableObjects;
using LCSC.Core.Services;
using LCSC.Models;
using LCSC.Models.Airtable;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;

namespace LCSC.App.ViewModels;

public partial class MembersViewModel(MembersService membersService) : ObservableRecipient
{
    private readonly MembersService _membersService = membersService;

    private bool _isLoading;
    private string? _searchTerm = string.Empty;
    private MemberModel? _selectedMember;

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetProperty(ref _isLoading, value))
            {
                Messenger.Send(new LoadingChangedMessage(value));
            }
        }
    }

    public ObservableCollection<MemberModel> Members { get; set; } = [];

    public ProfileCreatorObservableObject ProfileCreator { get; } = new ProfileCreatorObservableObject(membersService);

    public string? SearchTerm
    {
        get => _searchTerm;

        set
        {
            if (SetProperty(ref _searchTerm, value))
            {
                FilterMembersAsync().ConfigureAwait(false);
            }
        }
    }

    public MemberModel? SelectedMember
    {
        get => _selectedMember;
        set => SetProperty(ref _selectedMember, value);
    }

    [RelayCommand]
    public async Task AddProfile()
    {
        if (SelectedMember?.Profiles == null)
        {
            return;
        }

        var result = await _membersService.CreateBattleNetProfile(
            ProfileCreator.BattleTag,
            ProfileCreator.PulseId,
            ProfileCreator.ProfileRealm,
            ProfileCreator.ProfileId,
            SelectedMember.Profiles.Count == 0,
            ProfileCreator.Notes,
            SelectedMember.Record.Id);

        if (result != null)
        {
        }
    }

    [RelayCommand]
    public void CopyBattleTag(object item)
    {
        if (item is ItemClickEventArgs e
            && e.ClickedItem is BattleNetProfileRecord profile)
        {
            ClipboardHelper.Copy(profile.BattleTag);
        }
    }

    [RelayCommand]
    public async Task NavigateToProfilePage(string pulseId)
    {
        if (string.IsNullOrEmpty(pulseId))
        {
            return;
        }
        var uri = new Uri($"https://sc2pulse.nephest.com/sc2/?type=character&id={pulseId}#player-stats-mmr");
        await Launcher.LaunchUriAsync(uri);
    }

    [RelayCommand]
    public void Refresh(bool force)
    {
        SearchTerm = string.Empty;
        RefreshMembersAsync(force);
    }

    private async Task FilterMembersAsync(bool forceRefresh = false)
    {
        //Wait for the UI to update before fetching members
        await Task.Delay(120);

        IEnumerable<MemberModel> source = await _membersService.GetMembersAsync(forceRefresh);
        if (!string.IsNullOrWhiteSpace(_searchTerm))
        {
            source = source.Where(m => m.Record.Nick?
            .Contains(_searchTerm, StringComparison.InvariantCultureIgnoreCase) == true ||
            m.Record.RealName?.Contains(_searchTerm, StringComparison.InvariantCultureIgnoreCase) == true);
        }
        Members.Clear();
        foreach (var item in source)
        {
            Members.Add(item);
        }
    }

    private async void RefreshMembersAsync(bool forceRefresh = false)
    {
        if (IsLoading)
        {
            return;
        }
        IsLoading = true;
        await FilterMembersAsync(forceRefresh);
        IsLoading = false;
    }
}