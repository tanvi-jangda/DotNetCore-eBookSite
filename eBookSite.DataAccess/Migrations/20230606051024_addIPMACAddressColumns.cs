using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eBookSite.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addIPMACAddressColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientIPAddress",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientMACAddress",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientIPAddress",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ClientMACAddress",
                table: "AspNetUsers");
        }
    }
}
