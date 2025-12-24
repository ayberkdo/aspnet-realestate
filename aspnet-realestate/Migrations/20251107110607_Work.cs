using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace aspnet_realestate.Migrations
{
    /// <inheritdoc />
    public partial class Work : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AmenitiesGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmenitiesGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentCategoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Setting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SiteTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SiteDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SiteKeywords = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FaviconUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SitePhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SiteEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SiteAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MapEmbedCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InstagramUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FacebookUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TwitterUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YoutubeUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Setting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Amenities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AmenitiesGroupId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Amenities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Amenities_AmenitiesGroups_AmenitiesGroupId",
                        column: x => x.AmenitiesGroupId,
                        principalTable: "AmenitiesGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CategoryFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategoriesId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryFields_Categories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "AmenitiesGroups",
                columns: new[] { "Id", "Created", "Description", "ImageUrl", "IsActive", "Name", "Updated" },
                values: new object[] { 1, new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, "Ev Eşyaları", new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Created", "Description", "ImageUrl", "IsActive", "Name", "ParentCategoryId", "Slug", "Updated" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Satılık veya kiralık ev kategorisi", "/images/categories/konut.jpg", true, "Konut", 0, "konut", new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Satılık ve kiralık daireler", "/images/categories/daire.jpg", true, "Daire", 1, "daire", new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Lüks villa ve müstakil ev ilanları", "/images/categories/villa.jpg", true, "Villa", 1, "villa", new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Satılık arsa ve tarla ilanları", "/images/categories/arsa.jpg", true, "Arsa", 0, "arsa", new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "CategoryFields",
                columns: new[] { "Id", "CategoriesId", "CategoryId", "Created", "FieldName", "IsActive", "Updated" },
                values: new object[,]
                {
                    { 1, null, 2, new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Oda Sayısı", true, new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, null, 2, new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bina Yaşı", true, new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, null, 3, new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Havuz Var mı?", true, new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, null, 4, new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Metrekare", true, new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Setting",
                columns: new[] { "Id", "FacebookUrl", "FaviconUrl", "InstagramUrl", "LogoUrl", "MapEmbedCode", "SiteAddress", "SiteDescription", "SiteEmail", "SiteKeywords", "SitePhoneNumber", "SiteTitle", "TwitterUrl", "Updated", "YoutubeUrl" },
                values: new object[] { 1, "https://facebook.com/ayberkemlak", "/images/favicon.ico", "https://instagram.com/ayberkemlak", "/images/logo.png", "<iframe src='https://maps.google.com/...'></iframe>", "İstanbul, Türkiye", "Modern emlak platformu — satılık ve kiralık ilanlarınızı kolayca yönetin.", "info@ayberkemlak.com", "emlak, satılık, kiralık, daire, villa, arsa, ayberk emlak", "+90 555 123 4567", "Ayberk Emlak", "https://twitter.com/ayberkemlak", new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "https://youtube.com/@ayberkemlak" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Bio", "Created", "Email", "FullName", "IsActive", "PasswordHash", "PhoneNumber", "ProfileImageUrl", "Role", "Updated", "UserName" },
                values: new object[] { 1, "Merhaba, ben Ayberk. ASP.NET Core geliştiricisiyim.", new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ayberk@gmail.com", "Ayberk Dönmez", true, "ayberk", "5551234567", "", "Admin", new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ayberk" });

            migrationBuilder.InsertData(
                table: "Amenities",
                columns: new[] { "Id", "AmenitiesGroupId", "Created", "Description", "ImageUrl", "IsActive", "Name", "Updated" },
                values: new object[] { 1, 1, new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, true, "Çamaşır Makinesi", new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.CreateIndex(
                name: "IX_Amenities_AmenitiesGroupId",
                table: "Amenities",
                column: "AmenitiesGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryFields_CategoriesId",
                table: "CategoryFields",
                column: "CategoriesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Amenities");

            migrationBuilder.DropTable(
                name: "CategoryFields");

            migrationBuilder.DropTable(
                name: "Setting");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "AmenitiesGroups");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
