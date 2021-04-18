using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Web.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Leagues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: false),
                    Hockey_Category_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leagues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Teamname = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    League_ID = table.Column<int>(type: "int", nullable: true),
                    Sponsor = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Movement = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    SeasonDrawn = table.Column<int>(type: "int", nullable: false),
                    SeasonGoalDifference = table.Column<int>(type: "int", nullable: false),
                    SeasonGoalsAgainst = table.Column<int>(type: "int", nullable: false),
                    SeasonGoalsFor = table.Column<int>(type: "int", nullable: false),
                    SeasonLost = table.Column<int>(type: "int", nullable: false),
                    SeasonPlayed = table.Column<int>(type: "int", nullable: false),
                    SeasonPoints = table.Column<int>(type: "int", nullable: false),
                    SeasonWon = table.Column<int>(type: "int", nullable: false),
                    TotalDrawn = table.Column<int>(type: "int", nullable: false),
                    TotalGoalDifference = table.Column<int>(type: "int", nullable: false),
                    TotalGoalsAgainst = table.Column<int>(type: "int", nullable: false),
                    TotalGoalsFor = table.Column<int>(type: "int", nullable: false),
                    TotalLost = table.Column<int>(type: "int", nullable: false),
                    TotalPlayed = table.Column<int>(type: "int", nullable: false),
                    TotalPoints = table.Column<int>(type: "int", nullable: false),
                    TotalWon = table.Column<int>(type: "int", nullable: false),
                    Hockey_Category_ID = table.Column<int>(type: "int", nullable: true),
                    League_Rank = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Leagues");

            migrationBuilder.DropTable(
                name: "Teams");
        }
    }
}
