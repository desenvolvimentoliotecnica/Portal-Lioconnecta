using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalRH.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunicationLikes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "communication_interaction_audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunicationId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_communication_interaction_audit_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_communication_interaction_audit_logs_communications_Communi~",
                        column: x => x.CommunicationId,
                        principalTable: "communications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_communication_interaction_audit_logs_portal_users_PortalUse~",
                        column: x => x.PortalUserId,
                        principalTable: "portal_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "communication_likes",
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
                    table.PrimaryKey("PK_communication_likes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_communication_likes_communications_CommunicationId",
                        column: x => x.CommunicationId,
                        principalTable: "communications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_communication_likes_portal_users_PortalUserId",
                        column: x => x.PortalUserId,
                        principalTable: "portal_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_communication_interaction_audit_logs_CommunicationId",
                table: "communication_interaction_audit_logs",
                column: "CommunicationId");

            migrationBuilder.CreateIndex(
                name: "IX_communication_interaction_audit_logs_CreatedAtUtc",
                table: "communication_interaction_audit_logs",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_communication_interaction_audit_logs_PortalUserId",
                table: "communication_interaction_audit_logs",
                column: "PortalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_communication_likes_CommunicationId_PortalUserId",
                table: "communication_likes",
                columns: new[] { "CommunicationId", "PortalUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_communication_likes_CreatedAtUtc",
                table: "communication_likes",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_communication_likes_PortalUserId",
                table: "communication_likes",
                column: "PortalUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "communication_interaction_audit_logs");

            migrationBuilder.DropTable(
                name: "communication_likes");
        }
    }
}
