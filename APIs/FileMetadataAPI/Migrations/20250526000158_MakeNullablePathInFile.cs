using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileMetadataAPI.Migrations
{
    /// <inheritdoc />
    public partial class MakeNullablePathInFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddColumn<int>(
            //    name: "Id",
            //    table: "FileShares",
            //    type: "integer",
            //    nullable: false,
            //    defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Files",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            //migrationBuilder.AddColumn<string>(
            //    name: "Path",
            //    table: "Files",
            //    type: "text",
            //    nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "FileShares");

            migrationBuilder.DropColumn(
                name: "Path",
                table: "Files");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Files",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
