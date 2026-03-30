using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDomainEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Breed = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    PhotoUrl = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CatId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsRecurring = table.Column<bool>(type: "boolean", nullable: false),
                    RecurringIntervalDays = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                    table.CheckConstraint("CK_Expenses_Amount_Positive", "\"Amount\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "FeedingLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CatId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoggedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FoodBrand = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FoodType = table.Column<string>(type: "text", nullable: false),
                    AmountGrams = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedingLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FoodStocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CatId = table.Column<Guid>(type: "uuid", nullable: false),
                    FoodBrand = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FoodType = table.Column<string>(type: "text", nullable: false),
                    QuantityGrams = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    DailyUsageGrams = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    LowStockThresholdDays = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodStocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LitterLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CatId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoggedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EntryType = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LitterLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VetRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CatId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordType = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    ClinicName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    VetName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Cost = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    NextDueDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VetRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WaterLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CatId = table.Column<Guid>(type: "uuid", nullable: false),
                    CleanedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaterLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_CatId_Date",
                table: "Expenses",
                columns: new[] { "CatId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_FeedingLogs_CatId",
                table: "FeedingLogs",
                column: "CatId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodStocks_CatId",
                table: "FoodStocks",
                column: "CatId");

            migrationBuilder.CreateIndex(
                name: "IX_LitterLogs_CatId",
                table: "LitterLogs",
                column: "CatId");

            migrationBuilder.CreateIndex(
                name: "IX_VetRecords_CatId_NextDueDate",
                table: "VetRecords",
                columns: new[] { "CatId", "NextDueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_WaterLogs_CatId_CleanedAt",
                table: "WaterLogs",
                columns: new[] { "CatId", "CleanedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cats");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "FeedingLogs");

            migrationBuilder.DropTable(
                name: "FoodStocks");

            migrationBuilder.DropTable(
                name: "LitterLogs");

            migrationBuilder.DropTable(
                name: "VetRecords");

            migrationBuilder.DropTable(
                name: "WaterLogs");
        }
    }
}
