using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalRH.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMoodSurveyModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mood_survey_audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PortalUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OptionKey = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SurveyDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ActorLogin = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ActorDisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Origin = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mood_survey_audit_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_mood_survey_audit_logs_portal_users_PortalUserId",
                        column: x => x.PortalUserId,
                        principalTable: "portal_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mood_survey_votes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PortalUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OptionKey = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SurveyDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Origin = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mood_survey_votes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_mood_survey_votes_portal_users_PortalUserId",
                        column: x => x.PortalUserId,
                        principalTable: "portal_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_mood_survey_audit_logs_CreatedAtUtc",
                table: "mood_survey_audit_logs",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_mood_survey_audit_logs_PortalUserId",
                table: "mood_survey_audit_logs",
                column: "PortalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_mood_survey_audit_logs_SurveyDate",
                table: "mood_survey_audit_logs",
                column: "SurveyDate");

            migrationBuilder.CreateIndex(
                name: "IX_mood_survey_votes_PortalUserId_SurveyDate",
                table: "mood_survey_votes",
                columns: new[] { "PortalUserId", "SurveyDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mood_survey_votes_SurveyDate",
                table: "mood_survey_votes",
                column: "SurveyDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mood_survey_audit_logs");

            migrationBuilder.DropTable(
                name: "mood_survey_votes");
        }
    }
}
