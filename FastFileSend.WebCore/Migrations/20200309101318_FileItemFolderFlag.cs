using Microsoft.EntityFrameworkCore.Migrations;

namespace FastFileSend.WebCore.Migrations
{
    public partial class FileItemFolderFlag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Folder",
                table: "Files",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Folder",
                table: "Files");
        }
    }
}
