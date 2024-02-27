using System.Drawing.Printing;

namespace GoolGoal.API.Models
{
    public class PlayerList
    {
        public string? PlayerName { get; set; }

        public string? Photo { get; set; }

        public string? Position { get; set; }

        public string? Captain { get; set; }

        public string? TeamName { get; set; }


    }

    public class TeamDetails
    {
        public string? LeagueId { get; set; }

        public string? HomeTeamId { get; set; }

        public string? HomeTeamName { get; set; }

        public string? AwayTeamId { get; set; }

        public string? AwayTeamName { get; set; }

        public string? FixtureId { get; set; }
        public string? Season { get; set; }

        public List<PlayerList> playerLists { get; set; }

    }
    public class HomeAwayTeamPlayersDetails
    {
        public List<HomeTeamPlayers> HomeTeamPlayers { get; set; }

        public List<AwayTeamPlayers> AwayTeamPlayers { get; set; }
    }

    public class HomeTeamPlayers
    {
        public string? PlayerName { get; set; }

        public string? Photo { get; set; }

        public string? Position { get; set; }

        public string? Captain { get; set; }

        public string? TeamName { get; set; }
    }

    public class AwayTeamPlayers
    {
        public string? PlayerName { get; set; }

        public string? Photo { get; set; }

        public string? Position { get; set; }

        public string? Captain { get; set; }

        public string? TeamName { get; set; }


    }

    //public class Team
    //{
    //    public string? Id { get; set; }
    //    public string? TeamName { get; set; }
    //}
}
