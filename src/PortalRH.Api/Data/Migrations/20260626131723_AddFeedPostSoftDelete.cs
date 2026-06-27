using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalRH.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedPostSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "feed_posts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_feed_posts_DeletedAtUtc",
                table: "feed_posts",
                column: "DeletedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_feed_posts_DeletedAtUtc",
                table: "feed_posts");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "feed_posts");
        }
    }
}
