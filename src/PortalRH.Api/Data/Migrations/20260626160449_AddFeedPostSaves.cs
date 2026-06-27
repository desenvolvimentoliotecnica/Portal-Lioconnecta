using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalRH.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedPostSaves : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "communication_saves",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PortalUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Origin = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_communication_saves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_communication_saves_communications_CommunicationId",
                        column: x => x.CommunicationId,
                        principalTable: "communications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_communication_saves_portal_users_PortalUserId",
                        column: x => x.PortalUserId,
                        principalTable: "portal_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "feed_post_saves",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FeedPostId = table.Column<Guid>(type: "uuid", nullable: false),
                    PortalUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Origin = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feed_post_saves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_feed_post_saves_feed_posts_FeedPostId",
                        column: x => x.FeedPostId,
                        principalTable: "feed_posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_feed_post_saves_portal_users_PortalUserId",
                        column: x => x.PortalUserId,
                        principalTable: "portal_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_communication_saves_CommunicationId_PortalUserId",
                table: "communication_saves",
                columns: new[] { "CommunicationId", "PortalUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_communication_saves_CreatedAtUtc",
                table: "communication_saves",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_communication_saves_PortalUserId",
                table: "communication_saves",
                column: "PortalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_feed_post_saves_CreatedAtUtc",
                table: "feed_post_saves",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_feed_post_saves_FeedPostId_PortalUserId",
                table: "feed_post_saves",
                columns: new[] { "FeedPostId", "PortalUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_feed_post_saves_PortalUserId",
                table: "feed_post_saves",
                column: "PortalUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "communication_saves");

            migrationBuilder.DropTable(
                name: "feed_post_saves");
        }
    }
}
