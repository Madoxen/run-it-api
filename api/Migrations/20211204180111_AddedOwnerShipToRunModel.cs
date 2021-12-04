using Microsoft.EntityFrameworkCore.Migrations;

namespace run_it_api.Migrations
{
    public partial class AddedOwnerShipToRunModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Runs_Users_UserId",
                table: "Runs");

            migrationBuilder.RenameColumn(
                name: "Points",
                table: "Runs",
                newName: "RawPoints");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Runs",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Runs_Users_UserId",
                table: "Runs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Runs_Users_UserId",
                table: "Runs");

            migrationBuilder.RenameColumn(
                name: "RawPoints",
                table: "Runs",
                newName: "Points");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Runs",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Runs_Users_UserId",
                table: "Runs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
