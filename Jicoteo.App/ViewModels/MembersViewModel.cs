using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LCSC.App.ObservableObjects;
using LCSC.App.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LCSC.App.ViewModels;

public partial class MembersViewModel(RemoteDataService airtableService) : ObservableObject
{
    private readonly RemoteDataService _airtableService = airtableService;

    [ObservableProperty]
    private bool _isLoading;

    private string? _searchTerm;

    public ObservableCollection<MemberObservableObject> Members { get; set; } = [];

    public string? SearchTerm
    {
        get => _searchTerm;

        set
        {
            SetProperty(ref _searchTerm, value);
            RefreshMembersAsync();
        }
    }

    [RelayCommand]
    public void Refresh(bool force)
    {
        SearchTerm = string.Empty;
        RefreshMembersAsync(force); ;
    }

    private async void RefreshMembersAsync(bool forceRefresh = false)
    {
        IsLoading = true;
        IEnumerable<MemberObservableObject> source = await _airtableService.GetMembersAsync(forceRefresh);
        if (!string.IsNullOrWhiteSpace(_searchTerm))
        {
            source = source.Where(m => m.Nick?.Contains(_searchTerm) == true || m.RealName?.Contains(_searchTerm) == true);
        }
        Members.Clear();
        foreach (var item in source)
        {
            Members.Add(item);
        }
        IsLoading = false;
    }
}