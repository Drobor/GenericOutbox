using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Test.Migrations
{
    /// <inheritdoc />
    public partial class nullability2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OutboxEntities_OutboxEntities_ParentId",
                table: "OutboxEntities");

            migrationBuilder.AlterColumn<Guid>(
                name: "ParentId",
                table: "OutboxEntities",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_OutboxEntities_OutboxEntities_ParentId",
                table: "OutboxEntities",
                column: "ParentId",
                principalTable: "OutboxEntities",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OutboxEntities_OutboxEntities_ParentId",
                table: "OutboxEntities");

            migrationBuilder.AlterColumn<Guid>(
                name: "ParentId",
                table: "OutboxEntities",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OutboxEntities_OutboxEntities_ParentId",
                table: "OutboxEntities",
                column: "ParentId",
                principalTable: "OutboxEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
