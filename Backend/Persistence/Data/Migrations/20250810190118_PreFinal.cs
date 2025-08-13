using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class PreFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ResourceId1",
                table: "ShipmentItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitId1",
                table: "ShipmentItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ShipmentDocuments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Resources",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Resources",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ResourceId1",
                table: "ReceiptItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitId1",
                table: "ReceiptItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ReceiptDocuments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ResourceId1",
                table: "Balances",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitId1",
                table: "Balances",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentItems_ResourceId1",
                table: "ShipmentItems",
                column: "ResourceId1");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentItems_UnitId1",
                table: "ShipmentItems",
                column: "UnitId1");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptItems_ResourceId1",
                table: "ReceiptItems",
                column: "ResourceId1");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptItems_UnitId1",
                table: "ReceiptItems",
                column: "UnitId1");

            migrationBuilder.CreateIndex(
                name: "IX_Balances_ResourceId1",
                table: "Balances",
                column: "ResourceId1");

            migrationBuilder.CreateIndex(
                name: "IX_Balances_UnitId1",
                table: "Balances",
                column: "UnitId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Balances_Resources_ResourceId1",
                table: "Balances",
                column: "ResourceId1",
                principalTable: "Resources",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Balances_Units_UnitId1",
                table: "Balances",
                column: "UnitId1",
                principalTable: "Units",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptItems_Resources_ResourceId1",
                table: "ReceiptItems",
                column: "ResourceId1",
                principalTable: "Resources",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptItems_Units_UnitId1",
                table: "ReceiptItems",
                column: "UnitId1",
                principalTable: "Units",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentItems_Resources_ResourceId1",
                table: "ShipmentItems",
                column: "ResourceId1",
                principalTable: "Resources",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentItems_Units_UnitId1",
                table: "ShipmentItems",
                column: "UnitId1",
                principalTable: "Units",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Balances_Resources_ResourceId1",
                table: "Balances");

            migrationBuilder.DropForeignKey(
                name: "FK_Balances_Units_UnitId1",
                table: "Balances");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptItems_Resources_ResourceId1",
                table: "ReceiptItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptItems_Units_UnitId1",
                table: "ReceiptItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentItems_Resources_ResourceId1",
                table: "ShipmentItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentItems_Units_UnitId1",
                table: "ShipmentItems");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentItems_ResourceId1",
                table: "ShipmentItems");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentItems_UnitId1",
                table: "ShipmentItems");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptItems_ResourceId1",
                table: "ReceiptItems");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptItems_UnitId1",
                table: "ReceiptItems");

            migrationBuilder.DropIndex(
                name: "IX_Balances_ResourceId1",
                table: "Balances");

            migrationBuilder.DropIndex(
                name: "IX_Balances_UnitId1",
                table: "Balances");

            migrationBuilder.DropColumn(
                name: "ResourceId1",
                table: "ShipmentItems");

            migrationBuilder.DropColumn(
                name: "UnitId1",
                table: "ShipmentItems");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ShipmentDocuments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "ResourceId1",
                table: "ReceiptItems");

            migrationBuilder.DropColumn(
                name: "UnitId1",
                table: "ReceiptItems");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ReceiptDocuments");

            migrationBuilder.DropColumn(
                name: "ResourceId1",
                table: "Balances");

            migrationBuilder.DropColumn(
                name: "UnitId1",
                table: "Balances");
        }
    }
}
