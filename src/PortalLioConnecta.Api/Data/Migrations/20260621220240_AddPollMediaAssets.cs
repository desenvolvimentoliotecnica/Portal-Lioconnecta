using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalLioConnecta.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPollMediaAssets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentLabel",
                table: "polls",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "polls",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "polls",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentLabel",
                table: "polls");

            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "polls");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "polls");
        }
    }
}
