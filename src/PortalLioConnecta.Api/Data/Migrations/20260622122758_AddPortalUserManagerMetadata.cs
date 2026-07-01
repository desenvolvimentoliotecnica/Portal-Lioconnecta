using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalLioConnecta.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPortalUserManagerMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ManagerDisplayName",
                table: "portal_users",
                type: "character varying(180)",
                maxLength: 180,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagerDistinguishedName",
                table: "portal_users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ManagerDisplayName",
                table: "portal_users");

            migrationBuilder.DropColumn(
                name: "ManagerDistinguishedName",
                table: "portal_users");
        }
    }
}
