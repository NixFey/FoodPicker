using Microsoft.EntityFrameworkCore.Migrations;

namespace FoodPicker.Web.Data.Migrations
{
    public partial class UniqueMealVotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete from MealVotes where Id not in (select max(Id) from MealVotes group by MealId, UserId)");
            
            migrationBuilder.DropIndex(
                name: "IX_MealVotes_MealId",
                table: "MealVotes");

            migrationBuilder.CreateIndex(
                name: "IX_MealVotes_MealId_UserId",
                table: "MealVotes",
                columns: new[] { "MealId", "UserId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MealVotes_MealId_UserId",
                table: "MealVotes");

            migrationBuilder.CreateIndex(
                name: "IX_MealVotes_MealId",
                table: "MealVotes",
                column: "MealId");
        }
    }
}
