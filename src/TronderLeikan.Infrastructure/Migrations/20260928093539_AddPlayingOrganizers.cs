using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TronderLeikan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayingOrganizers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<Guid>>(
                name: "PlayingOrganizers",
                table: "Games",
                type: "uuid[]",
                nullable: false,
                defaultValueSql: "'{}'::uuid[]");

            // Spill der flagget var satt: alle arrangørene arver rollen «spilte»
            migrationBuilder.Sql(
                """UPDATE "Games" SET "PlayingOrganizers" = "Organizers" WHERE "IsOrganizersParticipating";""");

            migrationBuilder.DropColumn(
                name: "IsOrganizersParticipating",
                table: "Games");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOrganizersParticipating",
                table: "Games",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Flagget settes hvis minst én arrangør spilte
            migrationBuilder.Sql(
                """UPDATE "Games" SET "IsOrganizersParticipating" = cardinality("PlayingOrganizers") > 0;""");

            migrationBuilder.DropColumn(
                name: "PlayingOrganizers",
                table: "Games");
        }
    }
}
