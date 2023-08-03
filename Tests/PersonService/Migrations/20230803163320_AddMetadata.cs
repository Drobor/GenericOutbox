using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonService.Migrations
{
    /// <inheritdoc />
    public partial class AddMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "OutboxEntity",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "OutboxEntity");
        }
    }
}
