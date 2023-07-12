using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonService.Migrations
{
    /// <inheritdoc />
    public partial class outboxTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OutboxEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HandlerLock = table.Column<Guid>(type: "TEXT", nullable: true),
                    ParentId = table.Column<int>(type: "INTEGER", nullable: true),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    Payload = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Response = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Exception = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    RetryTimeout = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    RetriesCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LastUpdated = table.Column<long>(type: "INTEGER", nullable: false, defaultValueSql: "unixepoch()"),
                    Version = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OutboxEntity_OutboxEntity_ParentId",
                        column: x => x.ParentId,
                        principalTable: "OutboxEntity",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEntity_HandlerLock",
                table: "OutboxEntity",
                column: "HandlerLock");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEntity_ParentId",
                table: "OutboxEntity",
                column: "ParentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEntity_Version",
                table: "OutboxEntity",
                column: "Version");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxEntity");
        }
    }
}
