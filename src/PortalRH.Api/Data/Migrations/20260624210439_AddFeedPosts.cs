using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalRH.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "feed_posts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PortalUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Origin = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feed_posts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_feed_posts_portal_users_PortalUserId",
                        column: x => x.PortalUserId,
                        principalTable: "portal_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "feed_post_audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FeedPostId = table.Column<Guid>(type: "uuid", nullable: false),
                    PortalUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ActorLogin = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ActorDisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Origin = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feed_post_audit_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_feed_post_audit_logs_feed_posts_FeedPostId",
                        column: x => x.FeedPostId,
                        principalTable: "feed_posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_feed_post_audit_logs_portal_users_PortalUserId",
                        column: x => x.PortalUserId,
                        principalTable: "portal_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_feed_post_audit_logs_CreatedAtUtc",
                table: "feed_post_audit_logs",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_feed_post_audit_logs_FeedPostId",
                table: "feed_post_audit_logs",
                column: "FeedPostId");

            migrationBuilder.CreateIndex(
                name: "IX_feed_post_audit_logs_PortalUserId",
                table: "feed_post_audit_logs",
                column: "PortalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_feed_posts_CreatedAtUtc",
                table: "feed_posts",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_feed_posts_PortalUserId",
                table: "feed_posts",
                column: "PortalUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "feed_post_audit_logs");

            migrationBuilder.DropTable(
                name: "feed_posts");
        }
    }
}
