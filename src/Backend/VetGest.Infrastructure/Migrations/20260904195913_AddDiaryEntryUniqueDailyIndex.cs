using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetGest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDiaryEntryUniqueDailyIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PregnancyDiaryEntries_PregnancyId_EntryDate",
                table: "PregnancyDiaryEntries");

            migrationBuilder.CreateIndex(
                name: "IX_PregnancyDiaryEntries_PregnancyId_EntryDate",
                table: "PregnancyDiaryEntries",
                columns: new[] { "PregnancyId", "EntryDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PregnancyDiaryEntries_PregnancyId_EntryDate",
                table: "PregnancyDiaryEntries");

            migrationBuilder.CreateIndex(
                name: "IX_PregnancyDiaryEntries_PregnancyId_EntryDate",
                table: "PregnancyDiaryEntries",
                columns: new[] { "PregnancyId", "EntryDate" });
        }
    }
}
