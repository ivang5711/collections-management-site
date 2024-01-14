using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Collections.Migrations
{
    /// <inheritdoc />
    public partial class FixesLikeAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemLike");

            migrationBuilder.AddColumn<int>(
                name: "ItemId",
                table: "Likes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Likes_ItemId",
                table: "Likes",
                column: "ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Items_ItemId",
                table: "Likes",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Items_ItemId",
                table: "Likes");

            migrationBuilder.DropIndex(
                name: "IX_Likes_ItemId",
                table: "Likes");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "Likes");

            migrationBuilder.CreateTable(
                name: "ItemLike",
                columns: table => new
                {
                    ItemsId = table.Column<int>(type: "int", nullable: false),
                    LikesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemLike", x => new { x.ItemsId, x.LikesId });
                    table.ForeignKey(
                        name: "FK_ItemLike_Items_ItemsId",
                        column: x => x.ItemsId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemLike_Likes_LikesId",
                        column: x => x.LikesId,
                        principalTable: "Likes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemLike_LikesId",
                table: "ItemLike",
                column: "LikesId");
        }
    }
}
