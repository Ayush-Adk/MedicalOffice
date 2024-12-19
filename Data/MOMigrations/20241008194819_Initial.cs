using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalOffice.Data.MOMigrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Conditions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConditionName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conditions", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Doctors",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MiddleName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctors", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "MedicalTrials",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrialName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalTrials", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Specialties",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SpecialtyName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialties", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OHIP = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MiddleName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DOB = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExpYrVisits = table.Column<byte>(type: "INTEGER", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    EMail = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    Coverage = table.Column<int>(type: "INTEGER", nullable: false),
                    MedicalTrialID = table.Column<int>(type: "INTEGER", nullable: true),
                    DoctorID = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Patients_Doctors_DoctorID",
                        column: x => x.DoctorID,
                        principalTable: "Doctors",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Patients_MedicalTrials_MedicalTrialID",
                        column: x => x.MedicalTrialID,
                        principalTable: "MedicalTrials",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "DoctorSpecialties",
                columns: table => new
                {
                    DoctorID = table.Column<int>(type: "INTEGER", nullable: false),
                    SpecialtyID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorSpecialties", x => new { x.DoctorID, x.SpecialtyID });
                    table.ForeignKey(
                        name: "FK_DoctorSpecialties_Doctors_DoctorID",
                        column: x => x.DoctorID,
                        principalTable: "Doctors",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoctorSpecialties_Specialties_SpecialtyID",
                        column: x => x.SpecialtyID,
                        principalTable: "Specialties",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PatientConditions",
                columns: table => new
                {
                    ConditionID = table.Column<int>(type: "INTEGER", nullable: false),
                    PatientID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientConditions", x => new { x.ConditionID, x.PatientID });
                    table.ForeignKey(
                        name: "FK_PatientConditions_Conditions_ConditionID",
                        column: x => x.ConditionID,
                        principalTable: "Conditions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientConditions_Patients_PatientID",
                        column: x => x.PatientID,
                        principalTable: "Patients",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorSpecialties_SpecialtyID",
                table: "DoctorSpecialties",
                column: "SpecialtyID");

            migrationBuilder.CreateIndex(
                name: "IX_PatientConditions_PatientID",
                table: "PatientConditions",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_DOB_LastName_FirstName",
                table: "Patients",
                columns: new[] { "DOB", "LastName", "FirstName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patients_DoctorID",
                table: "Patients",
                column: "DoctorID");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_MedicalTrialID",
                table: "Patients",
                column: "MedicalTrialID");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_OHIP",
                table: "Patients",
                column: "OHIP",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoctorSpecialties");

            migrationBuilder.DropTable(
                name: "PatientConditions");

            migrationBuilder.DropTable(
                name: "Specialties");

            migrationBuilder.DropTable(
                name: "Conditions");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DropTable(
                name: "Doctors");

            migrationBuilder.DropTable(
                name: "MedicalTrials");
        }
    }
}
