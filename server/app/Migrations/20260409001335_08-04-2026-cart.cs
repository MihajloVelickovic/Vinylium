using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace app.Migrations
{
    /// <inheritdoc />
    public partial class _08042026cart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductUser_Products_WishlistBarcode",
                table: "ProductUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductUser",
                table: "ProductUser");

            migrationBuilder.DropIndex(
                name: "IX_ProductUser_WishlistBarcode",
                table: "ProductUser");

            migrationBuilder.RenameColumn(
                name: "WishlistBarcode",
                table: "ProductUser",
                newName: "CartBarcode");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductUser",
                table: "ProductUser",
                columns: new[] { "CartBarcode", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductUser_UserId",
                table: "ProductUser",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductUser_Products_CartBarcode",
                table: "ProductUser",
                column: "CartBarcode",
                principalTable: "Products",
                principalColumn: "Barcode",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductUser_Products_CartBarcode",
                table: "ProductUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductUser",
                table: "ProductUser");

            migrationBuilder.DropIndex(
                name: "IX_ProductUser_UserId",
                table: "ProductUser");

            migrationBuilder.RenameColumn(
                name: "CartBarcode",
                table: "ProductUser",
                newName: "WishlistBarcode");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductUser",
                table: "ProductUser",
                columns: new[] { "UserId", "WishlistBarcode" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductUser_WishlistBarcode",
                table: "ProductUser",
                column: "WishlistBarcode");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductUser_Products_WishlistBarcode",
                table: "ProductUser",
                column: "WishlistBarcode",
                principalTable: "Products",
                principalColumn: "Barcode",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
