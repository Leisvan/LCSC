namespace LCSC.Models.Airtable
{
    public class MatchJsonModel
    {
        public string? LoserId { get; set; }

        public int LoserRace { get; set; }

        public int LoserScore { get; set; }

        public string? Notes { get; set; } = string.Empty;

        public int Stage { get; set; }

        public string? WinnerId { get; set; }

        public int WinnerRace { get; set; }

        public int WinnerScore { get; set; }
    }
}