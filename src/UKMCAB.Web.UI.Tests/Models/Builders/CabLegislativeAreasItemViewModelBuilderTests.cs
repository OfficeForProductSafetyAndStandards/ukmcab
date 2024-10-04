using FluentAssertions;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Core.Security;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.LegislativeAreas;
using UKMCAB.Web.UI.Models.Builders;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;

#pragma warning disable CS8618

namespace UKMCAB.Web.UI.Tests.Models.Builders
{
    [TestFixture]
    public class CabLegislativeAreasItemViewModelBuilderTests
    {
        private CabLegislativeAreasItemViewModelBuilder _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new CabLegislativeAreasItemViewModelBuilder();
        }

        [Test]
        public void WithDocumentLegislativeAreaDetails_PopulatesDocumentLegislativeAreaDetails()
        {
            // Arrange
            var legislativeArea = new LegislativeAreaModel
            {
                Name = "Test name",
                HasDataModel = true,
            };
            var legislativeAreaId = Guid.NewGuid();
            var documentLegislativeArea = new DocumentLegislativeArea
            {
                IsProvisional = true,
                Archived = true,
                AppointmentDate = new DateTime(2024, 1, 1),
                ReviewDate = new DateTime(2024, 1, 1),
                Reason = "Test reason",
                RequestReason = "Test request reason",
                PointOfContactName = "Test contact name",
                PointOfContactEmail = "Test contact email",
                PointOfContactPhone = "Test contact phone",
                IsPointOfContactPublicDisplay = true,
                Status = LAStatus.Approved,
                RoleId = Roles.DFTP.Id,
                LegislativeAreaId = legislativeAreaId, 
                NewlyCreated = true
            };
            var expectedResult = new CABLegislativeAreasItemViewModel
            {
                Name = "Test name",
                IsProvisional = true,
                IsArchived = true,
                AppointmentDate = new DateTime(2024, 1, 1),
                ReviewDate = new DateTime(2024, 1, 1),
                Reason = "Test reason",
                RequestReason = "Test request reason",
                PointOfContactName = "Test contact name",
                PointOfContactEmail = "Test contact email",
                PointOfContactPhone = "Test contact phone",
                IsPointOfContactPublicDisplay = true,
                CanChooseScopeOfAppointment = true,
                Status = LAStatus.Approved,
                StatusCssStyle = "govuk-tag--turquoise",
                RoleName = "DFTP",
                RoleId = documentLegislativeArea.RoleId,
                LegislativeAreaId = documentLegislativeArea.LegislativeAreaId, 
                NewlyCreated = documentLegislativeArea.NewlyCreated
            };

            // Act
            var result = _sut.WithDocumentLegislativeAreaDetails(legislativeArea, documentLegislativeArea).Build();

            // ClassicAssert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void WithScopeOfAppointments_PopulatesScopeOfAppointments()
        {
            // Arrange
            var legislativeArea = CabLegislativeAreasItemViewModelBuilderTestsFixture.LegislativeArea;
            var purposeOfAppointments = CabLegislativeAreasItemViewModelBuilderTestsFixture.PurposeOfAppointments;
            var subCategories = CabLegislativeAreasItemViewModelBuilderTestsFixture.SubCategories;
            var categories = CabLegislativeAreasItemViewModelBuilderTestsFixture.Categories;
            var procedures = CabLegislativeAreasItemViewModelBuilderTestsFixture.Procedures;
            var products = CabLegislativeAreasItemViewModelBuilderTestsFixture.Products;
            var designatedStandards = CabLegislativeAreasItemViewModelBuilderTestsFixture.DesignatedStandards;
            var ppeProductTypes = CabLegislativeAreasItemViewModelBuilderTestsFixture.PpeProductTypes;
            var protectionAgainstRisks = CabLegislativeAreasItemViewModelBuilderTestsFixture.ProtectionAgainstRisks;
            var areaOfCompetencies = CabLegislativeAreasItemViewModelBuilderTestsFixture.AreaOfCompetencies;

            var legislativeAreaId = legislativeArea.Id;
            var purposeOfAppointmentId = purposeOfAppointments.First().Id;
            var subCategoryId = subCategories.First().Id;
            var categoryId = categories.First().Id;
            var procedureId = procedures.First().Id;
            var productId = products.First().Id;

            var documentScopeOfAppointments = CabLegislativeAreasItemViewModelBuilderTestsFixture.DocumentScopeOfAppointments(
                purposeOfAppointmentId,
                subCategoryId,
                categoryId,
                procedureId,
                productId
            );
            var documentScopeOfAppointmentId = documentScopeOfAppointments.First().Id;

            var expectedResult = new List<LegislativeAreaListItemViewModel>
            {
                new
                (
                    legislativeAreaId,
                    "Test legislative area name",
                    "Test purpose of appointment name",
                    "Test category name",
                    "Test subcategory name",
                    documentScopeOfAppointmentId,
                    null,
                    new List<string>{ "Test procedure name" }
                ),
                new 
                (
                    legislativeAreaId,
                    "Test legislative area name",
                    "Test purpose of appointment name",
                    null,
                    "Test subcategory name",
                    documentScopeOfAppointmentId,
                    "Test product name",
                    new List<string>{ "Test procedure name" }
                )
            };

            // Act
            var result = _sut.WithScopeOfAppointments(
                legislativeArea,
                documentScopeOfAppointments,
                purposeOfAppointments,
                categories,
                subCategories,
                products,
                procedures,
                designatedStandards,
                ppeProductTypes,
                protectionAgainstRisks,
                areaOfCompetencies).Build();

            // ClassicAssert
            result.ScopeOfAppointments.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void WithNoOfProductsInScopeOfAppointment_PopulatesNoOfProductsInScopeOfAppointment()
        {
            // Arrange
            var legislativeArea = CabLegislativeAreasItemViewModelBuilderTestsFixture.LegislativeArea;
            var purposeOfAppointments = CabLegislativeAreasItemViewModelBuilderTestsFixture.PurposeOfAppointments;
            var subCategories = CabLegislativeAreasItemViewModelBuilderTestsFixture.SubCategories;
            var categories = CabLegislativeAreasItemViewModelBuilderTestsFixture.Categories;
            var procedures = CabLegislativeAreasItemViewModelBuilderTestsFixture.Procedures;
            var products = CabLegislativeAreasItemViewModelBuilderTestsFixture.Products;
            var designatedStandards = new List<DesignatedStandardModel>();
            var ppeProductTypes = CabLegislativeAreasItemViewModelBuilderTestsFixture.PpeProductTypes;
            var protectionAgainstRisks = CabLegislativeAreasItemViewModelBuilderTestsFixture.ProtectionAgainstRisks;
            var areaOfCompetencies = CabLegislativeAreasItemViewModelBuilderTestsFixture.AreaOfCompetencies;

            var legislativeAreaId = legislativeArea.Id;
            var purposeOfAppointmentId = purposeOfAppointments.First().Id;
            var subCategoryId = subCategories.First().Id;
            var categoryId = categories.First().Id;
            var procedureId = procedures.First().Id;
            var productId = products.First().Id;

            var documentScopeOfAppointments = CabLegislativeAreasItemViewModelBuilderTestsFixture.DocumentScopeOfAppointments(
                purposeOfAppointmentId,
                subCategoryId,
                categoryId,
                procedureId,
                productId
            );

            var builder = _sut.WithScopeOfAppointments(
                legislativeArea,
                documentScopeOfAppointments,
                purposeOfAppointments,
                categories,
                subCategories,
                products,
                procedures,
                designatedStandards,
                ppeProductTypes,
                protectionAgainstRisks,
                areaOfCompetencies);

            var expectedResult = new List<int> { 2, 0 };

            // Act
            var result = builder.WithNoOfProductsInScopeOfAppointment().Build();

            // ClassicAssert
            result.ScopeOfAppointments.Select(s => s.NoOfProductsInScopeOfAppointment).Should().BeEquivalentTo(expectedResult);
        }

        public class CabLegislativeAreasItemViewModelBuilderTestsFixture
        {
            public static LegislativeAreaModel LegislativeArea => new()
            {
                Id = Guid.NewGuid(),
                Name = "Test legislative area name",
            };

            public static List<PurposeOfAppointmentModel> PurposeOfAppointments => new()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Test purpose of appointment name"
                }
            };

            public static List<SubCategoryModel> SubCategories => new()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Test subcategory name"
                }
            };

            public static List<CategoryModel> Categories => new()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Test category name"
                }
            };

            public static List<ProcedureModel> Procedures => new()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Test procedure name"
                }
            };

            public static List<ProductModel> Products => new()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Test product name"
                }
            };

            public static List<DesignatedStandardModel> DesignatedStandards => new()
            {
                new (Guid.NewGuid(), "Test designated standard", Guid.NewGuid(), new List<string>(), "Test publication reference" )
            };

            public static List<PpeProductTypeModel> PpeProductTypes => new()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Test ppe product type name"
                }
            };

            public static List<ProtectionAgainstRiskModel> ProtectionAgainstRisks => new()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Test protection against risk name"
                }
            };

            public static List<AreaOfCompetencyModel> AreaOfCompetencies => new()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Test area of competency name"
                }
            };

            public static List<DocumentScopeOfAppointment> DocumentScopeOfAppointments(Guid purposeOfAppointmentId, Guid subCategoryId, Guid categoryId, Guid procedureId, Guid productId) 
                => new()
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        PurposeOfAppointmentId = purposeOfAppointmentId,
                        SubCategoryId = subCategoryId,
                        CategoryIdAndProcedureIds = new List<CategoryAndProcedures>
                        {
                            new()
                            {
                                CategoryId = categoryId,
                                ProcedureIds = new List<Guid>{ procedureId }
                            }
                        },
                        ProductIdAndProcedureIds = new List<ProductAndProcedures>
                        {
                            new()
                            {
                                ProductId = productId,
                                ProcedureIds = new List<Guid>{ procedureId }
                            }
                        }
                    }
                };
        }
    }
}
