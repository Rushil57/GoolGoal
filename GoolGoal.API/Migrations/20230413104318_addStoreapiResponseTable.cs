using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoolGoal.API.Migrations
{
    /// <inheritdoc />
    public partial class addStoreapiResponseTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StoreAPIResponse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FixtureId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    APIName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    APIResponse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreAPIResponse", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoreAPIResponse");
        }
    }
}
