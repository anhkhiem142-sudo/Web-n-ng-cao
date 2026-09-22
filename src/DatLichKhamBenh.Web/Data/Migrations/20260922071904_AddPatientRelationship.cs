using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatLichKhamBenh.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Relationship",
                table: "Patients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Relationship",
                table: "Patients");
        }
    }
}
