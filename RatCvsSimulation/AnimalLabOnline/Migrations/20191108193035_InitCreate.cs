using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AnimalLabOnline.Migrations
{
    public partial class InitCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Drugs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Smiles = table.Column<string>(nullable: false),
                    Depict2D = table.Column<string>(nullable: false),
                    Fingerprint = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drugs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Index = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Desc = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Experiments",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Author = table.Column<string>(nullable: false),
                    Url = table.Column<string>(nullable: false),
                    ArticleTitle = table.Column<string>(nullable: false),
                    Results = table.Column<string>(nullable: false),
                    PValues = table.Column<string>(nullable: false),
                    SubjectId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Experiments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Experiments_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpDrugs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Index = table.Column<int>(nullable: false),
                    ExperimentId = table.Column<Guid>(nullable: false),
                    DrugId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpDrugs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpDrugs_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpDrugs_Experiments_ExperimentId",
                        column: x => x.ExperimentId,
                        principalTable: "Experiments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Index = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Speciality = table.Column<string>(nullable: true),
                    ExperimentId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpGroups_Experiments_ExperimentId",
                        column: x => x.ExperimentId,
                        principalTable: "Experiments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpIndicators",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Index = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Speciality = table.Column<string>(nullable: true),
                    Unit = table.Column<string>(nullable: true),
                    UnitFull = table.Column<string>(nullable: true),
                    OnFigure = table.Column<bool>(nullable: false),
                    ExperimentId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpIndicators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpIndicators_Experiments_ExperimentId",
                        column: x => x.ExperimentId,
                        principalTable: "Experiments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Protocols",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Treatment = table.Column<int>(nullable: false),
                    TreatmentName = table.Column<string>(nullable: true),
                    Speciality = table.Column<string>(nullable: true),
                    AdminMethod = table.Column<int>(nullable: false),
                    Dose = table.Column<float>(nullable: true),
                    DoseUnit = table.Column<int>(nullable: false),
                    ExpGroupId = table.Column<Guid>(nullable: false),
                    ExpDrugId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Protocols", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Protocols_ExpDrugs_ExpDrugId",
                        column: x => x.ExpDrugId,
                        principalTable: "ExpDrugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Protocols_ExpGroups_ExpGroupId",
                        column: x => x.ExpGroupId,
                        principalTable: "ExpGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpDrugs_DrugId",
                table: "ExpDrugs",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpDrugs_ExperimentId",
                table: "ExpDrugs",
                column: "ExperimentId");

            migrationBuilder.CreateIndex(
                name: "IX_Experiments_SubjectId",
                table: "Experiments",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpGroups_ExperimentId",
                table: "ExpGroups",
                column: "ExperimentId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpIndicators_ExperimentId",
                table: "ExpIndicators",
                column: "ExperimentId");

            migrationBuilder.CreateIndex(
                name: "IX_Protocols_ExpDrugId",
                table: "Protocols",
                column: "ExpDrugId");

            migrationBuilder.CreateIndex(
                name: "IX_Protocols_ExpGroupId",
                table: "Protocols",
                column: "ExpGroupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExpIndicators");

            migrationBuilder.DropTable(
                name: "Protocols");

            migrationBuilder.DropTable(
                name: "ExpDrugs");

            migrationBuilder.DropTable(
                name: "ExpGroups");

            migrationBuilder.DropTable(
                name: "Drugs");

            migrationBuilder.DropTable(
                name: "Experiments");

            migrationBuilder.DropTable(
                name: "Subjects");
        }
    }
}
