using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VetGest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Species = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Normal"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Conditions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Species = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Breed = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CurrentWeight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    OwnerId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Phases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Species = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    StartDayGestation = table.Column<int>(type: "int", nullable: false),
                    EndDayGestation = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FetalDevelopmentContent = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    MaternalChangesContent = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pregnancies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OvulationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstimatedDueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrentPhaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    OwnerId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pregnancies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pregnancies_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pregnancies_Phases_CurrentPhaseId",
                        column: x => x.CurrentPhaseId,
                        principalTable: "Phases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PregnancyDiaryEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PregnancyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Appetite = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Behavior = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Temperature = table.Column<decimal>(type: "decimal(4,1)", precision: 4, scale: 1, nullable: true),
                    SymptomsList = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PhotoUrls = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    OwnerId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PregnancyDiaryEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PregnancyDiaryEntries_Pregnancies_PregnancyId",
                        column: x => x.PregnancyId,
                        principalTable: "Pregnancies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VetConnections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PregnancyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VetUserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    TutorUserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    InvitationCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InvitationExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    OwnerId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VetConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VetConnections_Pregnancies_PregnancyId",
                        column: x => x.PregnancyId,
                        principalTable: "Pregnancies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AlertRules",
                columns: new[] { "Id", "Conditions", "Description", "IsActive", "Name", "Severity", "Species" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333311"), "{\"temperature_min\": 39.5, \"temperature_max\": 40.5}", "Mother's temperature is elevated, which may indicate infection or stress.", true, "High Temperature", "Attention", "Dog" },
                    { new Guid("33333333-3333-3333-3333-333333333312"), "{\"temperature_min\": 40.5}", "Mother's temperature is dangerously high. Seek veterinary care immediately.", true, "Very High Temperature", "Emergency", "Dog" },
                    { new Guid("33333333-3333-3333-3333-333333333313"), "{\"temperature_max\": 37.5}", "Mother's temperature is below normal, which may indicate illness or stress.", true, "Low Temperature", "Attention", "Dog" },
                    { new Guid("33333333-3333-3333-3333-333333333314"), "{\"appetite\": \"decreased\"}", "Mother shows reduced interest in food, which may be normal near birth but needs monitoring.", true, "Decreased Appetite", "Attention", "Dog" },
                    { new Guid("33333333-3333-3333-3333-333333333315"), "{\"appetite\": \"none\"}", "Mother refuses all food. Consult a veterinarian if this persists.", true, "No Food Intake", "Emergency", "Dog" },
                    { new Guid("33333333-3333-3333-3333-333333333316"), "discharge,bleeding", "Abnormal discharge may indicate infection or complications. Seek immediate veterinary care.", true, "Abnormal Discharge", "Emergency", "Dog" },
                    { new Guid("33333333-3333-3333-3333-333333333317"), "lethargy,unresponsive", "Mother is unusually inactive or unresponsive. This requires immediate veterinary evaluation.", true, "Severe Lethargy", "Emergency", "Dog" },
                    { new Guid("33333333-3333-3333-3333-333333333321"), "{\"temperature_min\": 39.0, \"temperature_max\": 39.8}", "Mother's temperature is elevated, which may indicate infection or stress.", true, "High Temperature", "Attention", "Cat" },
                    { new Guid("33333333-3333-3333-3333-333333333322"), "{\"temperature_min\": 39.8}", "Mother's temperature is dangerously high. Seek veterinary care immediately.", true, "Very High Temperature", "Emergency", "Cat" },
                    { new Guid("33333333-3333-3333-3333-333333333323"), "{\"temperature_max\": 37.5}", "Mother's temperature is below normal, which may indicate illness or stress.", true, "Low Temperature", "Attention", "Cat" },
                    { new Guid("33333333-3333-3333-3333-333333333324"), "{\"appetite\": \"decreased\"}", "Mother shows reduced interest in food, which may be normal near birth but needs monitoring.", true, "Decreased Appetite", "Attention", "Cat" },
                    { new Guid("33333333-3333-3333-3333-333333333325"), "discharge,bleeding", "Abnormal discharge may indicate infection or complications. Seek immediate veterinary care.", true, "Abnormal Discharge", "Emergency", "Cat" },
                    { new Guid("33333333-3333-3333-3333-333333333326"), "lethargy,unresponsive", "Mother is unusually inactive or unresponsive. This requires immediate veterinary evaluation.", true, "Severe Lethargy", "Emergency", "Cat" }
                });

            migrationBuilder.InsertData(
                table: "Phases",
                columns: new[] { "Id", "Description", "EndDayGestation", "FetalDevelopmentContent", "MaternalChangesContent", "Name", "Species", "StartDayGestation" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Fertilization and early embryo development", 14, "Embryos are microscopic. Cell division begins and implantation occurs.", "Mother shows minimal physical changes. Appetite and behavior may remain normal.", "Initial", "Dog", 0 },
                    { new Guid("11111111-1111-1111-1111-111111111112"), "Organogenesis and organ formation", 29, "Organs and body systems begin forming. Fetuses grow rapidly.", "Mother may experience morning sickness, increased appetite, or mood changes.", "Intermediate", "Dog", 15 },
                    { new Guid("11111111-1111-1111-1111-111111111113"), "Bone mineralization and maternal demand increase", 45, "Fetuses develop bones, teeth, and hair. Significant size increase.", "Mother's abdomen visibly enlarges. Appetite and water intake increase significantly.", "Growth", "Dog", 30 },
                    { new Guid("11111111-1111-1111-1111-111111111114"), "Final maturation and pre-birth preparation", 62, "Fetuses are now viable. Lungs mature and position for birth.", "Mother seeks nesting materials, displays restlessness, and may refuse food near due date.", "Final", "Dog", 46 },
                    { new Guid("11111111-1111-1111-1111-111111111115"), "Labor and delivery", 70, "Fetuses are fully developed and ready for birth.", "Mother shows clear labor signs: contractions, nesting behavior, and discharge.", "Birth", "Dog", 63 },
                    { new Guid("22222222-2222-2222-2222-222222222211"), "Fertilization and early embryo development", 14, "Embryos are microscopic. Implantation occurs in the uterine wall.", "Mother may show subtle behavioral changes or temporarily hide.", "Initial", "Cat", 0 },
                    { new Guid("22222222-2222-2222-2222-222222222212"), "Organogenesis and major organ formation", 30, "Organs develop rapidly. Embryos become recognizable kittens.", "Mother exhibits nesting behavior and may seek more attention.", "Intermediate", "Cat", 15 },
                    { new Guid("22222222-2222-2222-2222-222222222213"), "Fetal growth and bone formation", 48, "Rapid growth phase. Kittens develop their characteristic features.", "Mother's sides widen. May display increased vocalization and restlessness.", "Growth", "Cat", 31 },
                    { new Guid("22222222-2222-2222-2222-222222222214"), "Final maturation and birth preparation", 64, "Kittens fully formed and viable. Fine-tuning of body systems.", "Mother seeks secure nesting spaces and may stop eating days before labor.", "Final", "Cat", 49 },
                    { new Guid("22222222-2222-2222-2222-222222222215"), "Labor and delivery", 72, "Kittens are fully developed and ready to be born.", "Clear labor signs: panting, purring, tail twitching, and abdominal straining.", "Birth", "Cat", 65 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertRules_IsActive",
                table: "AlertRules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AlertRules_Species_Name",
                table: "AlertRules",
                columns: new[] { "Species", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlertRules_Species_Severity",
                table: "AlertRules",
                columns: new[] { "Species", "Severity" });

            migrationBuilder.CreateIndex(
                name: "IX_Pets_Name",
                table: "Pets",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Pets_OwnerId",
                table: "Pets",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Pets_OwnerId_IsActive",
                table: "Pets",
                columns: new[] { "OwnerId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Phases_Name",
                table: "Phases",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Phases_Species_StartDayGestation_EndDayGestation",
                table: "Phases",
                columns: new[] { "Species", "StartDayGestation", "EndDayGestation" });

            migrationBuilder.CreateIndex(
                name: "IX_Pregnancies_CurrentPhaseId",
                table: "Pregnancies",
                column: "CurrentPhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Pregnancies_EstimatedDueDate",
                table: "Pregnancies",
                column: "EstimatedDueDate");

            migrationBuilder.CreateIndex(
                name: "IX_Pregnancies_OwnerId",
                table: "Pregnancies",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Pregnancies_PetId",
                table: "Pregnancies",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_Pregnancies_PetId_Status",
                table: "Pregnancies",
                columns: new[] { "PetId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Pregnancies_Status",
                table: "Pregnancies",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PregnancyDiaryEntries_EntryDate",
                table: "PregnancyDiaryEntries",
                column: "EntryDate");

            migrationBuilder.CreateIndex(
                name: "IX_PregnancyDiaryEntries_OwnerId",
                table: "PregnancyDiaryEntries",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PregnancyDiaryEntries_PregnancyId",
                table: "PregnancyDiaryEntries",
                column: "PregnancyId");

            migrationBuilder.CreateIndex(
                name: "IX_PregnancyDiaryEntries_PregnancyId_EntryDate",
                table: "PregnancyDiaryEntries",
                columns: new[] { "PregnancyId", "EntryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PregnancyDiaryEntries_Temperature",
                table: "PregnancyDiaryEntries",
                column: "Temperature");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                table: "RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserClaims_UserId",
                table: "UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_UserId",
                table: "UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VetConnections_InvitationCode",
                table: "VetConnections",
                column: "InvitationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VetConnections_OwnerId",
                table: "VetConnections",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_VetConnections_PregnancyId",
                table: "VetConnections",
                column: "PregnancyId");

            migrationBuilder.CreateIndex(
                name: "IX_VetConnections_PregnancyId_Status",
                table: "VetConnections",
                columns: new[] { "PregnancyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_VetConnections_Status",
                table: "VetConnections",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_VetConnections_TutorUserId",
                table: "VetConnections",
                column: "TutorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VetConnections_VetUserId",
                table: "VetConnections",
                column: "VetUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VetConnections_VetUserId_Status",
                table: "VetConnections",
                columns: new[] { "VetUserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertRules");

            migrationBuilder.DropTable(
                name: "PregnancyDiaryEntries");

            migrationBuilder.DropTable(
                name: "RoleClaims");

            migrationBuilder.DropTable(
                name: "UserClaims");

            migrationBuilder.DropTable(
                name: "UserLogins");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserTokens");

            migrationBuilder.DropTable(
                name: "VetConnections");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Pregnancies");

            migrationBuilder.DropTable(
                name: "Pets");

            migrationBuilder.DropTable(
                name: "Phases");
        }
    }
}
