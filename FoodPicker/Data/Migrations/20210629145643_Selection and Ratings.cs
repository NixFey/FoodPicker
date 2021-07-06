using Microsoft.EntityFrameworkCore.Migrations;

namespace FoodPicker.Data.Migrations
{
    public partial class SelectionandRatings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SelectedForOrder",
                table: "Meals",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MealRating",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MealId = table.Column<int>(type: "INTEGER", nullable: false),
                    Rating = table.Column<int>(type: "INTEGER", nullable: false),
                    RatingComment = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealRating", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MealRating_Meals_MealId",
                        column: x => x.MealId,
                        principalTable: "Meals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MealRating_MealId",
                table: "MealRating",
                column: "MealId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MealRating");

            migrationBuilder.DropColumn(
                name: "SelectedForOrder",
                table: "Meals");
        }
    }
}
