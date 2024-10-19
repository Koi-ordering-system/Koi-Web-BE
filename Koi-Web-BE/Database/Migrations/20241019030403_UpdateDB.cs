using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koi_Web_BE.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Farms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Owner = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Rating = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Farms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kois",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    MinSize = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxSize = table.Column<decimal>(type: "numeric", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kois", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    AvatarUrl = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FarmImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FarmId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FarmImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FarmImages_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trips",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FarmId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: true),
                    Days = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trips_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Colors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KoiId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Colors_Kois_KoiId",
                        column: x => x.KoiId,
                        principalTable: "Kois",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FarmKois",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FarmId = table.Column<Guid>(type: "uuid", nullable: false),
                    KoiId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FarmKois", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FarmKois_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FarmKois_Kois_KoiId",
                        column: x => x.KoiId,
                        principalTable: "Kois",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KoiImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KoiId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KoiImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KoiImages_Kois_KoiId",
                        column: x => x.KoiId,
                        principalTable: "Kois",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    PayOSOrderCode = table.Column<long>(type: "bigint", nullable: true),
                    FarmId = table.Column<Guid>(type: "uuid", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    PrePaidPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderKois",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    KoiId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    ColorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Size = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderKois", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderKois_Colors_ColorId",
                        column: x => x.ColorId,
                        principalTable: "Colors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderKois_Kois_KoiId",
                        column: x => x.KoiId,
                        principalTable: "Kois",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderKois_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderTrips",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderTrips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderTrips_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderTrips_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    FarmId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reviews_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Colors_KoiId",
                table: "Colors",
                column: "KoiId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmImages_FarmId",
                table: "FarmImages",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmKois_FarmId",
                table: "FarmKois",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmKois_KoiId",
                table: "FarmKois",
                column: "KoiId");

            migrationBuilder.CreateIndex(
                name: "IX_KoiImages_KoiId",
                table: "KoiImages",
                column: "KoiId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderKois_ColorId",
                table: "OrderKois",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderKois_KoiId",
                table: "OrderKois",
                column: "KoiId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderKois_OrderId",
                table: "OrderKois",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_FarmId",
                table: "Orders",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTrips_OrderId",
                table: "OrderTrips",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderTrips_TripId",
                table: "OrderTrips",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_FarmId",
                table: "Reviews",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_OrderId",
                table: "Reviews",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_FarmId",
                table: "Trips",
                column: "FarmId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FarmImages");

            migrationBuilder.DropTable(
                name: "FarmKois");

            migrationBuilder.DropTable(
                name: "KoiImages");

            migrationBuilder.DropTable(
                name: "OrderKois");

            migrationBuilder.DropTable(
                name: "OrderTrips");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "Colors");

            migrationBuilder.DropTable(
                name: "Trips");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Kois");

            migrationBuilder.DropTable(
                name: "Farms");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
