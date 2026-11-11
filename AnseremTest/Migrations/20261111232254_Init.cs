using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnseremTest.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Domains",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Domains", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "DomainStopList",
                columns: table => new
                {
                    DomainName = table.Column<string>(type: "text", nullable: false),
                    StopAddressName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainStopList", x => new { x.DomainName, x.StopAddressName });
                    table.ForeignKey(
                        name: "FK_DomainStopList_Addresses_StopAddressName",
                        column: x => x.StopAddressName,
                        principalTable: "Addresses",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DomainStopList_Domains_DomainName",
                        column: x => x.DomainName,
                        principalTable: "Domains",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DomainSwitchList",
                columns: table => new
                {
                    DomainName = table.Column<string>(type: "text", nullable: false),
                    SwitchAddressName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainSwitchList", x => new { x.DomainName, x.SwitchAddressName });
                    table.ForeignKey(
                        name: "FK_DomainSwitchList_Addresses_SwitchAddressName",
                        column: x => x.SwitchAddressName,
                        principalTable: "Addresses",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DomainSwitchList_Domains_DomainName",
                        column: x => x.DomainName,
                        principalTable: "Domains",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DomainStopList_StopAddressName",
                table: "DomainStopList",
                column: "StopAddressName");

            migrationBuilder.CreateIndex(
                name: "IX_DomainSwitchList_SwitchAddressName",
                table: "DomainSwitchList",
                column: "SwitchAddressName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DomainStopList");

            migrationBuilder.DropTable(
                name: "DomainSwitchList");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "Domains");
        }
    }
}
