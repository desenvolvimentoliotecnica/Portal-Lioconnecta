using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalLioConnecta.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCommunications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "communications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Category = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Priority = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    Summary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    Audience = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Channel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Status = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    AttachmentLabel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Owner = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_communications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_communications_PublishedAt",
                table: "communications",
                column: "PublishedAt");

            migrationBuilder.CreateIndex(
                name: "IX_communications_Slug",
                table: "communications",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "communications");
        }
    }
}
