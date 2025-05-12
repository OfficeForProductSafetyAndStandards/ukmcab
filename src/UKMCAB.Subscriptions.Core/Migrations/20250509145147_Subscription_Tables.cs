using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UKMCAB.Subscriptions.Core.Migrations
{
    /// <inheritdoc />
    public partial class Subscription_Tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubscriptionEntities",
                columns: table => new
                {
                    TableKey = table.Column<string>(type: "text", nullable: false),
                    PartitionKey = table.Column<string>(type: "text", nullable: false),
                    RowKey = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EmailAddress = table.Column<string>(type: "text", nullable: false),
                    Frequency = table.Column<int>(type: "integer", nullable: false),
                    CabId = table.Column<Guid>(type: "uuid", nullable: true),
                    CabName = table.Column<string>(type: "text", nullable: true),
                    SearchQueryString = table.Column<string>(type: "text", nullable: true),
                    SearchKeywords = table.Column<string>(type: "text", nullable: true),
                    DueBaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastThumbprint = table.Column<string>(type: "text", nullable: true),
                    BlobName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionEntities", x => new { x.TableKey, x.PartitionKey, x.RowKey });
                });

            migrationBuilder.CreateTable(
                name: "TableEntities",
                columns: table => new
                {
                    TableKey = table.Column<string>(type: "text", nullable: false),
                    PartitionKey = table.Column<string>(type: "text", nullable: false),
                    RowKey = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableEntities", x => new { x.TableKey, x.PartitionKey, x.RowKey });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriptionEntities");

            migrationBuilder.DropTable(
                name: "TableEntities");
        }
    }
}
