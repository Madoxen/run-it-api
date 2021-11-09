using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace run_it_api.Migrations
{
    public partial class AddedWeightField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Run_Users_UserId",
                table: "Run");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Run",
                table: "Run");

            migrationBuilder.RenameTable(
                name: "Run",
                newName: "Runs");

            migrationBuilder.RenameIndex(
                name: "IX_Run_UserId",
                table: "Runs",
                newName: "IX_Runs_UserId");

            migrationBuilder.AddColumn<float>(
                name: "Weight",
                table: "Users",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Runs",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Runs",
                table: "Runs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Runs_Users_UserId",
                table: "Runs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Runs_Users_UserId",
                table: "Runs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Runs",
                table: "Runs");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "Runs");

            migrationBuilder.RenameTable(
                name: "Runs",
                newName: "Run");

            migrationBuilder.RenameIndex(
                name: "IX_Runs_UserId",
                table: "Run",
                newName: "IX_Run_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Run",
                table: "Run",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Run_Users_UserId",
                table: "Run",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
