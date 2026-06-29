using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalLioConnecta.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLdapConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ldap_configurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Server = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    UseLdaps = table.Column<bool>(type: "boolean", nullable: false),
                    UseStartTls = table.Column<bool>(type: "boolean", nullable: false),
                    IgnoreCertificateValidation = table.Column<bool>(type: "boolean", nullable: false),
                    BaseDn = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    UserSearchBase = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    NetbiosDomain = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    LoginFormat = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    BindDn = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    BindPasswordProtected = table.Column<string>(type: "text", nullable: true),
                    SearchFilter = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DisplayNameAttribute = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ldap_configurations", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ldap_configurations");
        }
    }
}
