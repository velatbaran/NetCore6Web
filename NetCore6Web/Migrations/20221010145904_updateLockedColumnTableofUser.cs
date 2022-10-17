using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetCore6Web.Migrations
{
    public partial class updateLockedColumnTableofUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Locked",
                table: "Users",
                newName: "IsLocked");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsLocked",
                table: "Users",
                newName: "Locked");
        }
    }
}
