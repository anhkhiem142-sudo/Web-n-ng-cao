using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatLichKhamBenh.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentInsurance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasInsurance",
                table: "Appointments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceNumber",
                table: "Appointments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasInsurance",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "InsuranceNumber",
                table: "Appointments");
        }
    }
}
