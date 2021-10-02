using Microsoft.EntityFrameworkCore.Migrations;

namespace FoodPicker.Web.Data.Migrations
{
    public partial class AddWeekUserComments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeekUserComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    WeekId = table.Column<int>(type: "INTEGER", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeekUserComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeekUserComments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WeekUserComments_MealWeeks_WeekId",
                        column: x => x.WeekId,
                        principalTable: "MealWeeks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WeekUserComments_UserId",
                table: "WeekUserComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WeekUserComments_WeekId",
                table: "WeekUserComments",
                column: "WeekId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeekUserComments");
        }
    }
}
