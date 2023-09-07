using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodPicker.Migrations.Migrations
{
    public partial class Adduseractivebool : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MealVotes_VoteOptions_VoteOptionId",
                table: "MealVotes");

            migrationBuilder.AlterColumn<int>(
                name: "VoteOptionId",
                table: "MealVotes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MealVotes_VoteOptions_VoteOptionId",
                table: "MealVotes",
                column: "VoteOptionId",
                principalTable: "VoteOptions",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MealVotes_VoteOptions_VoteOptionId",
                table: "MealVotes");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<int>(
                name: "VoteOptionId",
                table: "MealVotes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MealVotes_VoteOptions_VoteOptionId",
                table: "MealVotes",
                column: "VoteOptionId",
                principalTable: "VoteOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
