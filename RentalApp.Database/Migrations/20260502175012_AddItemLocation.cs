using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RentalApp.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddItemLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS postgis;");

            migrationBuilder.AddColumn<Point>(
                name: "Location",
                table: "items",
                type: "geography (point)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "items");
        }
    }
}
