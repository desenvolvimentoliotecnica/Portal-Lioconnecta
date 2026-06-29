using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalLioConnecta.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPortalUserPermissionAndAuthAuditEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_portal_user_login_events_portal_users_PortalUserId",
                table: "portal_user_login_events");

            migrationBuilder.AddColumn<int>(
                name: "FailedLoginCount",
                table: "portal_users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastFailedLoginAtUtc",
                table: "portal_users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastKnownIpAddress",
                table: "portal_users",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastOrigin",
                table: "portal_users",
                type: "character varying(240)",
                maxLength: 240,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModulePermissionsJson",
                table: "portal_users",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PortalUserId",
                table: "portal_user_login_events",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "EventType",
                table: "portal_user_login_events",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FailureReason",
                table: "portal_user_login_events",
                type: "character varying(240)",
                maxLength: 240,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "portal_user_login_events",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSuccess",
                table: "portal_user_login_events",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Origin",
                table: "portal_user_login_events",
                type: "character varying(240)",
                maxLength: 240,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "portal_user_login_events",
                type: "character varying(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_portal_user_login_events_Login",
                table: "portal_user_login_events",
                column: "Login");

            migrationBuilder.AddForeignKey(
                name: "FK_portal_user_login_events_portal_users_PortalUserId",
                table: "portal_user_login_events",
                column: "PortalUserId",
                principalTable: "portal_users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_portal_user_login_events_portal_users_PortalUserId",
                table: "portal_user_login_events");

            migrationBuilder.DropIndex(
                name: "IX_portal_user_login_events_Login",
                table: "portal_user_login_events");

            migrationBuilder.DropColumn(
                name: "FailedLoginCount",
                table: "portal_users");

            migrationBuilder.DropColumn(
                name: "LastFailedLoginAtUtc",
                table: "portal_users");

            migrationBuilder.DropColumn(
                name: "LastKnownIpAddress",
                table: "portal_users");

            migrationBuilder.DropColumn(
                name: "LastOrigin",
                table: "portal_users");

            migrationBuilder.DropColumn(
                name: "ModulePermissionsJson",
                table: "portal_users");

            migrationBuilder.DropColumn(
                name: "EventType",
                table: "portal_user_login_events");

            migrationBuilder.DropColumn(
                name: "FailureReason",
                table: "portal_user_login_events");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "portal_user_login_events");

            migrationBuilder.DropColumn(
                name: "IsSuccess",
                table: "portal_user_login_events");

            migrationBuilder.DropColumn(
                name: "Origin",
                table: "portal_user_login_events");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "portal_user_login_events");

            migrationBuilder.AlterColumn<Guid>(
                name: "PortalUserId",
                table: "portal_user_login_events",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_portal_user_login_events_portal_users_PortalUserId",
                table: "portal_user_login_events",
                column: "PortalUserId",
                principalTable: "portal_users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
