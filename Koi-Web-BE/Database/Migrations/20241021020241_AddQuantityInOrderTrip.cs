using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koi_Web_BE.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddQuantityInOrderTrip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "OrderTrips",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "OrderTrips");
        }
    }
}
