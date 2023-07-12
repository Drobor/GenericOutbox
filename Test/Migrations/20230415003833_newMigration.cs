using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Test.Migrations
{
    /// <inheritdoc />
    public partial class newMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OutboxEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    HandlerLock = table.Column<Guid>(type: "TEXT", nullable: true),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    Payload = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Response = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Exception = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    RetryTimeout = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    RetriesCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LastUpdated = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Version = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OutboxEntities_OutboxEntities_ParentId",
                        column: x => x.ParentId,
                        principalTable: "OutboxEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEntities_HandlerLock",
                table: "OutboxEntities",
                column: "HandlerLock");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEntities_ParentId",
                table: "OutboxEntities",
                column: "ParentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEntities_Version",
                table: "OutboxEntities",
                column: "Version");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxEntities");
        }
    }
}
