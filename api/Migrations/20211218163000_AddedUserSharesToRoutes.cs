using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace run_it_api.Migrations
{
    public partial class AddedUserSharesToRoutes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RouteShares",
                columns: table => new
                {
                    RouteId = table.Column<int>(type: "integer", nullable: false),
                    SharedToId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteShares", x => new { x.RouteId, x.SharedToId });
                    table.ForeignKey(
                        name: "FK_RouteShares_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RouteShares_Users_SharedToId",
                        column: x => x.SharedToId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RouteShares_SharedToId",
                table: "RouteShares",
                column: "SharedToId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RouteShares");
        }
    }
}
