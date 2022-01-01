using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickRefsServer.Migrations
{
    public partial class CreateKnowledgeTag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "knowledgeTags",
                columns: table => new
                {
                    KnowledgeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_knowledgeTags", x => new { x.KnowledgeId, x.TagId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "knowledgeTags");
        }
    }
}
