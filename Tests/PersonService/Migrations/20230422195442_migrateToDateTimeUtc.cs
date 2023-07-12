using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonService.Migrations
{
    /// <inheritdoc />
    public partial class migrateToDateTimeUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "OutboxEntity");

            migrationBuilder.RenameColumn(
                name: "RetryTimeout",
                table: "OutboxEntity",
                newName: "RetryTimeoutUtc");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedUtc",
                table: "OutboxEntity",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdatedUtc",
                table: "OutboxEntity");

            migrationBuilder.RenameColumn(
                name: "RetryTimeoutUtc",
                table: "OutboxEntity",
                newName: "RetryTimeout");

            migrationBuilder.AddColumn<long>(
                name: "LastUpdated",
                table: "OutboxEntity",
                type: "INTEGER",
                nullable: false,
                defaultValueSql: "unixepoch()");
        }
    }
}
