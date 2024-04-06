using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RisingStarsData.Migrations
{
    /// <inheritdoc />
    public partial class Mentors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MentorId",
                table: "Projects",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_MentorId",
                table: "Projects",
                column: "MentorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Mentors_MentorId",
                table: "Projects",
                column: "MentorId",
                principalTable: "Mentors",
                principalColumn: "MentorId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Mentors_MentorId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_MentorId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "MentorId",
                table: "Projects");
        }
    }
}
