using Microsoft.EntityFrameworkCore.Migrations;

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

            migrationBuilder.Sql(
                @"ALTER TABLE items ALTER COLUMN ""Location"" TYPE geometry USING ""Location""::geometry;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.Sql(
                @"ALTER TABLE items ALTER COLUMN ""Location"" TYPE geography(Point,4326) USING ""Location""::geography;");
        }
    }
}
