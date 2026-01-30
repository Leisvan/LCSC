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

public partial class MembersViewModel(CommunityDataService communityDataService, LadderService ladderService) : ObservableRecipient
{
    public int _seasonId = 0;
    private readonly CommunityDataService _communityDataService = communityDataService;
    private readonly LadderService _ladderService = ladderService;
    private bool _isLoading;
    private bool _isLoadingCurrentMember;
    private string? _searchTerm = string.Empty;

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

    public bool IsLoadingCurrentMember
    {
        get => _isLoadingCurrentMember;
        set => SetProperty(ref _isLoadingCurrentMember, value);
    }

    public ObservableCollection<MemberModel> Members { get; set; } = [];

    public bool NoMemberSelected => SelectedMember == null;

    public ProfileCreatorObservableObject ProfileCreator { get; } = new ProfileCreatorObservableObject(communityDataService);

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

    public string SeasonString => _seasonId == 0 ? string.Empty : string.Format("Members-SeasonFormat".GetTextLocalized(), _seasonId);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NoMemberSelected))]
    public partial MemberModel? SelectedMember { get; set; }

    [RelayCommand]
    public async Task AddProfile()
    {
        if (SelectedMember == null)
        {
            return;
        }
        if (!await ProfileCreator.ValidateAsync())
        {
            return;
        }

        var result = await _communityDataService.CreateBattleNetProfile(
            ProfileCreator.BattleTag,
            ProfileCreator.PulseId,
            ProfileCreator.ProfileRealm,
            ProfileCreator.ProfileId,
            SelectedMember.Profiles?.Count == 0,
            ProfileCreator.Notes,
            SelectedMember.Record.Id);

        if (result != null)
        {
            RefreshAndReselectCurrentMember();
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
    public async Task Initialize()
    {
        if (IsLoading)
        {
            return;
        }
        IsLoading = true;
        await _communityDataService.InitializeFromCacheAsync();
        await FilterMembersAsync(false);
        IsLoading = false;
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
    public async Task Refresh(bool force)
    {
        SearchTerm = string.Empty;
        await RefreshMembersAsync(force);
    }

    [RelayCommand]
    public async Task UpdateRegions()
    {
        if (IsLoading || IsLoadingCurrentMember || SelectedMember == null || SelectedMember.Profiles == null)
        {
            return;
        }
        IsLoadingCurrentMember = true;
        if (await _communityDataService.UpdateRegionsAsync(SelectedMember.Profiles))
        {
            RefreshAndReselectCurrentMember();
        }
        IsLoadingCurrentMember = false;
    }

    private async Task FilterMembersAsync(bool forceRefresh = false)
    {
        //Wait for the UI to update before fetching members
        await Task.Delay(120);

        IEnumerable<MemberModel> source = await _communityDataService.GetMembersAsync(forceRefresh);
        if (!string.IsNullOrWhiteSpace(_searchTerm))
        {
            source = source.Where(m => m.Record.Nick?
            .Contains(_searchTerm, StringComparison.InvariantCultureIgnoreCase) == true ||
            m.Record.RealName?.Contains(_searchTerm, StringComparison.InvariantCultureIgnoreCase) == true);
        }
        var firstTimeLoad = Members.Count == 0;
        Members.Clear();

        foreach (var item in source)
        {
            Members.Add(item);
        }
        if (forceRefresh || firstTimeLoad)
        {
            _seasonId = await _ladderService.GetSeasonIdAsync();
            if (_seasonId != 0)
            {
                foreach (var item in source)
                {
                    item.UpdateBestRegion(_seasonId);
                }
                OnPropertyChanged(nameof(SeasonString));
            }
        }

        SelectedMember = Members.FirstOrDefault();
    }

    private async void RefreshAndReselectCurrentMember()
    {
        var selectedId = SelectedMember?.Record.Id;
        await RefreshMembersAsync(true);
        SelectedMember = Members.FirstOrDefault(m => m.Record.Id == selectedId);
    }

    private async Task RefreshMembersAsync(bool forceRefresh = false)
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