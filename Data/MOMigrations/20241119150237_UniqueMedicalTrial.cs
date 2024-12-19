using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalOffice.Data.MOMigrations
{
    /// <inheritdoc />
    public partial class UniqueMedicalTrial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_MedicalTrials_TrialName",
                table: "MedicalTrials",
                column: "TrialName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MedicalTrials_TrialName",
                table: "MedicalTrials");
        }
    }
}
