using Microsoft.EntityFrameworkCore.Migrations;

namespace run_it_api.Migrations
{
    public partial class AddStatusFieldToRouteShare : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "RouteShares",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "RouteShares");
        }
    }
}
