using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWD392_PROJECT.Migrations
{
    /// <inheritdoc />
    public partial class AddProductEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SellingTime = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                });

            migrationBuilder.CreateTable(
                name: "StallProducts",
                columns: table => new
                {
                    StallProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    StallId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StallProducts", x => x.StallProductId);
                    table.ForeignKey(
                        name: "FK_StallProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StallProducts_Stalls_StallId",
                        column: x => x.StallId,
                        principalTable: "Stalls",
                        principalColumn: "StallId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StallProducts_ProductId",
                table: "StallProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StallProducts_StallId",
                table: "StallProducts",
                column: "StallId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StallProducts");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
