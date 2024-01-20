using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Collections.Migrations
{
    /// <inheritdoc />
    public partial class NumericalFieldsAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LikesTotal",
                table: "Items");

            migrationBuilder.CreateTable(
                name: "NumericalFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NumericalFields", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemNumericalField",
                columns: table => new
                {
                    ItemsId = table.Column<int>(type: "int", nullable: false),
                    NumericalFieldsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemNumericalField", x => new { x.ItemsId, x.NumericalFieldsId });
                    table.ForeignKey(
                        name: "FK_ItemNumericalField_Items_ItemsId",
                        column: x => x.ItemsId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemNumericalField_NumericalFields_NumericalFieldsId",
                        column: x => x.NumericalFieldsId,
                        principalTable: "NumericalFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemNumericalField_NumericalFieldsId",
                table: "ItemNumericalField",
                column: "NumericalFieldsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemNumericalField");

            migrationBuilder.DropTable(
                name: "NumericalFields");

            migrationBuilder.AddColumn<int>(
                name: "LikesTotal",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
