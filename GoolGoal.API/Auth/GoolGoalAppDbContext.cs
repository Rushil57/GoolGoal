using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GoolGoal.API.Models;

namespace GoolGoal.API.Auth
{
	public class GoolGoalAppDbContext : IdentityDbContext<ApplicationUser>
	{
		public GoolGoalAppDbContext(DbContextOptions<GoolGoalAppDbContext> options) : base(options)
		{
		}
        public DbSet<LeagueFollow> LeagueFollow { get; set; }
        public DbSet<TeamFavourite> TeamFavourite { get; set; }
        public DbSet<FixtureFollow> FixtureFollow { get; set; }
        public DbSet<StoreAPIResponse> StoreAPIResponse { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);
            /* Comment For Live Start  */

            //builder.Entity<ApplicationUser>(entity =>
            //{
            //    entity.ToTable(name: "dbo.aspNetUsers");
            //    entity.Property(e => e.Id).HasColumnName("UserId");
            //});

            /* Comment For Live END  */
        }
    }
}
