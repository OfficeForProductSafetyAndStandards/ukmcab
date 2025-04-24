using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UKMCAB.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Setup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AreaOfCompetencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar(1024)", nullable: false),
                    ProtectionAgainstRiskIds = table.Column<List<Guid>>(type: "uuid[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AreaOfCompetencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar(1024)", nullable: false),
                    LegislativeAreaId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurposeOfAppointmentId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DesignatedStandards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar(1024)", nullable: false),
                    LegislativeAreaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Regulation = table.Column<string>(type: "text", nullable: false),
                    ReferenceNumber = table.Column<List<string>>(type: "text[]", nullable: false),
                    NoticeOfPublicationReference = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesignatedStandards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(1024)", nullable: false),
                    StatusValue = table.Column<string>(type: "varchar(1024)", nullable: false),
                    CABId = table.Column<string>(type: "varchar(1024)", nullable: false),
                    SubStatus = table.Column<string>(type: "varchar(1024)", nullable: false),
                    CreatedByUserGroup = table.Column<string>(type: "varchar(1024)", nullable: false),
                    URLSlug = table.Column<string>(type: "varchar(1024)", nullable: false),
                    Name = table.Column<string>(type: "varchar(1024)", nullable: true),
                    UKASReference = table.Column<string>(type: "varchar(1024)", nullable: true),
                    CABNumber = table.Column<string>(type: "varchar(1024)", nullable: true),
                    CabBlob = table.Column<string>(type: "jsonb", nullable: false),
                    Version = table.Column<string>(type: "varchar(1024)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "LegislativeAreas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar(1024)", nullable: false),
                    Regulation = table.Column<string>(type: "varchar(1024)", nullable: false),
                    HasDataModel = table.Column<bool>(type: "bool", nullable: false),
                    RoleId = table.Column<string>(type: "varchar(1024)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegislativeAreas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PpeCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar(1024)", nullable: false),
                    LegislativeAreaId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PpeCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PpeProductTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar(1024)", nullable: false),
                    PpeCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    LegislativeAreaId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PpeProductTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Procedures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar(1024)", nullable: false),
                    LegislativeAreaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurposeOfAppointmentIds = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
                    CategoryIds = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
                    ProductIds = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
                    PpeProductTypeIds = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
                    ProtectionAgainstRiskIds = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
                    AreaOfCompetencyIds = table.Column<List<Guid>>(type: "uuid[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Procedures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar(1024)", nullable: false),
                    LegislativeAreaId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurposeOfAppointmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubCategoryId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProtectionAgainstRisks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar(1024)", nullable: false),
                    PpeProductTypeIds = table.Column<List<Guid>>(type: "uuid[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtectionAgainstRisks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurposeOfAppointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar(1024)", nullable: false),
                    LegislativeAreaId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurposeOfAppointments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar(1024)", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAccountRequests",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(1000)", nullable: false),
                    AuditLog = table.Column<string>(type: "jsonb", nullable: false),
                    SubjectId = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    Surname = table.Column<string>(type: "text", nullable: true),
                    OrganisationName = table.Column<string>(type: "text", nullable: true),
                    EmailAddress = table.Column<string>(type: "text", nullable: true),
                    ContactEmailAddress = table.Column<string>(type: "text", nullable: true),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ReviewComments = table.Column<string>(type: "text", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    LastUpdatedUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccountRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAccounts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(1000)", nullable: false),
                    AuditLog = table.Column<string>(type: "jsonb", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    Surname = table.Column<string>(type: "text", nullable: true),
                    SurnameNormalized = table.Column<string>(type: "text", nullable: true),
                    OrganisationName = table.Column<string>(type: "text", nullable: true),
                    EmailAddress = table.Column<string>(type: "text", nullable: true),
                    ContactEmailAddress = table.Column<string>(type: "text", nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    IsLocked = table.Column<bool>(type: "bool", nullable: false),
                    LockReason = table.Column<string>(type: "text", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    LastUpdatedUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    LastLogonUtc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    LockReasonDescription = table.Column<string>(type: "text", nullable: true),
                    LockInternalNotes = table.Column<string>(type: "text", nullable: true),
                    Role = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTasks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(1024)", nullable: false),
                    TaskType = table.Column<string>(type: "text", nullable: false),
                    Submitter = table.Column<string>(type: "jsonb", nullable: false),
                    ForRoleId = table.Column<string>(type: "text", nullable: false),
                    Assignee = table.Column<string>(type: "jsonb", nullable: true),
                    Assigned = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    SentOn = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "jsonb", nullable: false),
                    LastUpdatedOn = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    Approved = table.Column<bool>(type: "bool", nullable: true),
                    DeclineReason = table.Column<string>(type: "text", nullable: true),
                    Completed = table.Column<bool>(type: "bool", nullable: false),
                    CabId = table.Column<Guid>(type: "uuid", nullable: true),
                    DocumentLAId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTasks", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AreaOfCompetencies");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "DesignatedStandards");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "LegislativeAreas");

            migrationBuilder.DropTable(
                name: "PpeCategories");

            migrationBuilder.DropTable(
                name: "PpeProductTypes");

            migrationBuilder.DropTable(
                name: "Procedures");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "ProtectionAgainstRisks");

            migrationBuilder.DropTable(
                name: "PurposeOfAppointments");

            migrationBuilder.DropTable(
                name: "SubCategories");

            migrationBuilder.DropTable(
                name: "UserAccountRequests");

            migrationBuilder.DropTable(
                name: "UserAccounts");

            migrationBuilder.DropTable(
                name: "WorkflowTasks");
        }
    }
}
