using Microsoft.EntityFrameworkCore.Migrations;

namespace FileMetadataAPI.Migrations
{
    /// <inheritdoc />
    public partial class MakePermissionEnumInFileShare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Permission sütununu geçici olarak yeniden adlandır
            migrationBuilder.RenameColumn(
                name: "Permission",
                table: "FileShares",
                newName: "Permission_Old");

            // 2. Yeni integer türünde Permission sütunu ekle
            migrationBuilder.AddColumn<int>(
                name: "Permission",
                table: "FileShares",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // 3. Eski Permission_Old sütununu kaldır
            migrationBuilder.DropColumn(
                name: "Permission_Old",
                table: "FileShares");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Permission sütununu geçici olarak yeniden adlandır
            migrationBuilder.RenameColumn(
                name: "Permission",
                table: "FileShares",
                newName: "Permission_Old");

            // 2. Eski text türünde Permission sütunu ekle
            migrationBuilder.AddColumn<string>(
                name: "Permission",
                table: "FileShares",
                type: "text",
                nullable: false,
                defaultValue: "");

            // 3. Eski Permission_Old sütununu kaldır
            migrationBuilder.DropColumn(
                name: "Permission_Old",
                table: "FileShares");
        }
    }
}
