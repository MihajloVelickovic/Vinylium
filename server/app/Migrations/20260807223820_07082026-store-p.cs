using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace app.Migrations
{
    /// <inheritdoc />
    public partial class _07082026storep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "StoreStocks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "StoreStocks",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
