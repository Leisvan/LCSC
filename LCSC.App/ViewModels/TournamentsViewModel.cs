using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LCSC.App.Helpers;
using LCSC.App.Models.Messages;
using LCSC.App.ObservableObjects;
using LCSC.Core.Services;
using LCSC.Models;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LCSC.App.ViewModels
{
    public partial class TournamentsViewModel(CommunityDataService communityDataService) : ObservableRecipient
    {
        private readonly CommunityDataService _communityDataService = communityDataService;

        private bool _isLoading;

        private MatchModel? _selectedMatch;

        private TournamentModel? _selectedTournament;

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

        [ObservableProperty]
        public partial bool IsUploading { get; set; }

        public MatchCreatorObservableObject MatchCreator { get; } = new MatchCreatorObservableObject();

        public MatchModel? SelectedMatch
        {
            get => _selectedMatch;
            set => SetProperty(ref _selectedMatch, value);
        }

        public ObservableCollection<MatchModel>? SelectedMatches
            => new(SelectedTournament?.Matches ?? []);

        public TournamentModel? SelectedTournament
        {
            get => _selectedTournament;

            set
            {
                SetProperty(ref _selectedTournament, value);
                if (_selectedTournament != null)
                {
                    MatchCreator.Update(_selectedTournament.Participants);
                }
                OnPropertyChanged(nameof(SelectedMatches));
            }
        }

        public ObservableCollection<TournamentModel> Tournaments { get; set; } = [];

        [RelayCommand]
        public void AddMatch()
        {
            var match = MatchCreator.ToMatch();
            if (match == null || SelectedTournament == null)
            {
                return;
            }
            SelectedTournament.Matches?.Add(match);
            OnPropertyChanged(nameof(SelectedMatches));
        }

        [RelayCommand]
        public async Task Refresh(bool force)
        {
            IsLoading = true;
            Tournaments.Clear();
            var source = await _communityDataService.GetTournamentsAsync(force);

            Tournaments.Clear();
            foreach (var item in source)
            {
                Tournaments.Add(item);
            }
            IsLoading = false;

            SelectedTournament = Tournaments.FirstOrDefault();
        }

        [RelayCommand]
        public async Task RemoveSelectedMatch()
        {
            if (SelectedTournament == null ||
                SelectedTournament.Matches == null ||
                SelectedMatch == null)
            {
                return;
            }
            var result = await DialogHelper.ShowDialogAsync("Eliminar partida", "¿Está seguro que quiere eliminar esta partida?", "Sí", "No");

            if (result == ContentDialogResult.Primary)
            {
                var match = SelectedMatch;
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
                await _communityDataService.UpdateTournamentMatchesAsync(SelectedTournament.Record.Id, SelectedTournament.Matches);
                IsUploading = false;
            }
        }
    }
}