using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntegrationTests.Migrations
{
    /// <inheritdoc />
    public partial class RemoveParent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OutboxEntity_OutboxEntity_ParentId",
                table: "OutboxEntity");

            migrationBuilder.DropIndex(
                name: "IX_OutboxEntity_ParentId",
                table: "OutboxEntity");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "OutboxEntity");

            migrationBuilder.AlterColumn<Guid>(
                name: "Lock",
                table: "OutboxEntity",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "Lock",
                table: "OutboxEntity",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "OutboxEntity",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEntity_ParentId",
                table: "OutboxEntity",
                column: "ParentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OutboxEntity_OutboxEntity_ParentId",
                table: "OutboxEntity",
                column: "ParentId",
                principalTable: "OutboxEntity",
                principalColumn: "Id");
        }
    }
}
