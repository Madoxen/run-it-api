using Microsoft.EntityFrameworkCore.Migrations;

namespace run_it_api.Migrations
{
    public partial class AddedDistanceStatsToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DistanceLast30Days",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "DistanceTotal",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistanceLast30Days",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DistanceTotal",
                table: "Users");
        }
    }
}
