using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LCSC.App.Helpers;
using LCSC.App.ObservableObjects;
using LCSC.App.Services;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LCSC.App.ViewModels
{
    public partial class TournamentsViewModel(RemoteDataService airtableService) : ObservableObject
    {
        private readonly RemoteDataService _airtableService = airtableService;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isUploading;

        private TournamentObservableObject? _selectedTournament;

        public MatchCretorObservableObject MatchCreator { get; } = new MatchCretorObservableObject();

        public TournamentObservableObject? SelectedTournament
        {
            get => _selectedTournament;

            set
            {
                SetProperty(ref _selectedTournament, value);
                if (_selectedTournament != null)
                {
                    MatchCreator.Update(_selectedTournament.Participants);
                }
            }
        }

        public ObservableCollection<TournamentObservableObject> Tournaments { get; set; } = [];

        [RelayCommand]
        public void AddMatch()
        {
            var match = MatchCreator.ToMatch();
            if (match == null || SelectedTournament == null)
            {
                return;
            }
            SelectedTournament.Matches?.Add(match);
        }

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

        [RelayCommand]
        public async Task RemoveSelectedMatch()
        {
            if (SelectedTournament == null ||
                SelectedTournament.Matches == null ||
                SelectedTournament.SelectedMatch == null)
            {
                return;
            }
            var result = await DialogHelper.ShowDialogAsync("Eliminar partida", "¿Está seguro que quiere eliminar esta partida?", "Sí", "No");

            if (result == ContentDialogResult.Primary)
            {
                var match = SelectedTournament.SelectedMatch;
                SelectedTournament.Matches?.Remove(match);
                MatchCreator.Update(match);

                await DialogHelper.ShowDialogAsync("Partida eliminada", "Los datos se han copiado en el dialogo de agregar partida", "Ok");
            }
        }

        [RelayCommand]
        public async Task UploadMatches()
        {
            if (SelectedTournament == null ||
                SelectedTournament.Matches == null)
            {
                return;
            }
            var result = await DialogHelper.ShowDialogAsync("Actualizar partidas",
                "¿Está seguro que quiere enviar estas partidas?", "Sí", "No");

            if (result == ContentDialogResult.Primary)
            {
                IsUploading = true;
                await _airtableService.UpdateTournamentMatchesAsync(SelectedTournament);
                IsUploading = false;
            }
        }
    }
}