using Microsoft.EntityFrameworkCore.Migrations;

namespace FoodPicker.Data.Migrations
{
    public partial class Optionalvoters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "VoteIsRequired",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_MealVotes_UserId",
                table: "MealVotes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MealVotes_AspNetUsers_UserId",
                table: "MealVotes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MealVotes_AspNetUsers_UserId",
                table: "MealVotes");

            migrationBuilder.DropIndex(
                name: "IX_MealVotes_UserId",
                table: "MealVotes");

            migrationBuilder.DropColumn(
                name: "VoteIsRequired",
                table: "AspNetUsers");
        }
    }
}
