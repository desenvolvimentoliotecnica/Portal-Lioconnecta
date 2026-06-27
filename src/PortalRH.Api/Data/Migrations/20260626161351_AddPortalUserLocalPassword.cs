using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalRH.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPortalUserLocalPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "portal_users",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "portal_users");
        }
    }
}
