using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalLioConnecta.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTotvsRmExtendedConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "CodColigada",
                table: "totvs_rm_configurations",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<bool>(
                name: "EnableBeneficios",
                table: "totvs_rm_configurations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableCadastro",
                table: "totvs_rm_configurations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableFerias",
                table: "totvs_rm_configurations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableHolerite",
                table: "totvs_rm_configurations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnablePonto",
                table: "totvs_rm_configurations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableTeamDashboard",
                table: "totvs_rm_configurations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodColigada",
                table: "totvs_rm_configurations");

            migrationBuilder.DropColumn(
                name: "EnableBeneficios",
                table: "totvs_rm_configurations");

            migrationBuilder.DropColumn(
                name: "EnableCadastro",
                table: "totvs_rm_configurations");

            migrationBuilder.DropColumn(
                name: "EnableFerias",
                table: "totvs_rm_configurations");

            migrationBuilder.DropColumn(
                name: "EnableHolerite",
                table: "totvs_rm_configurations");

            migrationBuilder.DropColumn(
                name: "EnablePonto",
                table: "totvs_rm_configurations");

            migrationBuilder.DropColumn(
                name: "EnableTeamDashboard",
                table: "totvs_rm_configurations");
        }
    }
}
