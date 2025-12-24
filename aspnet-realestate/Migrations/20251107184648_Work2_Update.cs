using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aspnet_realestate.Migrations
{
    /// <inheritdoc />
    public partial class Work2_Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_PropertyDestinations_DestinationId",
                table: "Properties");

            migrationBuilder.RenameColumn(
                name: "DestinationId",
                table: "Properties",
                newName: "PropertyDestinationId");

            migrationBuilder.RenameIndex(
                name: "IX_Properties_DestinationId",
                table: "Properties",
                newName: "IX_Properties_PropertyDestinationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_PropertyDestinations_PropertyDestinationId",
                table: "Properties",
                column: "PropertyDestinationId",
                principalTable: "PropertyDestinations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_PropertyDestinations_PropertyDestinationId",
                table: "Properties");

            migrationBuilder.RenameColumn(
                name: "PropertyDestinationId",
                table: "Properties",
                newName: "DestinationId");

            migrationBuilder.RenameIndex(
                name: "IX_Properties_PropertyDestinationId",
                table: "Properties",
                newName: "IX_Properties_DestinationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_PropertyDestinations_DestinationId",
                table: "Properties",
                column: "DestinationId",
                principalTable: "PropertyDestinations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
