using CommunityToolkit.Mvvm.ComponentModel;
using LCSC.App.Models;
using LCSC.Common.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LCSC.App.ObservableObjects;

public partial class MatchCreatorObservableObject : ObservableObject
{
    private MemberObservableObject? _loser;
    private Race _loserRace;
    private string? _loserScore;
    private string? _notes;
    private MatchStage _stage;
    private MemberObservableObject? _winner;
    private Race _winnerRace;
    private string? _winnerScore;

    public MemberObservableObject? Loser
    {
        get => _loser;

        set
        {
            SetProperty(ref _loser, value);
            SetupPlayerProperties(false);
        }
    }

    public Race LoserRace
    {
        get => _loserRace;
        set => SetProperty(ref _loserRace, value);
    }

    public string? LoserScore
    {
        get => _loserScore;
        set => SetProperty(ref _loserScore, value);
    }

    public MatchStage MatchStage
    {
        get => _stage;
        set => SetProperty(ref _stage, value);
    }

    public string? Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public ObservableCollection<MemberObservableObject>? Participants { get; private set; }

    public Race[] RaceValues => Enum.GetValues<Race>().Except([Race.Unknown]).ToArray();

    public MatchStage[] StageValues => Enum.GetValues<MatchStage>();

    public MemberObservableObject? Winner
    {
        get => _winner;

        set
        {
            SetProperty(ref _winner, value);
            SetupPlayerProperties(true);
        }
    }

    public Race WinnerRace
    {
        get => _winnerRace;
        set => SetProperty(ref _winnerRace, value);
    }

    public string? WinnerScore
    {
        get => _winnerScore;
        set => SetProperty(ref _winnerScore, value);
    }

    public MatchObservableObject? ToMatch()
    {
        if (Winner == null || Loser == null)
        {
            return null;
        }
        int.TryParse(WinnerScore, out int winnerScore);
        int.TryParse(LoserScore, out int loserScore);

        var swap = winnerScore < loserScore;
        var winner = swap ? Loser : Winner;
        var loser = swap ? Winner : Loser;
        var wrace = swap ? LoserRace : WinnerRace;
        var lrace = swap ? WinnerRace : LoserRace;
        var wscore = swap ? loserScore : winnerScore;
        var lscore = swap ? winnerScore : loserScore;

        return new MatchObservableObject(winner, loser, wrace, lrace, wscore, lscore, MatchStage, Notes);
    }

    public void Update(MatchObservableObject match)
    {
        Winner = match.Winner;
        Loser = match.Loser;
        WinnerRace = match.WinnerRace;
        LoserRace = match.LoserRace;
        WinnerScore = match.WinnerScore.ToString();
        LoserScore = match.LoserScore.ToString();
        MatchStage = match.Stage;
        Notes = match.Notes;
    }

    public void Update(IEnumerable<MemberObservableObject>? members)
    {
        members ??= [];
        Participants = new ObservableCollection<MemberObservableObject>(members);
        OnPropertyChanged(nameof(Participants));
    }

    private void SetupPlayerProperties(bool winner)
    {
        if (winner && Winner != null)
        {
            WinnerRace = Winner.Race;
        }
        else if (Loser != null)
        {
            LoserRace = Loser.Race;
        }
    }
}