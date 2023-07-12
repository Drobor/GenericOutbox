using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Test.Migrations
{
    /// <inheritdoc />
    public partial class dbGeneratedUpdate4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "LastUpdated",
                table: "OutboxEntities",
                type: "INTEGER",
                nullable: false,
                defaultValueSql: "unixepoch()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "TEXT",
                oldDefaultValueSql: "unixepoch()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "LastUpdated",
                table: "OutboxEntities",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "unixepoch()",
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldDefaultValueSql: "unixepoch()");
        }
    }
}
