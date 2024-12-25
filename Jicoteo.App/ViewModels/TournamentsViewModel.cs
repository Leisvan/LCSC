using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LCSC.App.ObservableObjects;
using LCSC.App.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LCSC.App.ViewModels
{
    public partial class TournamentsViewModel(AirtableService airtableService) : ObservableObject
    {
        private readonly AirtableService _airtableService = airtableService;

        [ObservableProperty]
        private bool _isLoading;

        private TournamentObservableObject? _selectedTournament;

        public TournamentObservableObject? SelectedTournament
        {
            get => _selectedTournament;
            set => SetProperty(ref _selectedTournament, value);
        }

        public ObservableCollection<TournamentObservableObject> Tournaments { get; set; } = [];


        [RelayCommand]
        public async Task Refresh(bool force)
        {
            IsLoading = true;
            Tournaments.Clear();
            IEnumerable<TournamentObservableObject> source = await _airtableService.GetTournamentsAsync(force);

            Tournaments.Clear();
            foreach (var item in source)
            {
                Tournaments.Add(item);
            }
            IsLoading = false;
        }
    }
}