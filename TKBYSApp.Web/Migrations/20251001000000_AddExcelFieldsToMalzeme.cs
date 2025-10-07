using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TKBYSApp.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddExcelFieldsToMalzeme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AmbarAdi",
                table: "Malzemeler",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BarKod",
                table: "Malzemeler",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cinsi",
                table: "Malzemeler",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EkOzellik",
                table: "Malzemeler",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FisNo",
                table: "Malzemeler",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FisSonDurum",
                table: "Malzemeler",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "KurumGirisTarihi",
                table: "Malzemeler",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MarkaAdi",
                table: "Malzemeler",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Modeli",
                table: "Malzemeler",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OlcuAdi",
                table: "Malzemeler",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeriNo",
                table: "Malzemeler",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SicilNo",
                table: "Malzemeler",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Tarih",
                table: "Malzemeler",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TcNumarasi",
                table: "Malzemeler",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerildigiYerBirim",
                table: "Malzemeler",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Malzemeler_BarKod",
                table: "Malzemeler",
                column: "BarKod",
                unique: true,
                filter: "[BarKod] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Malzemeler_BarKod",
                table: "Malzemeler");

            migrationBuilder.DropColumn(
                name: "AmbarAdi",
                table: "Malzemeler");

            migrationBuilder.DropColumn(
                name: "BarKod",
                table: "Malzemeler");

            migrationBuilder.DropColumn(
                name: "Cinsi",
                table: "Malzemeler");

            migrationBuilder.DropColumn(
                name: "EkOzellik",
                table: "Malzemeler");

            migrationBuilder.DropColumn(
                name: "FisNo",
                table: "Malzemeler");

            migrationBuilder.DropColumn(
                name: "FisSonDurum",
                table: "Malzemeler");

            migrationBuilder.DropColumn(
                name: "KurumGirisTarihi",
                table: "Malzemeler");

            migrationBuilder.DropColumn(
                name: "MarkaAdi",
                table: "Malzemeler");

            migrationBuilder.DropColumn(
                name: "Modeli",
                table: "Malzemeler");

            migrationBuilder.DropColumn(
                name: "OlcuAdi",
                table: "Malzemeler");

            migrationBuilder.DropColumn(
                name: "SeriNo",
                table: "Malzemeler");

            migrationBuilder.DropColumn(
                name: "SicilNo",
                table: "Malzemeler");

            migrationBuilder.DropColumn(
                name: "Tarih",
                table: "Malzemeler");

            migrationBuilder.DropColumn(
                name: "TcNumarasi",
                table: "Malzemeler");

            migrationBuilder.DropColumn(
                name: "VerildigiYerBirim",
                table: "Malzemeler");
        }
    }
}
