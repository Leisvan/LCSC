using CommunityToolkit.Mvvm.ComponentModel;
using LCSC.App.Models;
using System;

namespace LCSC.App.ObservableObjects;

public partial class MatchObservableObject(
    MemberObservableObject winner,
    MemberObservableObject loser,
    Race winnerRace = Race.Unknown,
    Race loserRace = Race.Unknown,
    int winnerScore = 0,
    int loserScore = 0,
    MatchStage stage = MatchStage.Round7,
    string? notes = "") : ObservableObject, IComparable<MatchObservableObject>
{
    public MemberObservableObject Loser { get; } = loser;

    public Race LoserRace { get; } = loserRace;

    public int LoserScore { get; } = loserScore;

    public string? Notes { get; } = notes;

    public MatchStage Stage { get; } = stage;

    public MemberObservableObject Winner { get; } = winner;

    public Race WinnerRace { get; } = winnerRace;

    public int WinnerScore { get; } = winnerScore;

    public int CompareTo(MatchObservableObject? other)
    {
        if (other == null)
        {
            return 1;
        }
        return this.Stage.CompareTo(other.Stage);
    }
}