using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalRH.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Tone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Icon = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    TargetUrl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Audience = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "portal_user_notification_reads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PortalUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReadAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portal_user_notification_reads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_portal_user_notification_reads_notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_portal_user_notification_reads_portal_users_PortalUserId",
                        column: x => x.PortalUserId,
                        principalTable: "portal_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_IsActive",
                table: "notifications",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_PublishedAtUtc",
                table: "notifications",
                column: "PublishedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_SourceType_SourceId",
                table: "notifications",
                columns: new[] { "SourceType", "SourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_portal_user_notification_reads_NotificationId_PortalUserId",
                table: "portal_user_notification_reads",
                columns: new[] { "NotificationId", "PortalUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_portal_user_notification_reads_PortalUserId",
                table: "portal_user_notification_reads",
                column: "PortalUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "portal_user_notification_reads");

            migrationBuilder.DropTable(
                name: "notifications");
        }
    }
}
