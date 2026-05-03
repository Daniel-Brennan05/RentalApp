using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RentalApp.Database.Migrations
{
    /// <inheritdoc />
    public partial class PendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.AlterColumn<Point>(
                name: "Location",
                table: "items",
                type: "geometry",
                nullable: true,
                oldClrType: typeof(Point),
                oldType: "geography (point)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.AlterColumn<Point>(
                name: "Location",
                table: "items",
                type: "geography (point)",
                nullable: true,
                oldClrType: typeof(Point),
                oldType: "geometry",
                oldNullable: true);
        }
    }
}
