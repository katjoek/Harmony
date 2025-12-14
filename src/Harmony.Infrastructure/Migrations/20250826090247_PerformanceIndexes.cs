using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmony.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Persons_EmailAddress",
                table: "Persons",
                column: "EmailAddress");

            migrationBuilder.CreateIndex(
                name: "IX_PersonGroupMemberships_GroupId",
                table: "PersonGroupMemberships",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonGroupMemberships_PersonId",
                table: "PersonGroupMemberships",
                column: "PersonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Persons_EmailAddress",
                table: "Persons");

            migrationBuilder.DropIndex(
                name: "IX_PersonGroupMemberships_GroupId",
                table: "PersonGroupMemberships");

            migrationBuilder.DropIndex(
                name: "IX_PersonGroupMemberships_PersonId",
                table: "PersonGroupMemberships");
        }
    }
}
