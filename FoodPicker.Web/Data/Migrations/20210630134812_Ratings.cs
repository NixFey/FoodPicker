using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FoodPicker.Web.Data.Migrations
{
    public partial class Ratings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MealRating_Meals_MealId",
                table: "MealRating");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MealRating",
                table: "MealRating");

            migrationBuilder.RenameTable(
                name: "MealRating",
                newName: "MealRatings");

            migrationBuilder.RenameIndex(
                name: "IX_MealRating_MealId",
                table: "MealRatings",
                newName: "IX_MealRatings_MealId");

            migrationBuilder.AddColumn<DateTime>(
                name: "RatingTime",
                table: "MealRatings",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_MealRatings",
                table: "MealRatings",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MealRatings_Meals_MealId",
                table: "MealRatings",
                column: "MealId",
                principalTable: "Meals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MealRatings_Meals_MealId",
                table: "MealRatings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MealRatings",
                table: "MealRatings");

            migrationBuilder.DropColumn(
                name: "RatingTime",
                table: "MealRatings");

            migrationBuilder.RenameTable(
                name: "MealRatings",
                newName: "MealRating");

            migrationBuilder.RenameIndex(
                name: "IX_MealRatings_MealId",
                table: "MealRating",
                newName: "IX_MealRating_MealId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MealRating",
                table: "MealRating",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MealRating_Meals_MealId",
                table: "MealRating",
                column: "MealId",
                principalTable: "Meals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
