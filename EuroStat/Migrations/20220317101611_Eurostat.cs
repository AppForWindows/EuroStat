using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EuroStat.Migrations
{
    public partial class Eurostat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiBaseURIes",
                columns: table => new
                {
                    ID = table.Column<string>(nullable: false),
                    DisplayName = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    api_base_uri = table.Column<string>(nullable: true),
                    agencyID = table.Column<string>(nullable: true),
                    catalogue = table.Column<string>(nullable: true),
                    dbLoad = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiBaseURIes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    ID = table.Column<string>(nullable: false),
                    ApiBaseID = table.Column<string>(nullable: true),
                    CategorySchemeID = table.Column<string>(nullable: true),
                    ParentID = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Categories_ApiBaseURIes_ApiBaseID",
                        column: x => x.ApiBaseID,
                        principalTable: "ApiBaseURIes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Categorisations",
                columns: table => new
                {
                    ID = table.Column<string>(nullable: false),
                    ApiBaseID = table.Column<string>(nullable: true),
                    SourceID = table.Column<string>(nullable: true),
                    TargetID = table.Column<string>(nullable: true),
                    TargetParentID = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorisations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Categorisations_ApiBaseURIes_ApiBaseID",
                        column: x => x.ApiBaseID,
                        principalTable: "ApiBaseURIes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CategorySchemes",
                columns: table => new
                {
                    ID = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ApiBaseID = table.Column<string>(nullable: true),
                    IconColor = table.Column<byte[]>(nullable: true),
                    IconGray = table.Column<byte[]>(nullable: true),
                    IconHover = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategorySchemes", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CategorySchemes_ApiBaseURIes_ApiBaseID",
                        column: x => x.ApiBaseID,
                        principalTable: "ApiBaseURIes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Dataflows",
                columns: table => new
                {
                    ID = table.Column<string>(nullable: false),
                    ApiBaseID = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    HTML = table.Column<string>(nullable: true),
                    SDMX = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dataflows", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Dataflows_ApiBaseURIes_ApiBaseID",
                        column: x => x.ApiBaseID,
                        principalTable: "ApiBaseURIes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ApiBaseURIes",
                columns: new[] { "ID", "Description", "DisplayName", "agencyID", "api_base_uri", "catalogue", "dbLoad" },
                values: new object[] { "Eurostat", "https://ec.europa.eu/eurostat/online-help/public/en/API_01_Introduction_en/#APIBASE_URI", "Eurostat", "ESTAT", "https://ec.europa.eu/eurostat/api/dissemination", "https://ec.europa.eu/eurostat/api/dissemination/sdmx/2.1/sdmx-rest.wadl", null });

            migrationBuilder.InsertData(
                table: "ApiBaseURIes",
                columns: new[] { "ID", "Description", "DisplayName", "agencyID", "api_base_uri", "catalogue", "dbLoad" },
                values: new object[] { "DG_COMP", "https://ec.europa.eu/eurostat/online-help/public/en/API_01_Introduction_en/#APIBASE_URI", "DG COMP", "COMP", "https://webgate.ec.europa.eu/comp/redisstat/api/dissemination", "https://webgate.ec.europa.eu/comp/redisstat/api/dissemination/sdmx/2.1/sdmx-rest.wadl", null });

            migrationBuilder.InsertData(
                table: "ApiBaseURIes",
                columns: new[] { "ID", "Description", "DisplayName", "agencyID", "api_base_uri", "catalogue", "dbLoad" },
                values: new object[] { "DG_EMPL", "https://ec.europa.eu/eurostat/online-help/public/en/API_01_Introduction_en/#APIBASE_URI", "DG EMPL", "EMPL", "https://webgate.ec.europa.eu/empl/redisstat/api/dissemination", "https://webgate.ec.europa.eu/empl/redisstat/api/dissemination/sdmx/2.1/sdmx-rest.wadl", null });

            migrationBuilder.InsertData(
                table: "ApiBaseURIes",
                columns: new[] { "ID", "Description", "DisplayName", "agencyID", "api_base_uri", "catalogue", "dbLoad" },
                values: new object[] { "DG_GROW", "https://ec.europa.eu/eurostat/online-help/public/en/API_01_Introduction_en/#APIBASE_URI", "DG GROW", "GROW", "https://webgate.ec.europa.eu/grow/redisstat/api/dissemination", "https://webgate.ec.europa.eu/grow/redisstat/api/dissemination/sdmx/2.1/sdmx-rest.wadl", null });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ApiBaseID",
                table: "Categories",
                column: "ApiBaseID");

            migrationBuilder.CreateIndex(
                name: "IX_Categorisations_ApiBaseID",
                table: "Categorisations",
                column: "ApiBaseID");

            migrationBuilder.CreateIndex(
                name: "IX_CategorySchemes_ApiBaseID",
                table: "CategorySchemes",
                column: "ApiBaseID");

            migrationBuilder.CreateIndex(
                name: "IX_Dataflows_ApiBaseID",
                table: "Dataflows",
                column: "ApiBaseID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Categorisations");

            migrationBuilder.DropTable(
                name: "CategorySchemes");

            migrationBuilder.DropTable(
                name: "Dataflows");

            migrationBuilder.DropTable(
                name: "ApiBaseURIes");
        }
    }
}
