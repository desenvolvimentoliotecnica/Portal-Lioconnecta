using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalLioConnecta.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationRecipientForFeedInteractions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ActorPortalUserId",
                table: "notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RecipientPortalUserId",
                table: "notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_notifications_ActorPortalUserId",
                table: "notifications",
                column: "ActorPortalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_RecipientPortalUserId",
                table: "notifications",
                column: "RecipientPortalUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_portal_users_ActorPortalUserId",
                table: "notifications",
                column: "ActorPortalUserId",
                principalTable: "portal_users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_portal_users_RecipientPortalUserId",
                table: "notifications",
                column: "RecipientPortalUserId",
                principalTable: "portal_users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_notifications_portal_users_ActorPortalUserId",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_notifications_portal_users_RecipientPortalUserId",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "IX_notifications_ActorPortalUserId",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "IX_notifications_RecipientPortalUserId",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "ActorPortalUserId",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "RecipientPortalUserId",
                table: "notifications");
        }
    }
}
