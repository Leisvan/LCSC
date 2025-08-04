using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LCSC.Core.Services;
using System.Threading.Tasks;

namespace LCSC.App.ObservableObjects;

public partial class ProfileCreatorObservableObject : ObservableObject
{
    private readonly MembersService _membersService;
    private string? _battleTag;
    private bool _isSearching;
    private string? _notes;
    private string? _profileId;
    private string? _profileRealm;
    private string? _pulseId;

    public ProfileCreatorObservableObject(MembersService membersService)
    {
        _membersService = membersService;
    }

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

    [RelayCommand]
    public async Task Search()
    {
        IsSearching = true;

        var result = await _membersService.SearchProfileByBattleTag(BattleTag);
        if (result != null)
        {
            PulseId = result.PulseId;
            ProfileRealm = result.ProfileRealm;
            ProfileId = result.ProfileId;
        }

        IsSearching = false;
    }

    public async Task<bool> ValidateAsync()
    {
        if (IsSearching)
        {
            return false;
        }
        if (string.IsNullOrWhiteSpace(PulseId))
        {
            await Search();
        }
        if (string.IsNullOrWhiteSpace(PulseId))
        {
            return false;
        }
        return true;
    }
}