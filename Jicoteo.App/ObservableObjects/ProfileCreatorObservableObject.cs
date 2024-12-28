using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LCSC.App.Services;
using System.Threading.Tasks;

namespace LCSC.App.ObservableObjects;

public partial class ProfileCreatorObservableObject : ObservableObject
{
    private string? _battleTag;
    private bool _isSearching;
    private string? _notes;
    private string? _profileId;
    private string? _profileRealm;
    private string? _pulseId;
    private readonly RemoteDataService _remoteDataService;

    public string? BattleTag
    {
        get => _battleTag;
        set => SetProperty(ref _battleTag, value);
    }

    public bool IsSearching
    {
        get => _isSearching;
        set => SetProperty(ref _isSearching, value);
    }

    public string? Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public string? ProfileId
    {
        get => _profileId;
        set => SetProperty(ref _profileId, value);
    }

    public string? ProfileRealm
    {
        get => _profileRealm;
        set => SetProperty(ref _profileRealm, value);
    }

    public string? PulseId
    {
        get => _pulseId;
        set => SetProperty(ref _pulseId, value);
    }

    public ProfileCreatorObservableObject(RemoteDataService airtableService)
    {
        _remoteDataService = airtableService;
    }

    [RelayCommand]
    public async Task Search()
    {
        IsSearching = true;

        var result = await _remoteDataService.SearchProfileByBattleTag(BattleTag);
        if (result != null)
        {
            PulseId = result.PulseId;
            ProfileRealm = result.ProfileRealm;
            ProfileId = result.ProfileId;
        }

        IsSearching = false;
    }
}