using CommunityToolkit.Mvvm.ComponentModel;
using LCSC.App.Models;

namespace LCSC.App.ObservableObjects;

public partial class MatchObservableObject(
    MemberObservableObject? winner = null,
    MemberObservableObject? loser = null,
    Race winnerRace = Race.Unknown,
    Race loserRace = Race.Unknown,
    int winnerScore = 0,
    int loserScore = 0,
    MatchStage stage = MatchStage.Round7,
    string? notes = null) : ObservableObject
{
    public MemberObservableObject? Loser { get; } = loser;

    public Race LoserRace { get; } = loserRace;

    public int LoserScore { get; } = loserScore;

    public string? Notes { get; } = notes;

    public MatchStage Stage { get; } = stage;

    public MemberObservableObject? Winner { get; } = winner;

    public Race WinnerRace { get; } = winnerRace;

    public int WinnerScore { get; } = winnerScore;
}