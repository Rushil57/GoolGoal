namespace GoolGoal.API.Models
{
    public class MatchFixturesModel
    {
    }
    public class MatchDetailModel
    {
        public string FixtureId { get; set; } = "";
        public string LeagueId { get; set; } = "";
        public string LeagueName { get; set; } = "";
        public string Country { get; set; } = "";
        public string HomeTeam { get; set; } = "";
        public string HomeTeamId { get; set; } = "";
        public string HomeTeamLogo { get; set; } = "";
        public string AwayTeam { get; set; } = "";
        public string AwayTeamId { get; set; } = "";
        public string AwayTeamLogo { get; set; } = "";
        public string Score { get; set; } = "";
        public string Time { get; set; } = "";
        public string StartTime { get; set; } = "";
        public string Season { get; set; } = "";
        public Boolean IsFollow { get; set; }
    }
    public class LeagueList
    {
        public string? LeagueId { get; set; }
        public string? LeagueName { get; set; }
        public string? Logo { get; set; }
        public Boolean IsFollow { get; set; }
    }
    public class TeamList
    {
        public string? LeagueId { get; set; }
        public string? LeagueName { get; set; }
        public string? TeamId { get; set; }
        public string? Name { get; set; }
        public string? Logo { get; set; }
        public Boolean IsFavourite { get; set; }

    }
    public class MatchLineupsModel
    {
        public string PlayerName { get; set; }
        public string Position { get; set; }
        public string Number { get; set; }
        public string TeamId { get; set; }
        public string Team { get; set; }
    }
    public class MatchStandingsModel
    {
        public string Rank { get; set; }
        public string TeamName { get; set; }
        public string TeamLogo { get; set; }
        public string P { get; set; }
        public string GD { get; set; }
        public string PTS { get; set; }
    }
    public class MatchStatisticsModel
    {
        public Team ShotsOnGoal { get; set; } = new Team() { HomeTeam = "0", AwayTeam = "0", Name = "Shots on target" };
        public Team ShotsOffGoal { get; set; } = new Team() { HomeTeam = "0", AwayTeam = "0", Name = "Shots off target" };
        public Team BallPossession { get; set; } = new Team() { HomeTeam = "0", AwayTeam = "0", Name = "Possession (%)" };
        public Team CornerKicks { get; set; } = new Team() { HomeTeam = "0", AwayTeam = "0", Name = "Corner Kicks" };
        public Team OffSides { get; set; } = new Team() { HomeTeam = "0", AwayTeam = "0", Name = "Offsides" };
        public Team Fouls { get; set; } = new Team() { HomeTeam = "0", AwayTeam = "0", Name = "Fouls" };
        public Team GoalkeeperSaves { get; set; } = new Team() { HomeTeam = "0", AwayTeam = "0", Name = "Goalkeeper Saves" };
        public Team YellowCards { get; set; } = new Team() { HomeTeam = "0", AwayTeam = "0", Name = "Yellow Card" };
        public Team RedCards { get; set; } = new Team() { HomeTeam = "0", AwayTeam = "0", Name = "Red Card" };
    }
    public class Team
    {
        public string HomeTeam { get; set; }
        public string AwayTeam { get; set; }
        public string Name { get; set; }
    }
    public class MatchSubstituesModel
    {
        public string PlayerName { get; set; }
        public string PlayerImage { get; set; }
        public string TeamName { get; set; }
        public string Team { get; set; }
    }
    public class MatchSummary
    {
        public string? FixtureId { get; set; }

        public string? HomeTeam { get; set; }

        public string? AwayTeam { get; set; }

        public string? HTScore { get; set; }

        public string? Status { get; set; }

        public List<MatchEvents> MatchEvents { get; set; }
    }

    public class MatchEvents
    {
        public int? Elapsed { get; set; }
        public string? PlayerName { get; set; }
        public string? Type { get; set; }
        public string? Details { get; set; }
        public string? TeamName { get; set; }
        public string? Score { get; set; }
    }
    public class MatchInfo
    {
        public string? FixtureId { get; set; }
        public string? LeagueId { get; set; }
        public string? Date { get; set; }
        public string? Location { get; set; }
        public string? Score { get; set; }
        public string? HomeTeam { get; set; }
        public string? HomeTeamLogo { get; set; }
        public string? AwayTeam { get; set; }
        public string? AwayTeamLogo { get; set; }
        public string? Status { get; set; }
        public List<HomeTeamEvent> HomeTeamGoals { get; set; }
        public List<AwayTeamEvent> AwayTeamGoals { get; set; }

    }

    public class HomeTeamEvent
    {
        public string? Elapsed { get; set; }
        public string? PlayerName { get; set; }
    }

    public class AwayTeamEvent
    {
        public string? Elapsed { get; set; }
        public string? PlayerName { get; set; }
    }
    public class PlayerStatisticModel
    {
        public int Rank { get; set; }
        public string TeamName { get; set; }
        public string TeamLogo { get; set; }
        public string PlayerName { get; set; }
        public string Total { get; set; }
    }
    public class PlayerStatisticModelData
    {
        public string KeyName { get; set; }
        public List<PlayerStatisticModel> Data { get; set; } = new List<PlayerStatisticModel>();

    }
    public class HeadToHead
    {
        public List<HeadToHeadStatistics> HeadToHeadstatistics { get; set; }

        public List<HeadToHeadEvents> HeadToHeadEvents { get; set; }

    }

    public class HeadToHeadStatistics
    {
        public string? WinMatch { get; set; }

        public string? DrawMatch { get; set; }

        public string? TeamName { get; set; }

        //public List<HeadToHead> HeadToHeadEvents { get; set; }
    }

    public class HeadToHeadEvents
    {

        public string? EventDate { get; set; }

        public string? Score { get; set; }

        public string? HomeTeam { get; set; }

        public string? AwayTeam { get; set; }
    }

}
