using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPark.Asset.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Area = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Location = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Model = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CurrentWorkOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_assets_AssetCode",
                table: "assets",
                column: "AssetCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assets");
        }
    }
}
