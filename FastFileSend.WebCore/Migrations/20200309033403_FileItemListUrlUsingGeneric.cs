using Microsoft.EntityFrameworkCore.Migrations;

namespace FastFileSend.WebCore.Migrations
{
    public partial class FileItemListUrlUsingGeneric : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UriListStr",
                table: "Files");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Files",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Url",
                table: "Files");

            migrationBuilder.AddColumn<string>(
                name: "UriListStr",
                table: "Files",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
