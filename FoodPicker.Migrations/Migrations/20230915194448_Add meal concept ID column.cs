using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodPicker.Migrations.Migrations
{
    public partial class AddmealconceptIDcolumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MealConceptId",
                table: "Meals",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MealConceptId",
                table: "Meals");
        }
    }
}
