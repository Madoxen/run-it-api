using Microsoft.EntityFrameworkCore.Migrations;

namespace run_it_api.Migrations
{
    public partial class removedStatusFromRouteShares : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "RouteShares");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "RouteShares",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
