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
            migrationBuilder.DropForeignKey(
                name: "FK_Balances_Resources_ResourceId",
                table: "Balances");

            migrationBuilder.DropForeignKey(
                name: "FK_Balances_Units_UnitId",
                table: "Balances");

            migrationBuilder.DropIndex(
                name: "IX_Units_Name",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentItems_DocumentId",
                table: "ShipmentItems");

            migrationBuilder.DropIndex(
                name: "IX_Resources_Name",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptItems_DocumentId",
                table: "ReceiptItems");

            migrationBuilder.DropIndex(
                name: "IX_Clients_Name",
                table: "Clients");

            migrationBuilder.RenameIndex(
                name: "IX_Balances_ResourceId_UnitId",
                table: "Balances",
                newName: "IX_Balances_Resource_Unit");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Units",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<bool>(
                name: "IsArchived",
                table: "Units",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Units",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Units",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "ShipmentItems",
                type: "numeric(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "ShipmentDocuments",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "ShipmentDocuments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ShipmentDocuments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ShipmentDocuments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Resources",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<bool>(
                name: "IsArchived",
                table: "Resources",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Resources",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Resources",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "ReceiptItems",
                type: "numeric(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "ReceiptDocuments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ReceiptDocuments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ReceiptDocuments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Clients",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<bool>(
                name: "IsArchived",
                table: "Clients",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Clients",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Clients",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Clients",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "Balances",
                type: "numeric(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.CreateIndex(
                name: "IX_Units_IsArchived",
                table: "Units",
                column: "IsArchived");

            migrationBuilder.CreateIndex(
                name: "IX_Units_Name",
                table: "Units",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentItems_Document_Resource_Unit",
                table: "ShipmentItems",
                columns: new[] { "DocumentId", "ResourceId", "UnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentDocuments_Date",
                table: "ShipmentDocuments",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentDocuments_Status",
                table: "ShipmentDocuments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_IsArchived",
                table: "Resources",
                column: "IsArchived");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_Name",
                table: "Resources",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptItems_Document_Resource_Unit",
                table: "ReceiptItems",
                columns: new[] { "DocumentId", "ResourceId", "UnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptDocuments_Date",
                table: "ReceiptDocuments",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_IsArchived",
                table: "Clients",
                column: "IsArchived");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Name",
                table: "Clients",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Balances_Resources_ResourceId",
                table: "Balances",
                column: "ResourceId",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Balances_Units_UnitId",
                table: "Balances",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Balances_Resources_ResourceId",
                table: "Balances");

            migrationBuilder.DropForeignKey(
                name: "FK_Balances_Units_UnitId",
                table: "Balances");

            migrationBuilder.DropIndex(
                name: "IX_Units_IsArchived",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Units_Name",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentItems_Document_Resource_Unit",
                table: "ShipmentItems");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentDocuments_Date",
                table: "ShipmentDocuments");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentDocuments_Status",
                table: "ShipmentDocuments");

            migrationBuilder.DropIndex(
                name: "IX_Resources_IsArchived",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resources_Name",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptItems_Document_Resource_Unit",
                table: "ReceiptItems");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptDocuments_Date",
                table: "ReceiptDocuments");

            migrationBuilder.DropIndex(
                name: "IX_Clients_IsArchived",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_Name",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ShipmentDocuments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ShipmentDocuments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ReceiptDocuments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ReceiptDocuments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Clients");

            migrationBuilder.RenameIndex(
                name: "IX_Balances_Resource_Unit",
                table: "Balances",
                newName: "IX_Balances_ResourceId_UnitId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Units",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<bool>(
                name: "IsArchived",
                table: "Units",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "ShipmentItems",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,3)");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "ShipmentDocuments",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "ShipmentDocuments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Resources",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsArchived",
                table: "Resources",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "ReceiptItems",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,3)");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "ReceiptDocuments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Clients",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsArchived",
                table: "Clients",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Clients",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "Balances",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,3)");

            migrationBuilder.CreateIndex(
                name: "IX_Units_Name",
                table: "Units",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentItems_DocumentId",
                table: "ShipmentItems",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_Name",
                table: "Resources",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptItems_DocumentId",
                table: "ReceiptItems",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Name",
                table: "Clients",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Balances_Resources_ResourceId",
                table: "Balances",
                column: "ResourceId",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Balances_Units_UnitId",
                table: "Balances",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
