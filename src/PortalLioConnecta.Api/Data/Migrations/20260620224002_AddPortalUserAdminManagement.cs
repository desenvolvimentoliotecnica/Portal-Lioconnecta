using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalLioConnecta.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPortalUserAdminManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "portal_users",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "portal_user_admin_audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PortalUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActionType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ActorUsername = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ActorDisplayName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    ActorRole = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    PreviousValue = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    NewValue = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portal_user_admin_audit_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_portal_user_admin_audit_logs_admin_users_AdminUserId",
                        column: x => x.AdminUserId,
                        principalTable: "admin_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_portal_user_admin_audit_logs_portal_users_PortalUserId",
                        column: x => x.PortalUserId,
                        principalTable: "portal_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "portal_user_login_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PortalUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Login = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    DisplayNameSnapshot = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    EmailSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DepartmentSnapshot = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    AuthenticationProvider = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    LoggedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portal_user_login_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_portal_user_login_events_portal_users_PortalUserId",
                        column: x => x.PortalUserId,
                        principalTable: "portal_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_portal_user_admin_audit_logs_AdminUserId",
                table: "portal_user_admin_audit_logs",
                column: "AdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_portal_user_admin_audit_logs_CreatedAtUtc",
                table: "portal_user_admin_audit_logs",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_portal_user_admin_audit_logs_PortalUserId",
                table: "portal_user_admin_audit_logs",
                column: "PortalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_portal_user_login_events_LoggedAtUtc",
                table: "portal_user_login_events",
                column: "LoggedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_portal_user_login_events_PortalUserId",
                table: "portal_user_login_events",
                column: "PortalUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "portal_user_admin_audit_logs");

            migrationBuilder.DropTable(
                name: "portal_user_login_events");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "portal_users");
        }
    }
}
