using GoolGoal.API.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GoolGoal.API.RequestModels
{
    public class ForgotPasswordRequestModel
    {
        public string UserEmail { get; set; } = "";
    }
    public class GetUserProfileRequestModel
    {
        public string UserId { get; set; } = "";
    }
    public class GetLiveFixturesRequestModel
    {
        public string? UserId { get; set; }
        public Boolean IsFollowing { get; set; }
        public string? LeagueIds { get; set; } = "";
        public string? TeamIds { get; set; } = "";
    }
    public class GetLeaguesRequestModel
    {
        public string UserId { get; set; } = "";
        public Boolean IsFollowing { get; set; }
    }
    public class GetTeamsByLeagueIdRequestModel
    {
        public List<LeagList> Leagues { get; set; } = new List<LeagList>();
        public string? UserId { get; set; }
        public Boolean IsFavourite { get; set; }
    }
    public class LeagList
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class GetFixtureDataModel
    {
        public string FixtureId { get; set; }
    }
    public class FixtureStandingsModel
    {
        public string LeagueId { get; set; }
        public string Season { get; set; }
    }
    public class HeadToHeadRequestModel
    {
        public string TeamIds { get; set; }
    }

    public class LeagueFollowRequestModel
    {
        public string UserId { get; set; }
        public string LeagueId { get; set; }
        public Boolean IsFollow { get; set; }
    }
    public class TeamFavouriteRequestModel
    {
        public string UserId { get; set; }
        public string TeamId { get; set; }
        public Boolean IsFavourite { get; set; }
    }
    public class FixtureFollowRequestModel
    {
        public string UserId { get; set; }
        public string FixtureId { get; set; }
        public Boolean IsFollow { get; set; }
    }
}
