using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LCSC.App.ObservableObjects;
using LCSC.Http.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCSC.App.ViewModels;

public partial class MembersViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isLoading;

    private List<MemberObservableObject> _membersSource;
    private string _searchTerm;

    public MembersViewModel()
    {
        _membersSource = [];
        Members = [];
    }

    public ObservableCollection<MemberObservableObject> Members { get; set; }

    public string SearchTerm
    {
        get => _searchTerm;

        set
        {
            SetProperty(ref _searchTerm, value);
            RefreshMembers();
        }
    }

    [RelayCommand]
    public async Task Refresh(bool force)
    {
        IsLoading = true;
        SearchTerm = string.Empty;
        if (force || Members.Count == 0)
        {
            var members = await AirtableHttpHelper.GetMemberRecordsAsync();
            if (members != null && members.Any())
            {
                _membersSource.Clear();
                _membersSource.AddRange(members.OrderBy(m => m.Nick).Select(m => new MemberObservableObject(m)));
                RefreshMembers();
            }
        }
        IsLoading = false;
    }

    private void RefreshMembers()
    {
        IEnumerable<MemberObservableObject> source = _membersSource;
        if (!string.IsNullOrWhiteSpace(_searchTerm))
        {
            source = _membersSource.Where(m => m.Nick?.Contains(_searchTerm) == true || m.RealName?.Contains(_searchTerm) == true);
        }
        Members.Clear();
        foreach (var item in source)
        {
            Members.Add(item);
        }
    }
}