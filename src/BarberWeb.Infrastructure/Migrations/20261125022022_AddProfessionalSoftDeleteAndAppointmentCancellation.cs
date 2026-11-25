using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberWeb.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProfessionalSoftDeleteAndAppointmentCancellation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpeningHours_Professionals_ProfessionalId",
                table: "OpeningHours");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfessionalOfferings_Professionals_ProfessionalId",
                table: "ProfessionalOfferings");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Professionals",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CanceledAt",
                table: "Appointments",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CancellationReason",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OpeningHours_Professionals_ProfessionalId",
                table: "OpeningHours",
                column: "ProfessionalId",
                principalTable: "Professionals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfessionalOfferings_Professionals_ProfessionalId",
                table: "ProfessionalOfferings",
                column: "ProfessionalId",
                principalTable: "Professionals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpeningHours_Professionals_ProfessionalId",
                table: "OpeningHours");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfessionalOfferings_Professionals_ProfessionalId",
                table: "ProfessionalOfferings");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Professionals");

            migrationBuilder.DropColumn(
                name: "CanceledAt",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "Appointments");

            migrationBuilder.AddForeignKey(
                name: "FK_OpeningHours_Professionals_ProfessionalId",
                table: "OpeningHours",
                column: "ProfessionalId",
                principalTable: "Professionals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfessionalOfferings_Professionals_ProfessionalId",
                table: "ProfessionalOfferings",
                column: "ProfessionalId",
                principalTable: "Professionals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
