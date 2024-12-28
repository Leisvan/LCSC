using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LCSC.App.ObservableObjects;
using LCSC.App.Services;
using LCSC.Http.Models.Airtable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;

namespace LCSC.App.ViewModels;

public partial class MembersViewModel(RemoteDataService remoteDataService) : ObservableObject
{
    private readonly RemoteDataService _remoteDataService = remoteDataService;

    [ObservableProperty]
    private bool _isLoading;

    private string? _searchTerm;

    private MemberObservableObject? _selectedMember;

    public ObservableCollection<MemberObservableObject> Members { get; set; } = [];

    public ProfileCreatorObservableObject ProfileCreator { get; } = new ProfileCreatorObservableObject(remoteDataService);

    public string? SearchTerm
    {
        get => _searchTerm;

        set
        {
            SetProperty(ref _searchTerm, value);
            RefreshMembersAsync();
        }
    }

    public MemberObservableObject? SelectedMember
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

        var result = await _remoteDataService.CreateBattleNetProfile(
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

    private async void RefreshMembersAsync(bool forceRefresh = false)
    {
        if (IsLoading)
        {
            return;
        }
        IsLoading = true;
        IEnumerable<MemberObservableObject> source = await _remoteDataService.GetMembersAsync(forceRefresh);
        if (!string.IsNullOrWhiteSpace(_searchTerm))
        {
            source = source.Where(m => m.Nick?
            .Contains(_searchTerm, StringComparison.InvariantCultureIgnoreCase) == true ||
            m.RealName?.Contains(_searchTerm, StringComparison.InvariantCultureIgnoreCase) == true);
        }
        Members.Clear();
        foreach (var item in source)
        {
            Members.Add(item);
        }
        IsLoading = false;
    }
}