using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntegrationTests.Migrations
{
    /// <inheritdoc />
    public partial class initialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OutboxDataEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ScopeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Data = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxDataEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Lock = table.Column<Guid>(type: "TEXT", nullable: true),
                    HandlerLock = table.Column<Guid>(type: "TEXT", nullable: true),
                    ParentId = table.Column<int>(type: "INTEGER", nullable: true),
                    ScopeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    Payload = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    RetryTimeoutUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RetriesCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LastUpdatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
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
                name: "IX_OutboxDataEntity_ScopeId",
                table: "OutboxDataEntity",
                column: "ScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEntity_HandlerLock",
                table: "OutboxEntity",
                column: "HandlerLock");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEntity_Lock",
                table: "OutboxEntity",
                column: "Lock");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEntity_ParentId",
                table: "OutboxEntity",
                column: "ParentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEntity_ScopeId",
                table: "OutboxEntity",
                column: "ScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEntity_Version",
                table: "OutboxEntity",
                column: "Version");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxDataEntity");

            migrationBuilder.DropTable(
                name: "OutboxEntity");
        }
    }
}
