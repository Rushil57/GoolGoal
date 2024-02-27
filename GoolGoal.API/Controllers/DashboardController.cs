using GoolGoal.API.Auth;
using GoolGoal.API.Common;
using GoolGoal.API.Models;
using GoolGoal.API.RequestModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.WebRequestMethods;

namespace GoolGoal.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        CommonMethod common;
        private readonly GoolGoalAppDbContext _db;
        public DashboardController(GoolGoalAppDbContext dbContext)
        {
            _db = dbContext;
            common = new CommonMethod(dbContext);
        }

        /// <summary>
        /// Get Live Match List.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///     "UserId": "f95e864b-cc80-44d7-a35d-9cadfca39b10",
        ///     "IsFollowing": false,
        ///     "LeagueIds": "",
        ///     "TeamIds": ""
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [Route("GetLiveFixtures")]
        public IActionResult GetLiveFixtures([FromBody] GetLiveFixturesRequestModel model)
        {
            List<MatchDetailModel> liveMatchList = new List<MatchDetailModel>();
            List<MatchDetailModel> liveMatchListFinal = new List<MatchDetailModel>();
            try
            {
                if (!string.IsNullOrEmpty(model.UserId))
                {
                    DataSet ds = common.GetRequestData("/fixtures?last=99", 3);
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        if (ds.Tables["fixture"] != null && ds.Tables["fixture"].Rows.Count > 0)
                        {
                            for (int i = 0; i < ds.Tables["fixture"].Rows.Count; i++)
                            {
                                DateTime MatchDate = common.ConvertTimestampToDate(ds.Tables["fixture"].Rows[i]["timestamp"].ToString());
                                if (MatchDate >= DateTime.Now.AddHours(-2))
                                {
                                    var fixtureFollow = _db.FixtureFollow.Where(r => r.FixtureId == ds.Tables["fixture"].Rows[i]["id"].ToString() && r.UserId == model.UserId).ToList().FirstOrDefault();

                                    liveMatchList.Add(new MatchDetailModel
                                    {
                                        FixtureId = ds.Tables["fixture"].Rows[i]["id"].ToString(),
                                        LeagueId = ds.Tables["league"].Rows[i]["id"].ToString(),
                                        LeagueName = ds.Tables["league"].Rows[i]["name"].ToString(),
                                        Country = ds.Tables["league"].Rows[i]["country"].ToString(),
                                        HomeTeamId = ds.Tables["home"].Rows[i]["id"].ToString(),
                                        HomeTeam = ds.Tables["home"].Rows[i]["name"].ToString(),
                                        HomeTeamLogo = ds.Tables["home"].Rows[i]["logo"].ToString(),
                                        AwayTeamId = ds.Tables["away"].Rows[i]["id"].ToString(),
                                        AwayTeam = ds.Tables["away"].Rows[i]["name"].ToString(),
                                        AwayTeamLogo = ds.Tables["away"].Rows[i]["logo"].ToString(),
                                        Score = (string.IsNullOrEmpty(ds.Tables["goals"].Rows[i]["home"].ToString()) ? "0" : ds.Tables["goals"].Rows[i]["home"].ToString()) + "-" + (string.IsNullOrEmpty(ds.Tables["goals"].Rows[i]["away"].ToString()) ? "" : ds.Tables["goals"].Rows[i]["away"].ToString()),
                                        Time = string.IsNullOrEmpty(ds.Tables["status"].Rows[i]["elapsed"].ToString()) ? "00" : ds.Tables["status"].Rows[i]["elapsed"].ToString() + ":00",
                                        StartTime = MatchDate.ToString(),
                                        Season = ds.Tables["league"].Rows[i]["season"].ToString(),
                                        IsFollow = fixtureFollow is null ? false : fixtureFollow.IsFollow,
                                    });
                                }
                            }
                        }
                    }
                    ds = common.GetRequestData("/fixtures?live=all", 3);

                    if (ds != null && ds.Tables.Count > 0)
                    {
                        if (ds.Tables["fixture"] != null && ds.Tables["fixture"].Rows.Count > 0)
                        {
                            for (int i = 0; i < ds.Tables["fixture"].Rows.Count; i++)
                            {
                                var fixtureFollow = _db.FixtureFollow.Where(r => r.FixtureId == ds.Tables["fixture"].Rows[i]["id"].ToString() && r.UserId == model.UserId).ToList().FirstOrDefault();

                                liveMatchList.Add(new MatchDetailModel
                                {
                                    FixtureId = ds.Tables["fixture"].Rows[i]["id"].ToString(),
                                    LeagueId = ds.Tables["league"].Rows[i]["id"].ToString(),
                                    LeagueName = ds.Tables["league"].Rows[i]["name"].ToString(),
                                    Country = ds.Tables["league"].Rows[i]["country"].ToString(),
                                    HomeTeamId = ds.Tables["home"].Rows[i]["id"].ToString(),
                                    HomeTeam = ds.Tables["home"].Rows[i]["name"].ToString(),
                                    HomeTeamLogo = ds.Tables["home"].Rows[i]["logo"].ToString(),
                                    AwayTeamId = ds.Tables["away"].Rows[i]["id"].ToString(),
                                    AwayTeam = ds.Tables["away"].Rows[i]["name"].ToString(),
                                    AwayTeamLogo = ds.Tables["away"].Rows[i]["logo"].ToString(),
                                    Score = (string.IsNullOrEmpty(ds.Tables["goals"].Rows[i]["home"].ToString()) ? "0" : ds.Tables["goals"].Rows[i]["home"].ToString()) + "-" + (string.IsNullOrEmpty(ds.Tables["goals"].Rows[i]["away"].ToString()) ? "0" : ds.Tables["goals"].Rows[i]["away"].ToString()),
                                    //Time = common.ConvertTimestampToDate(ds.Tables["fixture"].Rows[i]["timestamp"].ToString()).ToString("HH:mm"),
                                    Time = string.IsNullOrEmpty(ds.Tables["status"].Rows[i]["elapsed"].ToString()) ? "00" : ds.Tables["status"].Rows[i]["elapsed"].ToString() + ":00",
                                    StartTime = common.ConvertTimestampToDate(ds.Tables["fixture"].Rows[i]["timestamp"].ToString()).ToString(),
                                    Season = ds.Tables["league"].Rows[i]["season"].ToString(),
                                    IsFollow = fixtureFollow is null ? false : fixtureFollow.IsFollow,
                                });
                            }
                        }
                    }
                    if (model.LeagueIds.Length > 0)
                    {
                        var leagueids = model.LeagueIds.Split(',');
                        var teamids = model.TeamIds.Split(',');
                        if (model.TeamIds.Length > 0)
                        {
                            foreach (var teamid in liveMatchList.Where(o => leagueids.Contains(o.LeagueId) && (teamids.Contains(o.AwayTeamId) || teamids.Contains(o.HomeTeamId))))
                            {
                                liveMatchListFinal.Add(teamid);
                            }
                        }
                        else
                        {
                            liveMatchListFinal.AddRange(liveMatchList.Where(o => leagueids.Contains(o.LeagueId)));
                        }
                    }
                    else
                    {
                        liveMatchListFinal = liveMatchList;
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = model.IsFollowing ? liveMatchListFinal.Where(r => r.IsFollow == true).OrderBy(x => x.StartTime) : liveMatchListFinal.OrderBy(x => x.StartTime)
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "UserId is required" });
                }
            }
            catch (Exception ex)
            {
                CommonDBHelper.ErrorLog("DashboardController - GetLiveFixtures", ex.Message, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
            }
        }

        /// <summary>
        /// Get 20 Upcoming Match List.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///     "UserId": "f95e864b-cc80-44d7-a35d-9cadfca39b10",
        ///     "IsFollowing": false,
        ///     "LeagueIds": "",
        ///     "TeamIds": ""
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [Route("GetUpcomingFixtures")]
        public IActionResult GetUpcomingFixtures([FromBody] GetLiveFixturesRequestModel model)
        {
            List<MatchDetailModel> liveMatchList = new List<MatchDetailModel>();
            //List<MatchDetailModel> liveMatchListRes = new List<MatchDetailModel>();
            List<MatchDetailModel> liveMatchListFinal = new List<MatchDetailModel>();
            try
            {
                if (!string.IsNullOrEmpty(model.UserId))
                {
                    DataSet ds = common.GetRequestData("/fixtures?next=20", 3);
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        if (ds.Tables["fixture"] != null && ds.Tables["fixture"].Rows.Count > 0)
                        {
                            for (int i = 0; i < ds.Tables["fixture"].Rows.Count; i++)
                            {
                                var fixtureFollow = _db.FixtureFollow.Where(r => r.FixtureId == ds.Tables["fixture"].Rows[i]["id"].ToString() && r.UserId == model.UserId).ToList().FirstOrDefault();

                                liveMatchList.Add(new MatchDetailModel
                                {
                                    FixtureId = ds.Tables["fixture"].Rows[i]["id"].ToString(),
                                    LeagueId = ds.Tables["league"].Rows[i]["id"].ToString(),
                                    LeagueName = ds.Tables["league"].Rows[i]["name"].ToString(),
                                    Country = ds.Tables["league"].Rows[i]["country"].ToString(),
                                    HomeTeamId = ds.Tables["home"].Rows[i]["id"].ToString(),
                                    HomeTeam = ds.Tables["home"].Rows[i]["name"].ToString(),
                                    HomeTeamLogo = ds.Tables["home"].Rows[i]["logo"].ToString(),
                                    AwayTeamId = ds.Tables["away"].Rows[i]["id"].ToString(),
                                    AwayTeam = ds.Tables["away"].Rows[i]["name"].ToString(),
                                    AwayTeamLogo = ds.Tables["away"].Rows[i]["logo"].ToString(),
                                    Score = "0-0",
                                    Time = Convert.ToDateTime(ds.Tables["fixture"].Rows[i]["date"].ToString()).ToString("dd MMM yyy,ddd"),
                                    StartTime = common.ConvertTimestampToDate(ds.Tables["fixture"].Rows[i]["timestamp"].ToString()).ToString(),
                                    Season = ds.Tables["league"].Rows[i]["season"].ToString(),
                                    IsFollow = fixtureFollow is null ? false : fixtureFollow.IsFollow,
                                });
                            }
                        }
                    }

                    if (model.LeagueIds.Length > 0)
                    {
                        var leagueids = model.LeagueIds.Split(',');
                        var teamids = model.TeamIds.Split(',');
                        if (model.TeamIds.Length > 0)
                        {
                            foreach (var teamid in liveMatchList.Where(o => leagueids.Contains(o.LeagueId) && (teamids.Contains(o.AwayTeamId) || teamids.Contains(o.HomeTeamId))))
                            {
                                liveMatchListFinal.Add(teamid);
                            }
                            //liveMatchListRes = liveMatchListFinal;
                            //foreach (var leagueid in model.LeagueIds.Split(','))
                            //{
                            //    if (!liveMatchListRes.Exists(o => o.LeagueId == leagueid))
                            //    {
                            //        liveMatchListFinal.AddRange(liveMatchList.Where(o => o.LeagueId == leagueid));
                            //    }
                            //}
                        }
                        else
                        {
                            liveMatchListFinal.AddRange(liveMatchList.Where(o => leagueids.Contains(o.LeagueId)));
                        }
                    }
                    else
                    {
                        liveMatchListFinal = liveMatchList;
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = model.IsFollowing ? liveMatchListFinal.Where(r => r.IsFollow == true).OrderBy(x => x.StartTime) : liveMatchListFinal.OrderBy(x => x.StartTime)
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "UserId is required" });
                }
            }
            catch (Exception ex)
            {
                CommonDBHelper.ErrorLog("DashboardController - GetUpcomingFixtures", ex.Message, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
            }
        }

        /// <summary>
        /// Get Following Match List.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///     "UserId": "f95e864b-cc80-44d7-a35d-9cadfca39b10",
        ///     "IsFollowing": true,
        ///     "LeagueIds": "",
        ///     "TeamIds": ""
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [Route("GetFollowingFixtures")]
        public IActionResult GetFollowingFixtures([FromBody] GetLiveFixturesRequestModel model)
        {
            List<MatchDetailModel> liveMatchList = new List<MatchDetailModel>();
            List<MatchDetailModel> liveMatchListFinal = new List<MatchDetailModel>();
            try
            {
                if (!string.IsNullOrEmpty(model.UserId))
                {
                    //DataSet ds = common.GetRequestData("/fixtures?last=20", 3);
                    //if (ds != null && ds.Tables.Count > 0)
                    //{
                    //    if (ds.Tables["fixture"] != null && ds.Tables["fixture"].Rows.Count > 0)
                    //    {
                    //        for (int i = 0; i < ds.Tables["fixture"].Rows.Count; i++)
                    //        {
                    //            var fixtureFollow = _db.FixtureFollow.Where(r => r.FixtureId == ds.Tables["fixture"].Rows[i]["id"].ToString() && r.UserId == model.UserId).ToList().FirstOrDefault();

                    //            liveMatchList.Add(new MatchDetailModel
                    //            {

                    //                FixtureId = ds.Tables["fixture"].Rows[i]["id"].ToString(),
                    //                LeagueId = ds.Tables["league"].Rows[i]["id"].ToString(),
                    //                LeagueName = ds.Tables["league"].Rows[i]["name"].ToString(),
                    //                Country = ds.Tables["league"].Rows[i]["country"].ToString(),
                    //                HomeTeamId = ds.Tables["home"].Rows[i]["id"].ToString(),
                    //                HomeTeam = ds.Tables["home"].Rows[i]["name"].ToString(),
                    //                HomeTeamLogo = ds.Tables["home"].Rows[i]["logo"].ToString(),
                    //                AwayTeamId = ds.Tables["away"].Rows[i]["id"].ToString(),
                    //                AwayTeam = ds.Tables["away"].Rows[i]["name"].ToString(),
                    //                AwayTeamLogo = ds.Tables["away"].Rows[i]["logo"].ToString(),
                    //                Score = (string.IsNullOrEmpty(ds.Tables["goals"].Rows[i]["home"].ToString()) ? "0" : ds.Tables["goals"].Rows[i]["home"].ToString()) + "-" + (string.IsNullOrEmpty(ds.Tables["goals"].Rows[i]["away"].ToString()) ? "" : ds.Tables["goals"].Rows[i]["away"].ToString()),
                    //                Time = common.ConvertTimestampToDate(ds.Tables["fixture"].Rows[i]["timestamp"].ToString()).ToString("dd MMM yyy,ddd"),
                    //                StartTime = common.ConvertTimestampToDate(ds.Tables["fixture"].Rows[i]["timestamp"].ToString()).ToString(),
                    //                Season = ds.Tables["league"].Rows[i]["season"].ToString(),
                    //                IsFollow = fixtureFollow is null ? false : fixtureFollow.IsFollow,
                    //            });
                    //        }
                    //    }
                    //}
                    DataSet ds = common.GetRequestData("/fixtures?live=all", 3);

                    if (ds != null && ds.Tables.Count > 0)
                    {
                        if (ds.Tables["fixture"] != null && ds.Tables["fixture"].Rows.Count > 0)
                        {
                            for (int i = 0; i < ds.Tables["fixture"].Rows.Count; i++)
                            {
                                var fixtureFollow = _db.FixtureFollow.Where(r => r.FixtureId == ds.Tables["fixture"].Rows[i]["id"].ToString() && r.UserId == model.UserId).ToList().FirstOrDefault();

                                liveMatchList.Add(new MatchDetailModel
                                {
                                    FixtureId = ds.Tables["fixture"].Rows[i]["id"].ToString(),
                                    LeagueId = ds.Tables["league"].Rows[i]["id"].ToString(),
                                    LeagueName = ds.Tables["league"].Rows[i]["name"].ToString(),
                                    Country = ds.Tables["league"].Rows[i]["country"].ToString(),
                                    HomeTeamId = ds.Tables["home"].Rows[i]["id"].ToString(),
                                    HomeTeam = ds.Tables["home"].Rows[i]["name"].ToString(),
                                    HomeTeamLogo = ds.Tables["home"].Rows[i]["logo"].ToString(),
                                    AwayTeamId = ds.Tables["away"].Rows[i]["id"].ToString(),
                                    AwayTeam = ds.Tables["away"].Rows[i]["name"].ToString(),
                                    AwayTeamLogo = ds.Tables["away"].Rows[i]["logo"].ToString(),
                                    Score = (string.IsNullOrEmpty(ds.Tables["goals"].Rows[i]["home"].ToString()) ? "0" : ds.Tables["goals"].Rows[i]["home"].ToString()) + "-" + (string.IsNullOrEmpty(ds.Tables["goals"].Rows[i]["away"].ToString()) ? "" : ds.Tables["goals"].Rows[i]["away"].ToString()),
                                    //Time = common.ConvertTimestampToDate(ds.Tables["fixture"].Rows[i]["timestamp"].ToString()).ToString("HH:mm"),
                                    Time = ds.Tables["status"].Rows[i]["elapsed"].ToString() + ":00",
                                    StartTime = common.ConvertTimestampToDate(ds.Tables["fixture"].Rows[i]["timestamp"].ToString()).ToString(),
                                    Season = ds.Tables["league"].Rows[i]["season"].ToString(),
                                    IsFollow = fixtureFollow is null ? false : fixtureFollow.IsFollow,
                                });
                            }
                        }
                    }
                    ds = common.GetRequestData("/fixtures?next=20", 3);
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        if (ds.Tables["fixture"] != null && ds.Tables["fixture"].Rows.Count > 0)
                        {
                            for (int i = 0; i < ds.Tables["fixture"].Rows.Count; i++)
                            {
                                var fixtureFollow = _db.FixtureFollow.Where(r => r.FixtureId == ds.Tables["fixture"].Rows[i]["id"].ToString() && r.UserId == model.UserId).ToList().FirstOrDefault();
                                liveMatchList.Add(new MatchDetailModel
                                {
                                    FixtureId = ds.Tables["fixture"].Rows[i]["id"].ToString(),
                                    LeagueId = ds.Tables["league"].Rows[i]["id"].ToString(),
                                    LeagueName = ds.Tables["league"].Rows[i]["name"].ToString(),
                                    Country = ds.Tables["league"].Rows[i]["country"].ToString(),
                                    HomeTeamId = ds.Tables["home"].Rows[i]["id"].ToString(),
                                    HomeTeam = ds.Tables["home"].Rows[i]["name"].ToString(),
                                    HomeTeamLogo = ds.Tables["home"].Rows[i]["logo"].ToString(),
                                    AwayTeamId = ds.Tables["away"].Rows[i]["id"].ToString(),
                                    AwayTeam = ds.Tables["away"].Rows[i]["name"].ToString(),
                                    AwayTeamLogo = ds.Tables["away"].Rows[i]["logo"].ToString(),
                                    Score = "0-0",
                                    Time = Convert.ToDateTime(ds.Tables["fixture"].Rows[i]["date"].ToString()).ToString("dd MMM yyy,ddd"),
                                    StartTime = common.ConvertTimestampToDate(ds.Tables["fixture"].Rows[i]["timestamp"].ToString()).ToString(),
                                    Season = ds.Tables["league"].Rows[i]["season"].ToString(),
                                    IsFollow = fixtureFollow is null ? false : fixtureFollow.IsFollow,
                                });
                            }
                        }
                    }
                    if (model.LeagueIds.Length > 0)
                    {
                        var leagueids = model.LeagueIds.Split(',');
                        var teamids = model.TeamIds.Split(',');
                        if (model.TeamIds.Length > 0)
                        {
                            foreach (var teamid in liveMatchList.Where(o => leagueids.Contains(o.LeagueId) && (teamids.Contains(o.AwayTeamId) || teamids.Contains(o.HomeTeamId))))
                            {
                                liveMatchListFinal.Add(teamid);
                            }
                        }
                        else
                        {
                            liveMatchListFinal.AddRange(liveMatchList.Where(o => leagueids.Contains(o.LeagueId)));
                        }
                    }
                    else
                    {
                        liveMatchListFinal = liveMatchList;
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = liveMatchListFinal.Where(r => r.IsFollow == true).OrderBy(x => x.StartTime)
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "UserId is required" });
                }
            }
            catch (Exception ex)
            {
                CommonDBHelper.ErrorLog("DashboardController - GetFollowingFixtures", ex.Message, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
            }
        }

        /// <summary>
        /// Get Leagues List.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///     "UserId": "f95e864b-cc80-44d7-a35d-9cadfca39b10",
        ///     "IsFollowing": false
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [Route("GetLeagues")]
        public IActionResult GetLeagues([FromBody] GetLeaguesRequestModel model)
        {
            List<LeagueList> leagueLists = new List<LeagueList>();
            try
            {
                if (!string.IsNullOrEmpty(model.UserId))
                {
                    DataSet ds = common.GetRequestData("/leagues?season=" + DateTime.Now.Year, 3);
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        if (ds.Tables["league"] != null && ds.Tables["league"].Rows.Count > 0)
                        {
                            for (int i = 0; i < ds.Tables["league"].Rows.Count; i++)
                            {
                                var leagueFollow = _db.LeagueFollow.Where(r => r.LeagueId == ds.Tables["league"].Rows[i]["id"].ToString() && r.UserId == model.UserId).ToList().FirstOrDefault();
                                leagueLists.Add(new LeagueList
                                {
                                    LeagueId = string.IsNullOrEmpty(ds.Tables["league"].Rows[i]["id"].ToString()) ? "" : ds.Tables["league"].Rows[i]["id"].ToString(),
                                    LeagueName = string.IsNullOrEmpty(ds.Tables["league"].Rows[i]["name"].ToString()) ? "" : ds.Tables["league"].Rows[i]["name"].ToString(),
                                    Logo = string.IsNullOrEmpty(ds.Tables["league"].Rows[i]["logo"].ToString()) ? "" : ds.Tables["league"].Rows[i]["logo"].ToString(),
                                    IsFollow = leagueFollow is null ? false : leagueFollow.IsFollow,
                                });
                            }
                        }
                    }
                    ds = common.GetRequestData("/leagues?season=" + (DateTime.Now.Year - 1), 3);
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        if (ds.Tables["league"] != null && ds.Tables["league"].Rows.Count > 0)
                        {
                            for (int i = 0; i < ds.Tables["league"].Rows.Count; i++)
                            {
                                var leagueFollow = _db.LeagueFollow.Where(r => r.LeagueId == ds.Tables["league"].Rows[i]["id"].ToString() && r.UserId == model.UserId).ToList().FirstOrDefault();
                                if (leagueLists.Where(o => o.LeagueId == ds.Tables["league"].Rows[i]["id"].ToString()).ToList().Count == 0)
                                {
                                    leagueLists.Add(new LeagueList
                                    {
                                        LeagueId = string.IsNullOrEmpty(ds.Tables["league"].Rows[i]["id"].ToString()) ? "" : ds.Tables["league"].Rows[i]["id"].ToString(),
                                        LeagueName = string.IsNullOrEmpty(ds.Tables["league"].Rows[i]["name"].ToString()) ? "" : ds.Tables["league"].Rows[i]["name"].ToString(),
                                        Logo = string.IsNullOrEmpty(ds.Tables["league"].Rows[i]["logo"].ToString()) ? "" : ds.Tables["league"].Rows[i]["logo"].ToString(),
                                        IsFollow = leagueFollow is null ? false : leagueFollow.IsFollow,
                                    });
                                }
                            }
                        }
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = model.IsFollowing ? leagueLists.Where(r => r.IsFollow == true).OrderBy(x => x.LeagueName) : leagueLists.OrderBy(x => x.LeagueName)
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "UserId is required" });
                }
            }
            catch (Exception ex)
            {
                CommonDBHelper.ErrorLog("DashboardController - GetLeagues", ex.Message, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
            }
        }

        /// <summary>
        /// Get Team List by LeagueIds.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///     "Leagues": [
        ///             {
        ///             "Id": "850",
        ///             "Name": "UEFA U21 Championship - Qualification"
        ///             },
        ///             {
        ///             "Id": "36",
        ///             "Name": "Africa Cup of Nations - Qualification"
        ///             }
        ///         ],
        ///     "UserId": "f95e864b-cc80-44d7-a35d-9cadfca39b10",
        ///     "IsFavourite": false
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [Route("GetTeamsByLeagueIds")]
        public IActionResult GetTeamsByLeagueId([FromBody] GetTeamsByLeagueIdRequestModel model)
        {
            List<TeamList> teamList = new List<TeamList>();

            try
            {
                if (!string.IsNullOrEmpty(model.UserId))
                {
                    if (model.Leagues.Count > 0)
                    {
                        foreach (var leagueId in model.Leagues.ToList())
                        {
                            if (!string.IsNullOrEmpty(leagueId.Id))
                            {
                                if (!string.IsNullOrEmpty(leagueId.Name))
                                {
                                    var url = "/teams/league/" + leagueId.Id;
                                    DataSet ds = common.GetRequestData(url, 2);
                                    if (ds != null && ds.Tables.Count > 0)
                                    {
                                        if (ds.Tables["teams"] != null && ds.Tables["teams"].Rows.Count > 0)
                                        {
                                            for (int i = 0; i < ds.Tables["teams"].Rows.Count; i++)
                                            {
                                                var teams = _db.TeamFavourite.Where(r => r.TeamId == ds.Tables["teams"].Rows[i]["team_id"].ToString() && r.UserId == model.UserId).ToList().FirstOrDefault();
                                                teamList.Add(new TeamList
                                                {
                                                    LeagueId = leagueId.Id,
                                                    LeagueName = leagueId.Name,
                                                    TeamId = string.IsNullOrEmpty(ds.Tables["teams"].Rows[i]["team_id"].ToString()) ? "" : ds.Tables["teams"].Rows[i]["team_id"].ToString(),
                                                    Name = string.IsNullOrEmpty(ds.Tables["teams"].Rows[i]["name"].ToString()) ? "" : ds.Tables["teams"].Rows[i]["name"].ToString(),
                                                    Logo = string.IsNullOrEmpty(ds.Tables["teams"].Rows[i]["logo"].ToString()) ? "" : ds.Tables["teams"].Rows[i]["logo"].ToString(),
                                                    IsFavourite = teams is null ? false : teams.IsFavourite,
                                                });
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Leagues Name is required" });
                                }
                            }
                            else
                            {
                                return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Leagues Id is required" });
                            }
                        }
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Success",
                            Result = model.IsFavourite ? teamList.Where(r => r.IsFavourite == true).OrderBy(x => x.Name).Distinct() : teamList.OrderBy(x => x.Name).Distinct()
                        });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Leagues are required" });
                    }
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "UserId is required" });
                }
            }
            catch (Exception ex)
            {
                CommonDBHelper.ErrorLog("DashboardController - GetTeamsByLeagueId", ex.Message, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
            }
        }

        /// <summary>
        /// Get Match Statistics details.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///     "FixtureId": "871393"
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [Route("GetFixtureStatisticById")]
        public IActionResult GetFixtureStatisticById([FromBody] GetFixtureDataModel model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.FixtureId))
                {
                    MatchStatisticsModel matchStatistics = new MatchStatisticsModel();
                    DataSet ds = common.GetRequestData("/fixtures/id/" + model.FixtureId, 2, true, model.FixtureId);
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        if (ds.Tables["statistics"] != null && ds.Tables["statistics"].Rows.Count > 0)
                        {
                            matchStatistics.ShotsOnGoal = new Team
                            {
                                HomeTeam = string.IsNullOrEmpty(ds.Tables["Shots on Goal"].Rows[0]["home"].ToString()) ? "0" : ds.Tables["Shots on Goal"].Rows[0]["home"].ToString(),
                                AwayTeam = string.IsNullOrEmpty(ds.Tables["Shots on Goal"].Rows[0]["away"].ToString()) ? "0" : ds.Tables["Shots on Goal"].Rows[0]["away"].ToString(),
                                Name = "Shots on target"
                            };
                            matchStatistics.ShotsOffGoal = new Team
                            {
                                HomeTeam = string.IsNullOrEmpty(ds.Tables["Shots off Goal"].Rows[0]["home"].ToString()) ? "0" : ds.Tables["Shots off Goal"].Rows[0]["home"].ToString(),
                                AwayTeam = string.IsNullOrEmpty(ds.Tables["Shots off Goal"].Rows[0]["away"].ToString()) ? "0" : ds.Tables["Shots off Goal"].Rows[0]["away"].ToString(),
                                Name = "Shots off target"
                            };
                            matchStatistics.BallPossession = new Team
                            {
                                HomeTeam = string.IsNullOrEmpty(ds.Tables["Ball Possession"].Rows[0]["home"].ToString()) ? "0" : ds.Tables["Ball Possession"].Rows[0]["home"].ToString(),
                                AwayTeam = string.IsNullOrEmpty(ds.Tables["Ball Possession"].Rows[0]["away"].ToString()) ? "0" : ds.Tables["Ball Possession"].Rows[0]["away"].ToString(),
                                Name = "Possession (%)"
                            };
                            matchStatistics.CornerKicks = new Team
                            {
                                HomeTeam = string.IsNullOrEmpty(ds.Tables["Corner Kicks"].Rows[0]["home"].ToString()) ? "0" : ds.Tables["Corner Kicks"].Rows[0]["home"].ToString(),
                                AwayTeam = string.IsNullOrEmpty(ds.Tables["Corner Kicks"].Rows[0]["away"].ToString()) ? "0" : ds.Tables["Corner Kicks"].Rows[0]["away"].ToString(),
                                Name = "Corner Kicks"
                            };
                            matchStatistics.OffSides = new Team
                            {
                                HomeTeam = string.IsNullOrEmpty(ds.Tables["Offsides"].Rows[0]["home"].ToString()) ? "0" : ds.Tables["Offsides"].Rows[0]["home"].ToString(),
                                AwayTeam = string.IsNullOrEmpty(ds.Tables["Offsides"].Rows[0]["away"].ToString()) ? "0" : ds.Tables["Offsides"].Rows[0]["away"].ToString(),
                                Name = "Offsides"
                            };
                            matchStatistics.Fouls = new Team
                            {
                                HomeTeam = string.IsNullOrEmpty(ds.Tables["Fouls"].Rows[0]["home"].ToString()) ? "0" : ds.Tables["Fouls"].Rows[0]["home"].ToString(),
                                AwayTeam = string.IsNullOrEmpty(ds.Tables["Fouls"].Rows[0]["away"].ToString()) ? "0" : ds.Tables["Fouls"].Rows[0]["away"].ToString(),
                                Name = "Fouls"
                            };
                            matchStatistics.GoalkeeperSaves = new Team
                            {
                                HomeTeam = string.IsNullOrEmpty(ds.Tables["Goalkeeper Saves"].Rows[0]["home"].ToString()) ? "0" : ds.Tables["Goalkeeper Saves"].Rows[0]["home"].ToString(),
                                AwayTeam = string.IsNullOrEmpty(ds.Tables["Goalkeeper Saves"].Rows[0]["away"].ToString()) ? "0" : ds.Tables["Goalkeeper Saves"].Rows[0]["away"].ToString(),
                                Name = "Goalkeeper Saves"
                            };
                            matchStatistics.YellowCards = new Team
                            {
                                HomeTeam = string.IsNullOrEmpty(ds.Tables["Yellow Cards"].Rows[0]["home"].ToString()) ? "0" : ds.Tables["Yellow Cards"].Rows[0]["home"].ToString(),
                                AwayTeam = string.IsNullOrEmpty(ds.Tables["Yellow Cards"].Rows[0]["away"].ToString()) ? "0" : ds.Tables["Yellow Cards"].Rows[0]["away"].ToString(),
                                Name = "Yellow Card"
                            };
                            matchStatistics.RedCards = new Team
                            {
                                HomeTeam = string.IsNullOrEmpty(ds.Tables["Red Cards"].Rows[0]["home"].ToString()) ? "0" : ds.Tables["Red Cards"].Rows[0]["home"].ToString(),
                                AwayTeam = string.IsNullOrEmpty(ds.Tables["Red Cards"].Rows[0]["away"].ToString()) ? "0" : ds.Tables["Red Cards"].Rows[0]["away"].ToString(),
                                Name = "Red Card"
                            };
                        }
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = matchStatistics
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "FixtureId is required" });
                }
            }
            catch (Exception ex)
            {
                CommonDBHelper.ErrorLog("DashboardController - GetFixtureStatisticById", ex.Message, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
            }
        }

        /// <summary>
        /// Get Match Lineups details.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///         "FixtureId": "871393"
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [Route("GetFixtureLineupById")]
        public IActionResult GetFixtureLineupById([FromBody] GetFixtureDataModel model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.FixtureId))
                {
                    List<MatchLineupsModel> matchLineups = new List<MatchLineupsModel>();
                    DataSet ds = common.GetRequestData("/fixtures/id/" + model.FixtureId, 2, true, model.FixtureId);
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        if (ds.Tables["startXI"] != null && ds.Tables["startXI"].Rows.Count > 0)
                        {
                            for (int i = 0; i < ds.Tables["startXI"].Rows.Count; i++)
                            {
                                matchLineups.Add(new MatchLineupsModel
                                {
                                    PlayerName = ds.Tables["startXI"].Rows[i]["player"].ToString(),
                                    Position = ds.Tables["startXI"].Rows[i]["pos"].ToString(),
                                    Number = ds.Tables["startXI"].Rows[i]["number"].ToString(),
                                    TeamId = ds.Tables["startXI"].Rows[i]["team_id"].ToString(),
                                    Team = ds.Tables["homeTeam"].Rows[0]["team_id"].ToString() == ds.Tables["startXI"].Rows[i]["team_id"].ToString() ? "Home" : "Away",
                                });
                            }
                        }
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = matchLineups
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "FixtureId is required" });
                }
            }
            catch (Exception ex)
            {
                CommonDBHelper.ErrorLog("DashboardController - GetFixtureLineupById", ex.Message, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
            }
        }

        /// <summary>
        /// Get Match Substitutes details.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///         "FixtureId": "871393"
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [Route("GetFixtureSubstitutesById")]
        public IActionResult GetFixtureSubstitutesById([FromBody] GetFixtureDataModel model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.FixtureId))
                {

                    List<MatchSubstituesModel> matchLineups = new List<MatchSubstituesModel>();
                    DataSet ds = common.GetRequestData("/fixtures/id/" + model.FixtureId, 2);
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        if (ds.Tables["substitutes"] != null && ds.Tables["substitutes"].Rows.Count > 0)
                        {
                            var HomeTeam = ds.Tables["homeTeam"].Rows[0]["team_name"].ToString();
                            var AwayTeam = ds.Tables["awayTeam"].Rows[0]["team_name"].ToString();
                            for (int i = 0; i < ds.Tables["substitutes"].Rows.Count; i++)
                            {
                                matchLineups.Add(new MatchSubstituesModel
                                {
                                    PlayerName = ds.Tables["substitutes"].Rows[i]["player"].ToString(),
                                    PlayerImage = "https://media.api-sports.io/football/players/" + ds.Tables["substitutes"].Rows[i]["player_id"].ToString() + ".png",
                                    TeamName = ds.Tables["homeTeam"].Rows[0]["team_id"].ToString() == ds.Tables["substitutes"].Rows[i]["team_id"].ToString() ? HomeTeam : AwayTeam,
                                    Team = ds.Tables["homeTeam"].Rows[0]["team_id"].ToString() == ds.Tables["substitutes"].Rows[i]["team_id"].ToString() ? "Home" : "Away",
                                });
                            }
                        }
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = matchLineups
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "FixtureId is required" });
                }
            }
            catch (Exception ex)
            {

                CommonDBHelper.ErrorLog("DashboardController - GetFixtureSubstitutesById", ex.Message, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
            }
        }

        /// <summary>
        /// Get Match Standings details by League and Season.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///         "LeagueId": "71",
        ///         "Season": "2022"
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [Route("GetFixtureStandingsById")]
        public IActionResult GetFixtureStandingsById([FromBody] FixtureStandingsModel model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.LeagueId))
                {
                    if (!string.IsNullOrEmpty(model.Season))
                    {
                        List<MatchStandingsModel> matchLineups = new List<MatchStandingsModel>();
                        DataSet ds = common.GetRequestData("/standings?league=" + model.LeagueId + "&season=" + model.Season, 3);
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            if (ds.Tables["standings"] != null && ds.Tables["standings"].Rows.Count > 0)
                            {
                                for (int i = 0; i < ds.Tables["standings"].Rows.Count - 1; i++)
                                {
                                    if (ds.Tables["standings"].Rows[i + 1]["rank"].ToString() == "" && ds.Tables["standings"].Rows[i + 1]["league_Id"].ToString() == "0")
                                    {
                                        break;
                                    }
                                    matchLineups.Add(new MatchStandingsModel
                                    {
                                        Rank = ds.Tables["standings"].Rows[i + 1]["rank"].ToString(),
                                        TeamName = ds.Tables["team"].Rows[i]["name"].ToString(),
                                        TeamLogo = ds.Tables["team"].Rows[i]["logo"].ToString(),
                                        P = ds.Tables["all"].Rows[i]["played"].ToString(),
                                        GD = ds.Tables["standings"].Rows[i + 1]["goalsDiff"].ToString(),
                                        PTS = ds.Tables["standings"].Rows[i + 1]["points"].ToString(),
                                    });
                                }
                            }
                        }
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Success",
                            Result = matchLineups
                        });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Season is required" });
                    }
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "LeagueId is required" });
                }
            }
            catch (Exception ex)
            {
                CommonDBHelper.ErrorLog("DashboardController - GetFixtureStandingsById", ex.Message, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
            }
        }

        /// <summary>
        /// Get Match Information By FixtureId.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///         "FixtureId": "871393"
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [Route("GetFixtureInformation")]
        public IActionResult GetFixtureInformation([FromBody] GetFixtureDataModel model)
        {
            string url = "/fixtures/id/" + model.FixtureId;
            List<MatchInfo> matches = new List<MatchInfo>();
            List<HomeTeamEvent> HomeTeamEvents = new List<HomeTeamEvent>();
            List<AwayTeamEvent> AwayTeamEvents = new List<AwayTeamEvent>();
            try
            {
                if (!string.IsNullOrEmpty(model.FixtureId))
                {
                    DataSet ds = common.GetRequestData(url, 2);
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        if (ds.Tables["fixtures"] != null && ds.Tables["fixtures"].Rows.Count > 0)
                        {
                            var HomeTeam = ds.Tables["homeTeam"].Rows[0].ItemArray[1].ToString().Trim();
                            var AwayTeam = ds.Tables["awayTeam"].Rows[0].ItemArray[1].ToString();

                            if (ds.Tables["events"] != null && ds.Tables["events"].Rows.Count > 0)
                            {
                                for (int i = 0; i < ds.Tables["events"].Rows.Count; i++)
                                {
                                    var EventType = ds.Tables["events"].Rows[i]["type"].ToString();
                                    var TeamName = ds.Tables["events"].Rows[i]["teamName"].ToString().Trim();
                                    if (EventType == "Goal")
                                    {
                                        if (HomeTeam == TeamName)
                                        {
                                            HomeTeamEvents.Add(new HomeTeamEvent
                                            {
                                                Elapsed = string.IsNullOrEmpty(ds.Tables["events"].Rows[i]["elapsed"].ToString()) ? "" : ds.Tables["events"].Rows[i]["elapsed"].ToString(),
                                                PlayerName = string.IsNullOrEmpty(ds.Tables["events"].Rows[i]["player"].ToString()) ? "" : ds.Tables["events"].Rows[i]["player"].ToString()
                                            });
                                        }
                                        else
                                        {
                                            AwayTeamEvents.Add(new AwayTeamEvent
                                            {
                                                Elapsed = string.IsNullOrEmpty(ds.Tables["events"].Rows[i]["elapsed"].ToString()) ? "" : ds.Tables["events"].Rows[i]["elapsed"].ToString(),
                                                PlayerName = string.IsNullOrEmpty(ds.Tables["events"].Rows[i]["player"].ToString()) ? "" : ds.Tables["events"].Rows[i]["player"].ToString()
                                            });
                                        }
                                    }

                                }
                            }
                            for (int i = 0; i < ds.Tables["fixtures"].Rows.Count; i++)
                            {
                                matches.Add(new MatchInfo
                                {
                                    FixtureId = string.IsNullOrEmpty(ds.Tables["fixtures"].Rows[i]["fixture_id"].ToString()) ? "" : ds.Tables["fixtures"].Rows[i]["fixture_id"].ToString(),
                                    LeagueId = string.IsNullOrEmpty(ds.Tables["fixtures"].Rows[i]["league_id"].ToString()) ? "" : ds.Tables["fixtures"].Rows[i]["league_id"].ToString(),
                                    Location = string.IsNullOrEmpty(ds.Tables["fixtures"].Rows[i]["venue"].ToString()) ? "" : ds.Tables["fixtures"].Rows[i]["venue"].ToString(),
                                    HomeTeam = string.IsNullOrEmpty(ds.Tables["homeTeam"].Rows[i]["team_name"].ToString()) ? "" : ds.Tables["homeTeam"].Rows[i]["team_name"].ToString(),
                                    HomeTeamLogo = string.IsNullOrEmpty(ds.Tables["homeTeam"].Rows[i]["logo"].ToString()) ? "" : ds.Tables["homeTeam"].Rows[i]["logo"].ToString(),
                                    AwayTeam = string.IsNullOrEmpty(ds.Tables["awayTeam"].Rows[i]["team_name"].ToString()) ? "" : ds.Tables["awayTeam"].Rows[i]["team_name"].ToString(),
                                    AwayTeamLogo = string.IsNullOrEmpty(ds.Tables["awayTeam"].Rows[i]["logo"].ToString()) ? "" : ds.Tables["awayTeam"].Rows[i]["logo"].ToString(),
                                    Score = (string.IsNullOrEmpty(ds.Tables["fixtures"].Rows[i]["goalsHomeTeam"].ToString()) ? "0" : ds.Tables["fixtures"].Rows[i]["goalsHomeTeam"].ToString()) + "-" + (string.IsNullOrEmpty(ds.Tables["fixtures"].Rows[i]["goalsAwayTeam"].ToString()) ? "0" : ds.Tables["fixtures"].Rows[i]["goalsAwayTeam"].ToString()),
                                    Date = string.IsNullOrEmpty(ds.Tables["fixtures"].Rows[i]["event_timestamp"].ToString()) ? "" : common.ConvertTimestampToDate(ds.Tables["fixtures"].Rows[i]["event_timestamp"].ToString()).ToString("dd MMMM yyyy,ddd"),
                                    Status = string.IsNullOrEmpty(ds.Tables["fixtures"].Rows[i]["status"].ToString()) ? "" : ds.Tables["fixtures"].Rows[i]["status"].ToString(),
                                    HomeTeamGoals = HomeTeamEvents,
                                    AwayTeamGoals = AwayTeamEvents,
                                });
                            }
                        }
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = matches
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "FixtureId is required" });
                }
            }
            catch (Exception ex)
            {
                CommonDBHelper.ErrorLog("DashboardController - GetFixtureInformation", ex.Message, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
            }

        }

        /// <summary>
        /// Get Match Summary By FixtureId.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///         "FixtureId": "871393"
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [Route("GetFixtureSummary")]
        public IActionResult GetFixtureSummary([FromBody] GetFixtureDataModel model)
        {

            List<MatchSummary> matchSummaries = new List<MatchSummary>();
            List<MatchEvents> matchEvents = new List<MatchEvents>();
            try
            {
                if (!string.IsNullOrEmpty(model.FixtureId))
                {
                    DataSet ds = common.GetRequestData("/fixtures/id/" + model.FixtureId, 2);
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        if (ds.Tables["fixtures"] != null && ds.Tables["fixtures"].Rows.Count > 0)
                        {
                            if (ds.Tables["events"] != null && ds.Tables["events"].Rows.Count > 0)
                            {
                                int TeamAScore = 0;
                                int TeamBScore = 0;
                                for (int i = 0; i < ds.Tables["events"].Rows.Count; i++)
                                {
                                    if (ds.Tables["homeTeam"].Rows[0]["team_id"].ToString() == ds.Tables["events"].Rows[i]["team_id"].ToString())
                                    {
                                        TeamAScore = TeamAScore + (ds.Tables["events"].Rows[i]["type"].ToString() == "Goal" ? 1 : 0);
                                    }
                                    else
                                    {
                                        TeamBScore = TeamBScore + (ds.Tables["events"].Rows[i]["type"].ToString() == "Goal" ? 1 : 0);
                                    }
                                    matchEvents.Add(new MatchEvents
                                    {
                                        //Elapsed = string.IsNullOrEmpty(ds.Tables["events"].Rows[i]["elapsed"].ToString()) ? "" : ds.Tables["events"].Rows[i]["elapsed"].ToString(),
                                        Elapsed = Convert.ToInt32(ds.Tables["events"].Rows[i]["elapsed"]),
                                        PlayerName = string.IsNullOrEmpty(ds.Tables["events"].Rows[i]["player"].ToString()) ? "" : ds.Tables["events"].Rows[i]["player"].ToString(),
                                        Type = string.IsNullOrEmpty(ds.Tables["events"].Rows[i]["type"].ToString()) ? "" : ds.Tables["events"].Rows[i]["type"].ToString(),
                                        Details = string.IsNullOrEmpty(ds.Tables["events"].Rows[i]["detail"].ToString()) ? "" : ds.Tables["events"].Rows[i]["detail"].ToString(),
                                        TeamName = string.IsNullOrEmpty(ds.Tables["events"].Rows[i]["teamName"].ToString()) ? "" : ds.Tables["events"].Rows[i]["teamName"].ToString(),
                                        Score = ds.Tables["events"].Rows[i]["type"].ToString() == "Goal" ? TeamAScore + "-" + TeamBScore : "",
                                    });
                                }

                                matchEvents = matchEvents.OrderBy(x => x.Elapsed).ToList();
                            }

                            for (int i = 0; i < ds.Tables["fixtures"].Rows.Count; i++)
                            {
                                matchSummaries.Add(new MatchSummary
                                {
                                    FixtureId = string.IsNullOrEmpty(ds.Tables["fixtures"].Rows[i]["fixture_id"].ToString()) ? "" : ds.Tables["fixtures"].Rows[i]["fixture_id"].ToString(),
                                    HomeTeam = string.IsNullOrEmpty(ds.Tables["homeTeam"].Rows[i]["team_name"].ToString()) ? "" : ds.Tables["homeTeam"].Rows[i]["team_name"].ToString(),
                                    AwayTeam = string.IsNullOrEmpty(ds.Tables["awayTeam"].Rows[i]["team_name"].ToString()) ? "" : ds.Tables["awayTeam"].Rows[i]["team_name"].ToString(),
                                    HTScore = ds.Tables["score"].Rows[0]["halftime"].ToString(),
                                    Status = string.IsNullOrEmpty(ds.Tables["fixtures"].Rows[i]["status"].ToString()) ? "" : ds.Tables["fixtures"].Rows[i]["status"].ToString(),
                                    MatchEvents = matchEvents,
                                });
                            }
                        }
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = matchSummaries
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "FixtureId is required" });
                }
            }
            catch (Exception ex)
            {
                CommonDBHelper.ErrorLog("DashboardController - GetFixtureSummary", ex.Message, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
            }

        }

        /// <summary>
        /// Set Follow/Unfollow League
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///         "UserId": "a32321d9-9a58-4b23-bd66-489b07e02367",
        ///         "LeagueId": "850",
        ///         "IsFollow": true
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [Route("SetFollowLeague")]
        public IActionResult SetFollowLeague([FromBody] LeagueFollowRequestModel model)
        {
            string strError = "";
            try
            {
                if (!string.IsNullOrEmpty(model.LeagueId))
                {
                    if (!string.IsNullOrEmpty(model.UserId))
                    {
                        if (!string.IsNullOrEmpty(model.IsFollow.ToString()))
                        {
                            LeagueFollow leagues = _db.LeagueFollow.Where(r => r.LeagueId == model.LeagueId && r.UserId == model.UserId).ToList().FirstOrDefault();
                            if (leagues != null)
                            {
                                leagues.IsFollow = model.IsFollow;
                                _db.LeagueFollow.Update(leagues);
                                _db.SaveChanges();
                            }
                            else
                            {
                                _db.LeagueFollow.Add(new LeagueFollow
                                {
                                    LeagueId = model.LeagueId,
                                    UserId = model.UserId,
                                    IsFollow = model.IsFollow,
                                    CreatedDateTime = DateTime.Now
                                });
                                _db.SaveChanges();
                            }
                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "Success"
                            });
                        }
                        else
                        {
                            strError = "IsFollow is required";
                        }
                    }
                    else
                    {
                        strError = "UserId is required";
                    }
                }
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = string.IsNullOrEmpty(strError) ? "LeagueId is required" : strError });
            }
            catch (Exception ex)
            {
                CommonDBHelper.ErrorLog("DashboardController - SetFollowLeague", ex.Message, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
            }
        }

        /// <summary>
        /// Set Favourite/UnFavourite Team
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///         "UserId": "a32321d9-9a58-4b23-bd66-489b07e02367",
        ///         "TeamId": "263",
        ///         "IsFavourite": true
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [Route("SetFavouriteTeam")]
        public IActionResult SetFavouriteTeam([FromBody] TeamFavouriteRequestModel model)
        {
            string strError = "";
            try
            {
                if (!string.IsNullOrEmpty(model.TeamId))
                {
                    if (!string.IsNullOrEmpty(model.UserId))
                    {
                        if (!string.IsNullOrEmpty(model.IsFavourite.ToString()))
                        {
                            TeamFavourite team = _db.TeamFavourite.Where(r => r.TeamId == model.TeamId && r.UserId == model.UserId).ToList().FirstOrDefault();
                            if (team != null)
                            {
                                team.IsFavourite = model.IsFavourite;
                                _db.TeamFavourite.Update(team);
                                _db.SaveChanges();
                            }
                            else
                            {
                                _db.TeamFavourite.Add(new TeamFavourite
                                {
                                    TeamId = model.TeamId,
                                    UserId = model.UserId,
                                    IsFavourite = model.IsFavourite,
                                    CreatedDateTime = DateTime.Now
                                });
                                _db.SaveChanges();
                            }
                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "Success"
                            });
                        }
                        else
                        {
                            strError = "IsFavourite is required";
                        }
                    }
                    else
                    {
                        strError = "UserId is required";
                    }
                }
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = string.IsNullOrEmpty(strError) ? "TeamId is required" : strError });
            }
            catch (Exception ex)
            {
                CommonDBHelper.ErrorLog("DashboardController - SetFavouriteTeam", ex.Message, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
            }
        }

        /// <summary>
        /// Set Follow/Unfollow Fixture
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///         "UserId": "a32321d9-9a58-4b23-bd66-489b07e02367",
        ///         "FixtureId": "888006",
        ///         "IsFollow": true
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [Route("SetFollowFixture")]
        public IActionResult SetFollowFixture([FromBody] FixtureFollowRequestModel model)
        {
            string strError = "";
            try
            {
                if (!string.IsNullOrEmpty(model.FixtureId))
                {
                    if (!string.IsNullOrEmpty(model.UserId))
                    {
                        if (!string.IsNullOrEmpty(model.IsFollow.ToString()))
                        {
                            FixtureFollow fixture = _db.FixtureFollow.Where(r => r.FixtureId == model.FixtureId && r.UserId == model.UserId).ToList().FirstOrDefault();
                            if (fixture != null)
                            {
                                fixture.IsFollow = model.IsFollow;
                                _db.FixtureFollow.Update(fixture);
                                _db.SaveChanges();
                            }
                            else
                            {
                                _db.FixtureFollow.Add(new FixtureFollow
                                {
                                    FixtureId = model.FixtureId,
                                    UserId = model.UserId,
                                    IsFollow = model.IsFollow,
                                    CreatedDateTime = DateTime.Now
                                });
                                _db.SaveChanges();
                            }
                            return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                            {
                                StatusCode = StatusCodes.Status200OK,
                                Message = "Success"
                            });
                        }
                        else
                        {
                            strError = "IsFollow is required";
                        }
                    }
                    else
                    {
                        strError = "UserId is required";
                    }
                }
                return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = string.IsNullOrEmpty(strError) ? "FixtureId is required" : strError });
            }
            catch (Exception ex)
            {
                CommonDBHelper.ErrorLog("DashboardController - SetFollowFixture", ex.Message, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
            }
        }

        /// <summary>
        /// Get Team Players by FixtureId
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///         "FixtureId": "871393"
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [Route("GetTeamPlayer")]
        public IActionResult GetTeamPlayer([FromBody] GetFixtureDataModel model)
        {
            List<HomeTeamPlayers> homeTeamPlayers = new List<HomeTeamPlayers>();
            List<AwayTeamPlayers> awayTeamPlayers = new List<AwayTeamPlayers>();
            List<HomeAwayTeamPlayersDetails> homeawayTeamList = new List<HomeAwayTeamPlayersDetails>();
            List<TeamDetails> teamDetails = new List<TeamDetails>();

            string url = "/fixtures?id=" + model.FixtureId;
            try
            {
                if (!string.IsNullOrEmpty(model.FixtureId))
                {
                    string ds = common.GetRequestDataAsString(url, 3);
                    if (ds != null)
                    {
                        var details = JObject.Parse(ds);
                        var data = details["response"][0]["players"];
                        var HomeTeam = details["response"][0]["teams"]["home"]["name"].ToString();
                        var AwayTeam = details["response"][0]["teams"]["away"]["name"].ToString();
                        if (data != null)
                        {
                            var TotalCount = data.Count();
                            if (TotalCount > 0)
                            {
                                for (int i = 0; i < TotalCount; i++)
                                {
                                    var TeamName = data[i]["team"]["name"].ToString();
                                    if (TeamName == HomeTeam)
                                    {
                                        var playerlist = data[i]["players"].Count();
                                        for (int j = 0; j < playerlist; j++)
                                        {
                                            homeTeamPlayers.Add(new HomeTeamPlayers
                                            {
                                                //TeamId = string.IsNullOrEmpty(ds.Tables["teams"].Rows[i]["team_id"].ToString()) ? "" : ds.Tables["teams"].Rows[i]["team_id"].ToString(),
                                                PlayerName = string.IsNullOrEmpty(data[i]["players"][j]["player"]["name"].ToString()) ? "" : data[i]["players"][j]["player"]["name"].ToString(),
                                                Photo = string.IsNullOrEmpty(data[i]["players"][j]["player"]["photo"].ToString()) ? "" : data[i]["players"][j]["player"]["photo"].ToString(),
                                                Position = string.IsNullOrEmpty(data[i]["players"][j]["statistics"][0]["games"]["position"].ToString()) ? "" : data[i]["players"][j]["statistics"][0]["games"]["position"].ToString(),
                                                Captain = string.IsNullOrEmpty(data[i]["players"][j]["statistics"][0]["games"]["captain"].ToString()) ? "" : data[i]["players"][j]["statistics"][0]["games"]["captain"].ToString(),
                                                TeamName = string.IsNullOrEmpty(data[i]["team"]["name"].ToString()) ? "" : data[i]["team"]["name"].ToString()
                                            });
                                        }

                                        homeTeamPlayers.Add(new HomeTeamPlayers
                                        {
                                            PlayerName = string.IsNullOrEmpty(details["response"][0]["lineups"][i]["coach"]["name"].ToString()) ? "" : details["response"][0]["lineups"][i]["coach"]["name"].ToString(),
                                            Photo = string.IsNullOrEmpty(details["response"][0]["lineups"][i]["coach"]["photo"].ToString()) ? "" : details["response"][0]["lineups"][i]["coach"]["photo"].ToString(),
                                            Position = "Coach",
                                            Captain = "false",
                                            TeamName = string.IsNullOrEmpty(data[i]["team"]["name"].ToString()) ? "" : data[i]["team"]["name"].ToString()
                                        });

                                    }
                                    else
                                    {
                                        var playerlist = data[i]["players"].Count();
                                        for (int j = 0; j < playerlist; j++)
                                        {
                                            awayTeamPlayers.Add(new AwayTeamPlayers
                                            {
                                                PlayerName = string.IsNullOrEmpty(data[i]["players"][j]["player"]["name"].ToString()) ? "" : data[i]["players"][j]["player"]["name"].ToString(),
                                                Photo = string.IsNullOrEmpty(data[i]["players"][j]["player"]["photo"].ToString()) ? "" : data[i]["players"][j]["player"]["photo"].ToString(),
                                                Position = string.IsNullOrEmpty(data[i]["players"][j]["statistics"][0]["games"]["position"].ToString()) ? "" : data[i]["players"][j]["statistics"][0]["games"]["position"].ToString(),
                                                Captain = string.IsNullOrEmpty(data[i]["players"][j]["statistics"][0]["games"]["captain"].ToString()) ? "" : data[i]["players"][j]["statistics"][0]["games"]["captain"].ToString(),
                                                TeamName = string.IsNullOrEmpty(data[i]["team"]["name"].ToString()) ? "" : data[i]["team"]["name"].ToString()
                                            });
                                        }

                                        awayTeamPlayers.Add(new AwayTeamPlayers
                                        {
                                            PlayerName = string.IsNullOrEmpty(details["response"][0]["lineups"][i]["coach"]["name"].ToString()) ? "" : details["response"][0]["lineups"][i]["coach"]["name"].ToString(),
                                            Photo = string.IsNullOrEmpty(details["response"][0]["lineups"][i]["coach"]["photo"].ToString()) ? "" : details["response"][0]["lineups"][i]["coach"]["photo"].ToString(),
                                            Position = "Coach",
                                            Captain = "false",
                                            TeamName = string.IsNullOrEmpty(data[i]["team"]["name"].ToString()) ? "" : data[i]["team"]["name"].ToString()
                                        });
                                    }
                                }
                                homeawayTeamList.Add(new HomeAwayTeamPlayersDetails
                                {
                                    HomeTeamPlayers = homeTeamPlayers,
                                    AwayTeamPlayers = awayTeamPlayers
                                });
                            }
                            else
                            {
                                teamDetails.Add(new TeamDetails
                                {
                                    LeagueId = string.IsNullOrEmpty(details["response"][0]["league"]["id"].ToString()) ? "" : details["response"][0]["league"]["id"].ToString(),
                                    HomeTeamId = string.IsNullOrEmpty(details["response"][0]["teams"]["home"]["id"].ToString()) ? "" : details["response"][0]["teams"]["home"]["id"].ToString(),
                                    HomeTeamName = string.IsNullOrEmpty(details["response"][0]["teams"]["home"]["name"].ToString()) ? "" : details["response"][0]["teams"]["home"]["name"].ToString(),
                                    AwayTeamId = string.IsNullOrEmpty(details["response"][0]["teams"]["away"]["id"].ToString()) ? "" : details["response"][0]["teams"]["away"]["id"].ToString(),
                                    AwayTeamName = string.IsNullOrEmpty(details["response"][0]["teams"]["away"]["name"].ToString()) ? "" : details["response"][0]["teams"]["away"]["name"].ToString(),
                                    FixtureId = string.IsNullOrEmpty(details["response"][0]["fixture"]["id"].ToString()) ? "" : details["response"][0]["fixture"]["id"].ToString(),
                                    Season = string.IsNullOrEmpty(details["response"][0]["league"]["season"].ToString()) ? "" : details["response"][0]["league"]["season"].ToString(),
                                });
                            }
                        }
                    }
                    if (homeawayTeamList.Count > 0)
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Success",
                            Result = homeawayTeamList
                        });
                    }
                    else
                    {
                        homeawayTeamList = GetPlayerDetailsUsingTeamId(teamDetails);
                    }
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Success",
                        Result = homeawayTeamList
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "FixtureId is required" });
                }
            }
            catch (Exception ex)
            {
                CommonDBHelper.ErrorLog("DashboardController - GetLiveMatches", ex.Message, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
            }
        }

        private List<HomeAwayTeamPlayersDetails> GetPlayerDetailsUsingTeamId(List<TeamDetails> teamDetails)
        {
            List<HomeAwayTeamPlayersDetails> homeAwayTeamPlayersDetails = new List<HomeAwayTeamPlayersDetails>();
            List<HomeTeamPlayers> homeTeamPlayers = new List<HomeTeamPlayers>();
            List<AwayTeamPlayers> awayTeamPlayers = new List<AwayTeamPlayers>();
            try
            {
                var TeamDetails = teamDetails.FirstOrDefault();
                List<string> TeamList = new List<string>();
                TeamList.Add(TeamDetails.HomeTeamId);
                TeamList.Add(TeamDetails.AwayTeamId);
                var HomeTeam = TeamDetails.HomeTeamName.ToString();
                var AwayTeam = TeamDetails.AwayTeamName.ToString();
                var HomeTeamId = TeamDetails.HomeTeamId.ToString();
                var AwayTeamId = TeamDetails.AwayTeamId.ToString();


                foreach (var TeamId in TeamList)
                {

                    var coachUrl = "/coachs?team=" + TeamId;
                    string coachData = common.GetRequestDataAsString(coachUrl, 3);
                    if (coachData != null)
                    {
                        var details = JObject.Parse(coachData);
                        var coachDetails = details["response"];
                        if (coachDetails != null)
                        {
                            var TotalCoach = coachDetails.Count();
                            if (TotalCoach > 0)
                            {
                                var HomeTeamCount = 0;
                                var AwayTeamcount = 0;
                                for (int i = 0; i < TotalCoach; i++)
                                {
                                    var CoachCareerList = details["response"][i]["career"].Count();
                                    for (int j = 0; j < CoachCareerList; j++)
                                    {
                                        var endDate = details["response"][i]["career"][j]["end"].ToString();
                                        if (endDate == "")
                                        {
                                            var TeamName = details["response"][i]["career"][j]["team"]["id"].ToString();
                                            if (TeamName == HomeTeamId && HomeTeamCount == 0)
                                            {
                                                homeTeamPlayers.Add(new HomeTeamPlayers
                                                {
                                                    PlayerName = string.IsNullOrEmpty(details["response"][i]["name"].ToString()) ? "" : details["response"][i]["name"].ToString(),
                                                    Photo = string.IsNullOrEmpty(details["response"][i]["photo"].ToString()) ? "" : details["response"][i]["photo"].ToString(),
                                                    Position = "Coach",
                                                    Captain = "false",
                                                    TeamName = string.IsNullOrEmpty(details["response"][i]["career"][j]["team"]["name"].ToString()) ? "" : details["response"][i]["career"][j]["team"]["name"].ToString(),
                                                });
                                                HomeTeamCount++;
                                            }
                                            if (TeamName == AwayTeamId && AwayTeamcount == 0)
                                            {
                                                awayTeamPlayers.Add(new AwayTeamPlayers
                                                {
                                                    PlayerName = string.IsNullOrEmpty(details["response"][i]["name"].ToString()) ? "" : details["response"][i]["name"].ToString(),
                                                    Photo = string.IsNullOrEmpty(details["response"][i]["photo"].ToString()) ? "" : details["response"][i]["photo"].ToString(),
                                                    Position = "Coach",
                                                    Captain = "false",
                                                    TeamName = string.IsNullOrEmpty(details["response"][i]["career"][j]["team"]["name"].ToString()) ? "" : details["response"][i]["career"][j]["team"]["name"].ToString(),
                                                });
                                                AwayTeamcount++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }


                    var url = "/players?team=" + TeamId + "&season=" + TeamDetails.Season;
                    string ds = common.GetRequestDataAsString(url, 3);
                    if (ds != null)
                    {
                        var details = JObject.Parse(ds);
                        var Playerdata = details["response"];
                        if (Playerdata != null)
                        {
                            var TotalData = Playerdata.Count();
                            if (TotalData > 0)
                            {
                                //var HomeTeam = Playerdata[0]["statistics"][0]["team"]["name"].ToString();
                                for (int i = 0; i < TotalData; i++)
                                {
                                    var team = Playerdata[i]["statistics"][0]["team"]["name"].ToString();
                                    if (HomeTeam == team)
                                    {
                                        homeTeamPlayers.Add(new HomeTeamPlayers
                                        {
                                            //TeamId = string.IsNullOrEmpty(ds.Tables["teams"].Rows[i]["team_id"].ToString()) ? "" : ds.Tables["teams"].Rows[i]["team_id"].ToString(),
                                            PlayerName = string.IsNullOrEmpty(Playerdata[i]["player"]["name"].ToString()) ? "" : Playerdata[i]["player"]["name"].ToString(),
                                            Photo = string.IsNullOrEmpty(Playerdata[i]["player"]["photo"].ToString()) ? "" : Playerdata[i]["player"]["photo"].ToString(),
                                            Position = string.IsNullOrEmpty(Playerdata[i]["statistics"][0]["games"]["position"].ToString()) ? "" : Playerdata[i]["statistics"][0]["games"]["position"].ToString(),
                                            Captain = string.IsNullOrEmpty(Playerdata[i]["statistics"][0]["games"]["captain"].ToString()) ? "" : Playerdata[i]["statistics"][0]["games"]["captain"].ToString(),
                                            TeamName = string.IsNullOrEmpty(Playerdata[i]["statistics"][0]["team"]["name"].ToString()) ? "" : Playerdata[i]["statistics"][0]["team"]["name"].ToString()
                                        });
                                    }
                                    else
                                    {
                                        awayTeamPlayers.Add(new AwayTeamPlayers
                                        {
                                            PlayerName = string.IsNullOrEmpty(Playerdata[i]["player"]["name"].ToString()) ? "" : Playerdata[i]["player"]["name"].ToString(),
                                            Photo = string.IsNullOrEmpty(Playerdata[i]["player"]["photo"].ToString()) ? "" : Playerdata[i]["player"]["photo"].ToString(),
                                            Position = string.IsNullOrEmpty(Playerdata[i]["statistics"][0]["games"]["position"].ToString()) ? "" : Playerdata[i]["statistics"][0]["games"]["position"].ToString(),
                                            Captain = string.IsNullOrEmpty(Playerdata[i]["statistics"][0]["games"]["captain"].ToString()) ? "" : Playerdata[i]["statistics"][0]["games"]["captain"].ToString(),
                                            TeamName = string.IsNullOrEmpty(Playerdata[i]["statistics"][0]["team"]["name"].ToString()) ? "" : Playerdata[i]["statistics"][0]["team"]["name"].ToString()
                                        });
                                    }
                                }
                            }
                        }
                    }


                }

                homeAwayTeamPlayersDetails.Add(new HomeAwayTeamPlayersDetails
                {
                    HomeTeamPlayers = homeTeamPlayers,
                    AwayTeamPlayers = awayTeamPlayers
                });

                return homeAwayTeamPlayersDetails;
            }
            catch (Exception ex)
            {
                CommonDBHelper.ErrorLog("DashboardController - GetPlayerDetailsUsingTeamId", ex.Message, ex.StackTrace);
                throw ex;
            }
        }

        /// <summary>
        /// Get Player Statistics Details
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///         "LeagueId": "73",
        ///         "Season": "2022"
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [Route("GetPlayerStatistics")]
        public IActionResult GetPlayerStatistics([FromBody] FixtureStandingsModel model)
        {
            List<PlayerStatisticModel> liveMatchList = new List<PlayerStatisticModel>();
            List<PlayerStatisticModelData> liveMatchData = new List<PlayerStatisticModelData>();
            try
            {
                if (!string.IsNullOrEmpty(model.LeagueId))
                {
                    if (!string.IsNullOrEmpty(model.Season))
                    {
                        DataSet ds = common.GetRequestData("/players/topscorers?league=" + model.LeagueId + "&season=" + model.Season, 3);
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            if (ds.Tables["player"] != null && ds.Tables["player"].Rows.Count > 0)
                            {
                                for (int i = 0; i < (ds.Tables["player"].Rows.Count > 10 ? 10 : ds.Tables["player"].Rows.Count); i++)
                                {
                                    liveMatchList.Add(new PlayerStatisticModel
                                    {
                                        Rank = i + 1,
                                        TeamLogo = ds.Tables["team"].Rows[i]["logo"].ToString(),
                                        TeamName = ds.Tables["team"].Rows[i]["name"].ToString(),
                                        PlayerName = ds.Tables["player"].Rows[i]["name"].ToString(),
                                        Total = ds.Tables["goals"].Rows[i]["total"].ToString()
                                    });
                                }
                            }
                        }
                        liveMatchData.Add(new PlayerStatisticModelData { KeyName = "Goal", Data = liveMatchList });

                        ds = common.GetRequestData("/players/topassists?league=" + model.LeagueId + "&season=" + model.Season, 3);
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            liveMatchList = new List<PlayerStatisticModel>();
                            if (ds.Tables["player"] != null && ds.Tables["player"].Rows.Count > 0)
                            {
                                for (int i = 0; i < (ds.Tables["player"].Rows.Count > 10 ? 10 : ds.Tables["player"].Rows.Count); i++)
                                {
                                    liveMatchList.Add(new PlayerStatisticModel
                                    {
                                        Rank = i + 1,
                                        TeamLogo = ds.Tables["team"].Rows[i]["logo"].ToString(),
                                        TeamName = ds.Tables["team"].Rows[i]["name"].ToString(),
                                        PlayerName = ds.Tables["player"].Rows[i]["name"].ToString(),
                                        Total = ds.Tables["goals"].Rows[i]["assists"].ToString()
                                    });
                                }
                            }
                        }
                        liveMatchData.Add(new PlayerStatisticModelData { KeyName = "ASSIST", Data = liveMatchList });

                        ds = common.GetRequestData("/players/topredcards?league=" + model.LeagueId + "&season=" + model.Season, 3);
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            liveMatchList = new List<PlayerStatisticModel>();
                            if (ds.Tables["player"] != null && ds.Tables["player"].Rows.Count > 0)
                            {
                                for (int i = 0; i < (ds.Tables["player"].Rows.Count > 10 ? 10 : ds.Tables["player"].Rows.Count); i++)
                                {
                                    liveMatchList.Add(new PlayerStatisticModel
                                    {
                                        Rank = i + 1,
                                        TeamLogo = ds.Tables["team"].Rows[i]["logo"].ToString(),
                                        TeamName = ds.Tables["team"].Rows[i]["name"].ToString(),
                                        PlayerName = ds.Tables["player"].Rows[i]["name"].ToString(),
                                        Total = ds.Tables["cards"].Rows[i]["red"].ToString()
                                    });
                                }
                            }
                        }
                        liveMatchData.Add(new PlayerStatisticModelData { KeyName = "Red Card", Data = liveMatchList });

                        ds = common.GetRequestData("/players/topyellowcards?league=" + model.LeagueId + "&season=" + model.Season, 3);
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            liveMatchList = new List<PlayerStatisticModel>();
                            if (ds.Tables["player"] != null && ds.Tables["player"].Rows.Count > 0)
                            {
                                for (int i = 0; i < (ds.Tables["player"].Rows.Count > 10 ? 10 : ds.Tables["player"].Rows.Count); i++)
                                {
                                    liveMatchList.Add(new PlayerStatisticModel
                                    {
                                        Rank = i + 1,
                                        TeamLogo = ds.Tables["team"].Rows[i]["logo"].ToString(),
                                        TeamName = ds.Tables["team"].Rows[i]["name"].ToString(),
                                        PlayerName = ds.Tables["player"].Rows[i]["name"].ToString(),
                                        Total = ds.Tables["cards"].Rows[i]["yellow"].ToString()
                                    });
                                }
                            }
                        }
                        liveMatchData.Add(new PlayerStatisticModelData { KeyName = "Yellow Card", Data = liveMatchList });

                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Success",
                            Result = liveMatchData
                        });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Season is required" });
                    }
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "LeagueId is required" });
                }
            }
            catch (Exception ex)
            {
                CommonDBHelper.ErrorLog("DashboardController - GetPlayerStatistics", ex.Message, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
            }
        }

        /// <summary>
        /// Get HeadToHead Details of Two Teams
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///         "TeamIds": "33,34"
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        [Route("GetHeadToHead")]
        public IActionResult GetHeadToHead([FromBody] HeadToHeadRequestModel model)
        {

            List<HeadToHead> headToHead = new List<HeadToHead>();
            List<HeadToHeadStatistics> headToHeadStatistics = new List<HeadToHeadStatistics>();
            List<HeadToHeadEvents> headToHeadEvents = new List<HeadToHeadEvents>();
            var TotalPlayedMatched = 0;
            try
            {
                if (!string.IsNullOrEmpty(model.TeamIds))
                {
                    var teamList = model.TeamIds.Split(',');
                    var TeamA = teamList[0];
                    var TeamB = teamList[1];
                    if (!string.IsNullOrEmpty(TeamA) && !string.IsNullOrEmpty(TeamB))
                    {
                        string url = "/fixtures/h2h/" + TeamA + "/" + TeamB;
                        DataSet ds = common.GetRequestData(url, 2, true);
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            if (ds.Tables["api"] != null && ds.Tables["api"].Rows.Count > 0)
                            {
                                if (ds.Tables["teams"] != null && ds.Tables["teams"].Rows.Count > 0)
                                {
                                    for (int j = 0; j < ds.Tables["teams"].Rows.Count; j++)
                                    {
                                        TotalPlayedMatched = Convert.ToInt32(ds.Tables["played"].Rows[0]["total"]);
                                        headToHeadStatistics.Add(new HeadToHeadStatistics
                                        {
                                            TeamName = string.IsNullOrEmpty(ds.Tables["teams"].Rows[j]["team_name"].ToString()) ? "" : ds.Tables["teams"].Rows[j]["team_name"].ToString(),
                                            WinMatch = string.IsNullOrEmpty(ds.Tables["wins"].Rows[j]["total"].ToString()) ? "" : ds.Tables["wins"].Rows[j]["total"].ToString(),
                                            DrawMatch = string.IsNullOrEmpty(ds.Tables["draws"].Rows[j]["total"].ToString()) ? "" : ds.Tables["draws"].Rows[j]["total"].ToString(),
                                        });
                                    }
                                }

                                if (ds.Tables["fixtures"] != null && ds.Tables["fixtures"].Rows.Count > 0)
                                {
                                    for (int i = 0; i < TotalPlayedMatched; i++)
                                    {
                                        headToHeadEvents.Add(new HeadToHeadEvents
                                        {
                                            Score = (string.IsNullOrEmpty(ds.Tables["fixtures"].Rows[i]["goalsHomeTeam"].ToString()) ? "0" : ds.Tables["fixtures"].Rows[i]["goalsHomeTeam"].ToString()) + "-" + (string.IsNullOrEmpty(ds.Tables["fixtures"].Rows[i]["goalsAwayTeam"].ToString()) ? "" : ds.Tables["fixtures"].Rows[i]["goalsAwayTeam"].ToString()),
                                            EventDate = string.IsNullOrEmpty(ds.Tables["fixtures"].Rows[i]["event_date"].ToString()) ? "" : Convert.ToDateTime(ds.Tables["fixtures"].Rows[i]["event_date"].ToString()).ToString("dd MMMM yyyy,ddd"),
                                            HomeTeam = string.IsNullOrEmpty(ds.Tables["homeTeam"].Rows[i]["team_name"].ToString()) ? "" : ds.Tables["homeTeam"].Rows[i]["team_name"].ToString(),
                                            AwayTeam = string.IsNullOrEmpty(ds.Tables["awayTeam"].Rows[i]["team_name"].ToString()) ? "" : ds.Tables["awayTeam"].Rows[i]["team_name"].ToString(),
                                        });
                                    }
                                }


                                headToHead.Add(new HeadToHead
                                {
                                    HeadToHeadstatistics = headToHeadStatistics,
                                    HeadToHeadEvents = headToHeadEvents
                                });

                            }
                        }
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel
                        {
                            StatusCode = StatusCodes.Status200OK,
                            Message = "Success",
                            Result = headToHead
                        });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Both TeamIds are required" });
                    }
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "TeamIds are required" });
                }
            }
            catch (Exception ex)
            {
                CommonDBHelper.ErrorLog("DashboardController - GetHeadToHead", ex.Message, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new APIResponseModel { StatusCode = StatusCodes.Status403Forbidden, Message = "Something Went Wrong." });
            }
        }
    }
}
