using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Collections.Migrations
{
    /// <inheritdoc />
    public partial class CustomFieldsAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DateFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DateFields", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogicalFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogicalFields", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StringFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StringFields", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TextFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextFields", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DateFieldItem",
                columns: table => new
                {
                    DateFieldsId = table.Column<int>(type: "int", nullable: false),
                    ItemsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DateFieldItem", x => new { x.DateFieldsId, x.ItemsId });
                    table.ForeignKey(
                        name: "FK_DateFieldItem_DateFields_DateFieldsId",
                        column: x => x.DateFieldsId,
                        principalTable: "DateFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DateFieldItem_Items_ItemsId",
                        column: x => x.ItemsId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemLogicalField",
                columns: table => new
                {
                    ItemsId = table.Column<int>(type: "int", nullable: false),
                    LogicalFieldsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemLogicalField", x => new { x.ItemsId, x.LogicalFieldsId });
                    table.ForeignKey(
                        name: "FK_ItemLogicalField_Items_ItemsId",
                        column: x => x.ItemsId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemLogicalField_LogicalFields_LogicalFieldsId",
                        column: x => x.LogicalFieldsId,
                        principalTable: "LogicalFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemStringField",
                columns: table => new
                {
                    ItemsId = table.Column<int>(type: "int", nullable: false),
                    StringFieldsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemStringField", x => new { x.ItemsId, x.StringFieldsId });
                    table.ForeignKey(
                        name: "FK_ItemStringField_Items_ItemsId",
                        column: x => x.ItemsId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemStringField_StringFields_StringFieldsId",
                        column: x => x.StringFieldsId,
                        principalTable: "StringFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemTextField",
                columns: table => new
                {
                    ItemsId = table.Column<int>(type: "int", nullable: false),
                    TextFieldsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemTextField", x => new { x.ItemsId, x.TextFieldsId });
                    table.ForeignKey(
                        name: "FK_ItemTextField_Items_ItemsId",
                        column: x => x.ItemsId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemTextField_TextFields_TextFieldsId",
                        column: x => x.TextFieldsId,
                        principalTable: "TextFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DateFieldItem_ItemsId",
                table: "DateFieldItem",
                column: "ItemsId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemLogicalField_LogicalFieldsId",
                table: "ItemLogicalField",
                column: "LogicalFieldsId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemStringField_StringFieldsId",
                table: "ItemStringField",
                column: "StringFieldsId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemTextField_TextFieldsId",
                table: "ItemTextField",
                column: "TextFieldsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DateFieldItem");

            migrationBuilder.DropTable(
                name: "ItemLogicalField");

            migrationBuilder.DropTable(
                name: "ItemStringField");

            migrationBuilder.DropTable(
                name: "ItemTextField");

            migrationBuilder.DropTable(
                name: "DateFields");

            migrationBuilder.DropTable(
                name: "LogicalFields");

            migrationBuilder.DropTable(
                name: "StringFields");

            migrationBuilder.DropTable(
                name: "TextFields");
        }
    }
}
