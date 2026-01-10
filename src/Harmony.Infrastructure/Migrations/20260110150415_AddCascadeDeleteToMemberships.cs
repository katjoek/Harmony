using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmony.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCascadeDeleteToMemberships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_PersonGroupMemberships_Groups_GroupId",
                table: "PersonGroupMemberships",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonGroupMemberships_Persons_PersonId",
                table: "PersonGroupMemberships",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonGroupMemberships_Groups_GroupId",
                table: "PersonGroupMemberships");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonGroupMemberships_Persons_PersonId",
                table: "PersonGroupMemberships");
        }
    }
}
