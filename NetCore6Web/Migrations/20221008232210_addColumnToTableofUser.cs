using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetCore6Web.Migrations
{
    public partial class addColumnToTableofUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRemoved",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRemoved",
                table: "Users");
        }
    }
}
