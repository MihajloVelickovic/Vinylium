using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace app.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Tracklist = table.Column<string>(type: "TEXT", nullable: false),
                    Runtime = table.Column<string>(type: "TEXT", maxLength: 11, nullable: false),
                    ReleaseDate = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    InWarehouse = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 254, nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 254, nullable: false),
                    Password = table.Column<string>(type: "TEXT", maxLength: 254, nullable: false),
                    Admin = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductStore",
                columns: table => new
                {
                    AvailableAtId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductStore", x => new { x.AvailableAtId, x.ProductsId });
                    table.ForeignKey(
                        name: "FK_ProductStore_Locations_AvailableAtId",
                        column: x => x.AvailableAtId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductStore_Products_ProductsId",
                        column: x => x.ProductsId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductUser",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    WishlistId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductUser", x => new { x.UserId, x.WishlistId });
                    table.ForeignKey(
                        name: "FK_ProductUser_Products_WishlistId",
                        column: x => x.WishlistId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductUser_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductWarehouse",
                columns: table => new
                {
                    ProductsId = table.Column<int>(type: "INTEGER", nullable: false),
                    WarehouseId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductWarehouse", x => new { x.ProductsId, x.WarehouseId });
                    table.ForeignKey(
                        name: "FK_ProductWarehouse_Products_ProductsId",
                        column: x => x.ProductsId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoreVinylium",
                columns: table => new
                {
                    StoresId = table.Column<int>(type: "INTEGER", nullable: false),
                    VinyliumId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreVinylium", x => new { x.StoresId, x.VinyliumId });
                    table.ForeignKey(
                        name: "FK_StoreVinylium_Locations_StoresId",
                        column: x => x.StoresId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserVinylium",
                columns: table => new
                {
                    UsersId = table.Column<int>(type: "INTEGER", nullable: false),
                    VinyliumId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVinylium", x => new { x.UsersId, x.VinyliumId });
                    table.ForeignKey(
                        name: "FK_UserVinylium_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vinylium",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WarehouseId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vinylium", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VinyliumId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Warehouses_Vinylium_VinyliumId",
                        column: x => x.VinyliumId,
                        principalTable: "Vinylium",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductStore_ProductsId",
                table: "ProductStore",
                column: "ProductsId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductUser_WishlistId",
                table: "ProductUser",
                column: "WishlistId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductWarehouse_WarehouseId",
                table: "ProductWarehouse",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreVinylium_VinyliumId",
                table: "StoreVinylium",
                column: "VinyliumId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username_Password",
                table: "Users",
                columns: new[] { "Username", "Password" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserVinylium_VinyliumId",
                table: "UserVinylium",
                column: "VinyliumId");

            migrationBuilder.CreateIndex(
                name: "IX_Vinylium_WarehouseId",
                table: "Vinylium",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_VinyliumId",
                table: "Warehouses",
                column: "VinyliumId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductWarehouse_Warehouses_WarehouseId",
                table: "ProductWarehouse",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StoreVinylium_Vinylium_VinyliumId",
                table: "StoreVinylium",
                column: "VinyliumId",
                principalTable: "Vinylium",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserVinylium_Vinylium_VinyliumId",
                table: "UserVinylium",
                column: "VinyliumId",
                principalTable: "Vinylium",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vinylium_Warehouses_WarehouseId",
                table: "Vinylium",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vinylium_Warehouses_WarehouseId",
                table: "Vinylium");

            migrationBuilder.DropTable(
                name: "ProductStore");

            migrationBuilder.DropTable(
                name: "ProductUser");

            migrationBuilder.DropTable(
                name: "ProductWarehouse");

            migrationBuilder.DropTable(
                name: "StoreVinylium");

            migrationBuilder.DropTable(
                name: "UserVinylium");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropTable(
                name: "Vinylium");
        }
    }
}
