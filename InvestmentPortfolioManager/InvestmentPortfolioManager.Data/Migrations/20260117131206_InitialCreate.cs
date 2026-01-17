using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvestmentPortfolioManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Portfolios",
                columns: table => new
                {
                    InvestmentPortfolioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Owner = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Portfolios", x => x.InvestmentPortfolioId);
                });

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Asset_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvestmentPortfolioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchasePrice = table.Column<double>(type: "float", nullable: false),
                    Volatility = table.Column<double>(type: "float", nullable: false),
                    MeanReturn = table.Column<double>(type: "float", nullable: false),
                    LowPriceThreshold = table.Column<double>(type: "float", nullable: true),
                    AssetName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssetSymbol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<double>(type: "float", nullable: false),
                    CurrentPrice = table.Column<double>(type: "float", nullable: false),
                    AssetType = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                    Rate = table.Column<double>(type: "float", nullable: true),
                    Unit = table.Column<int>(type: "int", nullable: true),
                    Street = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HouseNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FlatNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZipCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Asset_id);
                    table.ForeignKey(
                        name: "FK_Assets_Portfolios_InvestmentPortfolioId",
                        column: x => x.InvestmentPortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "InvestmentPortfolioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_InvestmentPortfolioId",
                table: "Assets",
                column: "InvestmentPortfolioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "Portfolios");
        }
    }
}
