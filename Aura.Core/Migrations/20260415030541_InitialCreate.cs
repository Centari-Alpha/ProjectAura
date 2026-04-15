using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aura.Core.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Constellations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Theme = table.Column<string>(type: "TEXT", nullable: false),
                    DominantEssence = table.Column<int>(type: "INTEGER", nullable: false),
                    CenterOfMass_X = table.Column<float>(type: "REAL", nullable: false),
                    CenterOfMass_Y = table.Column<float>(type: "REAL", nullable: false),
                    CenterOfMass_Z = table.Column<float>(type: "REAL", nullable: false),
                    NodeIds = table.Column<string>(type: "TEXT", nullable: false),
                    OverallCohesionScore = table.Column<float>(type: "REAL", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Constellations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThoughtNodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Essence = table.Column<int>(type: "INTEGER", nullable: false),
                    FulfillmentScore = table.Column<float>(type: "REAL", nullable: false),
                    Weight = table.Column<float>(type: "REAL", nullable: false),
                    DailyHomePosition_X = table.Column<float>(type: "REAL", nullable: false),
                    DailyHomePosition_Y = table.Column<float>(type: "REAL", nullable: false),
                    DailyHomePosition_Z = table.Column<float>(type: "REAL", nullable: false),
                    IsOrbiting = table.Column<bool>(type: "INTEGER", nullable: false),
                    Attachments = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThoughtNodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Relationships",
                columns: table => new
                {
                    SourceNodeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetNodeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConnectionStrength = table.Column<float>(type: "REAL", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relationships", x => new { x.SourceNodeId, x.TargetNodeId });
                    table.ForeignKey(
                        name: "FK_Relationships_ThoughtNodes_SourceNodeId",
                        column: x => x.SourceNodeId,
                        principalTable: "ThoughtNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Relationships_ThoughtNodes_TargetNodeId",
                        column: x => x.TargetNodeId,
                        principalTable: "ThoughtNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_TargetNodeId",
                table: "Relationships",
                column: "TargetNodeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Constellations");

            migrationBuilder.DropTable(
                name: "Relationships");

            migrationBuilder.DropTable(
                name: "ThoughtNodes");
        }
    }
}
