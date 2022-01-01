using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickRefsServer.Migrations
{
    public partial class RenameTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_tags",
                table: "tags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_knowledgeTags",
                table: "knowledgeTags");

            migrationBuilder.RenameTable(
                name: "tags",
                newName: "Tags");

            migrationBuilder.RenameTable(
                name: "knowledgeTags",
                newName: "KnowledgeTags");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tags",
                table: "Tags",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KnowledgeTags",
                table: "KnowledgeTags",
                columns: new[] { "KnowledgeId", "TagId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Tags",
                table: "Tags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KnowledgeTags",
                table: "KnowledgeTags");

            migrationBuilder.RenameTable(
                name: "Tags",
                newName: "tags");

            migrationBuilder.RenameTable(
                name: "KnowledgeTags",
                newName: "knowledgeTags");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tags",
                table: "tags",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_knowledgeTags",
                table: "knowledgeTags",
                columns: new[] { "KnowledgeId", "TagId" });
        }
    }
}
