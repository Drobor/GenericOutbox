using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Test.Migrations
{
    /// <inheritdoc />
    public partial class CreatedUtcColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Exception",
                table: "OutboxEntities");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "OutboxEntities");

            migrationBuilder.DropColumn(
                name: "Response",
                table: "OutboxEntities");

            migrationBuilder.RenameColumn(
                name: "RetryTimeout",
                table: "OutboxEntities",
                newName: "RetryTimeoutUtc");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "OutboxEntities",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedUtc",
                table: "OutboxEntities",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "Lock",
                table: "OutboxEntities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ScopeId",
                table: "OutboxEntities",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEntities_Lock",
                table: "OutboxEntities",
                column: "Lock");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEntities_ScopeId",
                table: "OutboxEntities",
                column: "ScopeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxEntities_Lock",
                table: "OutboxEntities");

            migrationBuilder.DropIndex(
                name: "IX_OutboxEntities_ScopeId",
                table: "OutboxEntities");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "OutboxEntities");

            migrationBuilder.DropColumn(
                name: "LastUpdatedUtc",
                table: "OutboxEntities");

            migrationBuilder.DropColumn(
                name: "Lock",
                table: "OutboxEntities");

            migrationBuilder.DropColumn(
                name: "ScopeId",
                table: "OutboxEntities");

            migrationBuilder.RenameColumn(
                name: "RetryTimeoutUtc",
                table: "OutboxEntities",
                newName: "RetryTimeout");

            migrationBuilder.AddColumn<byte[]>(
                name: "Exception",
                table: "OutboxEntities",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastUpdated",
                table: "OutboxEntities",
                type: "INTEGER",
                nullable: false,
                defaultValueSql: "unixepoch()");

            migrationBuilder.AddColumn<byte[]>(
                name: "Response",
                table: "OutboxEntities",
                type: "BLOB",
                nullable: true);
        }
    }
}
