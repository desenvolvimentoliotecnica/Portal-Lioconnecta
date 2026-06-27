using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalRH.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedPostMediaComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "feed_post_media_comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FeedPostMediaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PortalUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    Origin = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feed_post_media_comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_feed_post_media_comments_feed_post_media_FeedPostMediaId",
                        column: x => x.FeedPostMediaId,
                        principalTable: "feed_post_media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_feed_post_media_comments_portal_users_PortalUserId",
                        column: x => x.PortalUserId,
                        principalTable: "portal_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_feed_post_media_comments_FeedPostMediaId_CreatedAtUtc",
                table: "feed_post_media_comments",
                columns: new[] { "FeedPostMediaId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_feed_post_media_comments_PortalUserId",
                table: "feed_post_media_comments",
                column: "PortalUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "feed_post_media_comments");
        }
    }
}
