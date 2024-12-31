using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LCSC.App.ObservableObjects;
using LCSC.Models;
using LCSC.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using Microsoft.UI.Xaml.Controls;
using LCSC.Models.Airtable;
using LCSC.App.Helpers;

namespace LCSC.App.ViewModels;

public partial class MembersViewModel(MembersService membersService) : ObservableObject
{
    private readonly MembersService _membersService = membersService;

    [ObservableProperty]
    private bool _isLoading;

    private string? _searchTerm;

    private MemberModel? _selectedMember;

    public ObservableCollection<MemberModel> Members { get; set; } = [];

    public ProfileCreatorObservableObject ProfileCreator { get; } = new ProfileCreatorObservableObject(membersService);

    public string? SearchTerm
    {
        get => _searchTerm;

        set
        {
            SetProperty(ref _searchTerm, value);
            RefreshMembersAsync();
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

    [RelayCommand]
    public void CopyBattleTag(object item)
    {
        if (item is ItemClickEventArgs e 
            && e.ClickedItem is BattleNetProfileRecord profile)
        {
            ClipboardHelper.Copy(profile.BattleTag);
        }
    }

    private async void RefreshMembersAsync(bool forceRefresh = false)
    {
        if (IsLoading)
        {
            return;
        }
        IsLoading = true;
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
        IsLoading = false;
    }
}