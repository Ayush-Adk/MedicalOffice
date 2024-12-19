using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalOffice.Data.MOMigrations
{
    /// <inheritdoc />
    public partial class UniqueAppointmentReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AppointmentReasons_ReasonName",
                table: "AppointmentReasons",
                column: "ReasonName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppointmentReasons_ReasonName",
                table: "AppointmentReasons");
        }
    }
}
