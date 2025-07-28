using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Magneto.Contract.Migrations
{
    /// <inheritdoc />
    public partial class _0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "uav_def_playback_files",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileName = table.Column<string>(type: "text", nullable: true),
                    FileType = table.Column<int>(type: "integer", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    UpDatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Segments = table.Column<string>(type: "text", nullable: true),
                    TotalFrames = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_uav_def_playback_files", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "uav_def_records",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    InvasionArea = table.Column<int>(type: "integer", nullable: false),
                    DetectionEquipments = table.Column<string[]>(type: "text[]", nullable: true),
                    NumOfFlyingObjects = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_uav_def_records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "uav_def_white_lists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DroneSerialNum = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_uav_def_white_lists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "uav_def_disposals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    RecordId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_uav_def_disposals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_uav_def_disposals_uav_def_records_RecordId",
                        column: x => x.RecordId,
                        principalTable: "uav_def_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "uav_def_evidence",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Model = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    RadioFrequency = table.Column<double>(type: "double precision", nullable: false),
                    ElectronicFingerprint = table.Column<string>(type: "text", nullable: true),
                    LastFlightLatitude = table.Column<double>(type: "double precision", nullable: false),
                    LastFlightLongitude = table.Column<double>(type: "double precision", nullable: false),
                    PilotLatitude = table.Column<double>(type: "double precision", nullable: false),
                    PilotLongitude = table.Column<double>(type: "double precision", nullable: false),
                    ReturnLatitude = table.Column<double>(type: "double precision", nullable: false),
                    ReturnLongitude = table.Column<double>(type: "double precision", nullable: false),
                    LastFlightVerticalSpeed = table.Column<double>(type: "double precision", nullable: false),
                    LastFlightHorizontalSpeed = table.Column<double>(type: "double precision", nullable: false),
                    LastFlightAltitude = table.Column<double>(type: "double precision", nullable: false),
                    LastFlightBearing = table.Column<double>(type: "double precision", nullable: false),
                    RecordId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_uav_def_evidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_uav_def_evidence_uav_def_records_RecordId",
                        column: x => x.RecordId,
                        principalTable: "uav_def_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "uav_def_uav_paths",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UavSerialNum = table.Column<string>(type: "text", nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    RecordId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_uav_def_uav_paths", x => x.Id);
                    table.ForeignKey(
                        name: "FK_uav_def_uav_paths_uav_def_records_RecordId",
                        column: x => x.RecordId,
                        principalTable: "uav_def_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "uav_def_evd_and_files",
                columns: table => new
                {
                    EvdId = table.Column<int>(type: "integer", nullable: false),
                    FileId = table.Column<int>(type: "integer", nullable: false),
                    EvidenceId = table.Column<int>(type: "integer", nullable: true),
                    PlaybackFileId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_uav_def_evd_and_files", x => new { x.EvdId, x.FileId });
                    table.ForeignKey(
                        name: "FK_uav_def_evd_and_files_uav_def_evidence_EvidenceId",
                        column: x => x.EvidenceId,
                        principalTable: "uav_def_evidence",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_uav_def_evd_and_files_uav_def_playback_files_PlaybackFileId",
                        column: x => x.PlaybackFileId,
                        principalTable: "uav_def_playback_files",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_uav_def_disposals_RecordId",
                table: "uav_def_disposals",
                column: "RecordId");

            migrationBuilder.CreateIndex(
                name: "IX_uav_def_evd_and_files_EvidenceId",
                table: "uav_def_evd_and_files",
                column: "EvidenceId");

            migrationBuilder.CreateIndex(
                name: "IX_uav_def_evd_and_files_PlaybackFileId",
                table: "uav_def_evd_and_files",
                column: "PlaybackFileId");

            migrationBuilder.CreateIndex(
                name: "IX_uav_def_evidence_RecordId",
                table: "uav_def_evidence",
                column: "RecordId");

            migrationBuilder.CreateIndex(
                name: "IX_uav_def_uav_paths_RecordId",
                table: "uav_def_uav_paths",
                column: "RecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "uav_def_disposals");

            migrationBuilder.DropTable(
                name: "uav_def_evd_and_files");

            migrationBuilder.DropTable(
                name: "uav_def_uav_paths");

            migrationBuilder.DropTable(
                name: "uav_def_white_lists");

            migrationBuilder.DropTable(
                name: "uav_def_evidence");

            migrationBuilder.DropTable(
                name: "uav_def_playback_files");

            migrationBuilder.DropTable(
                name: "uav_def_records");
        }
    }
}
