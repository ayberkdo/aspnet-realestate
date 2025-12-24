using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aspnet_realestate.Migrations
{
    /// <inheritdoc />
    public partial class Add_Deleted_Column_To_Messages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Deleted",
                table: "Messages",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Messages");
        }
    }
}
