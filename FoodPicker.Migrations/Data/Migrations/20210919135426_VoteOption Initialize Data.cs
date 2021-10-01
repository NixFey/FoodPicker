using Microsoft.EntityFrameworkCore.Migrations;

namespace FoodPicker.Web.Data.Migrations
{
    public partial class VoteOptionInitializeData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData("VoteOptions", new []{"Name", "Weight"}, new[]{"Yes", "1.0"});
            migrationBuilder.InsertData("VoteOptions", new []{"Name", "Weight"}, new[]{"Maybe", "0.5"});
            migrationBuilder.InsertData("VoteOptions", new []{"Name", "Weight"}, new[]{"No", "0.0"});
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
