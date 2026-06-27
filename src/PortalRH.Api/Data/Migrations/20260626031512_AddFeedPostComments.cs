using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalRH.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedPostComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "feed_post_comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FeedPostId = table.Column<Guid>(type: "uuid", nullable: false),
                    PortalUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    Origin = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feed_post_comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_feed_post_comments_feed_posts_FeedPostId",
                        column: x => x.FeedPostId,
                        principalTable: "feed_posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_feed_post_comments_portal_users_PortalUserId",
                        column: x => x.PortalUserId,
                        principalTable: "portal_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "feed_post_comment_mentions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FeedPostCommentId = table.Column<Guid>(type: "uuid", nullable: false),
                    MentionedPortalUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feed_post_comment_mentions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_feed_post_comment_mentions_feed_post_comments_FeedPostComme~",
                        column: x => x.FeedPostCommentId,
                        principalTable: "feed_post_comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_feed_post_comment_mentions_portal_users_MentionedPortalUser~",
                        column: x => x.MentionedPortalUserId,
                        principalTable: "portal_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_feed_post_comment_mentions_FeedPostCommentId_MentionedPorta~",
                table: "feed_post_comment_mentions",
                columns: new[] { "FeedPostCommentId", "MentionedPortalUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_feed_post_comment_mentions_MentionedPortalUserId",
                table: "feed_post_comment_mentions",
                column: "MentionedPortalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_feed_post_comments_FeedPostId_CreatedAtUtc",
                table: "feed_post_comments",
                columns: new[] { "FeedPostId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_feed_post_comments_PortalUserId",
                table: "feed_post_comments",
                column: "PortalUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "feed_post_comment_mentions");

            migrationBuilder.DropTable(
                name: "feed_post_comments");
        }
    }
}
