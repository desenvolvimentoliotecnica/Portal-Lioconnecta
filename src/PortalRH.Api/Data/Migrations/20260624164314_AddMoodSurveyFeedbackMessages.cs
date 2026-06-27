using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalRH.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMoodSurveyFeedbackMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FeedbackMessageId",
                table: "mood_survey_votes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "mood_survey_feedback_messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OptionKey = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Message = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mood_survey_feedback_messages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_mood_survey_votes_FeedbackMessageId",
                table: "mood_survey_votes",
                column: "FeedbackMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_mood_survey_feedback_messages_OptionKey",
                table: "mood_survey_feedback_messages",
                column: "OptionKey");

            migrationBuilder.CreateIndex(
                name: "IX_mood_survey_feedback_messages_OptionKey_IsActive",
                table: "mood_survey_feedback_messages",
                columns: new[] { "OptionKey", "IsActive" });

            migrationBuilder.AddForeignKey(
                name: "FK_mood_survey_votes_mood_survey_feedback_messages_FeedbackMes~",
                table: "mood_survey_votes",
                column: "FeedbackMessageId",
                principalTable: "mood_survey_feedback_messages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mood_survey_votes_mood_survey_feedback_messages_FeedbackMes~",
                table: "mood_survey_votes");

            migrationBuilder.DropTable(
                name: "mood_survey_feedback_messages");

            migrationBuilder.DropIndex(
                name: "IX_mood_survey_votes_FeedbackMessageId",
                table: "mood_survey_votes");

            migrationBuilder.DropColumn(
                name: "FeedbackMessageId",
                table: "mood_survey_votes");
        }
    }
}
