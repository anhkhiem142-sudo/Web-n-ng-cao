using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatLichKhamBenh.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveInsuranceNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InsuranceNumber",
                table: "Appointments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InsuranceNumber",
                table: "Appointments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }
    }
}
