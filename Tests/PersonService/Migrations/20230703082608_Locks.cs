using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonService.Migrations
{
    /// <inheritdoc />
    public partial class Locks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Exception",
                table: "OutboxEntity");

            migrationBuilder.DropColumn(
                name: "Response",
                table: "OutboxEntity");

            migrationBuilder.AddColumn<Guid>(
                name: "Lock",
                table: "OutboxEntity",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScopeId",
                table: "OutboxEntity",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "OutboxDataEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ScopeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Data = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxDataEntity", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEntity_Lock",
                table: "OutboxEntity",
                column: "Lock");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEntity_ScopeId",
                table: "OutboxEntity",
                column: "ScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxDataEntity_ScopeId",
                table: "OutboxDataEntity",
                column: "ScopeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxDataEntity");

            migrationBuilder.DropIndex(
                name: "IX_OutboxEntity_Lock",
                table: "OutboxEntity");

            migrationBuilder.DropIndex(
                name: "IX_OutboxEntity_ScopeId",
                table: "OutboxEntity");

            migrationBuilder.DropColumn(
                name: "Lock",
                table: "OutboxEntity");

            migrationBuilder.DropColumn(
                name: "ScopeId",
                table: "OutboxEntity");

            migrationBuilder.AddColumn<byte[]>(
                name: "Exception",
                table: "OutboxEntity",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Response",
                table: "OutboxEntity",
                type: "BLOB",
                nullable: true);
        }
    }
}
