using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eBookSite.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class imageurl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] {  "ImageURL" },
                values: new object[] { "" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
               columns: new[] { "ImageURL" },
                values: new object[] { "" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
              columns: new[] { "ImageURL" },
                values: new object[] { "" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ImageURL" },
                values: new object[] { "" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
             columns: new[] { "ImageURL" },
                values: new object[] { "" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
               columns: new[] { "ImageURL" },
                values: new object[] { "" });

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "ImageURL",
                table: "Products");
        }
    }
}
