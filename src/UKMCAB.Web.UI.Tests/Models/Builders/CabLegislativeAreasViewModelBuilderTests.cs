using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.LegislativeAreas;
using UKMCAB.Web.UI.Models.Builders;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

#pragma warning disable CS8618

namespace UKMCAB.Web.UI.Tests.Models.Builders
{
    [TestFixture]
    public class CabLegislativeAreasViewModelBuilderTests
    {
        private Mock<ICabLegislativeAreasItemViewModelBuilder> _mockCabLegislativeAreasItemViewModelBuilder;

        private CabLegislativeAreasViewModelBuilder _sut;

        [SetUp]
        public void Setup()
        {
            _mockCabLegislativeAreasItemViewModelBuilder = new Mock<ICabLegislativeAreasItemViewModelBuilder>(MockBehavior.Strict);
            _sut = new CabLegislativeAreasViewModelBuilder(_mockCabLegislativeAreasItemViewModelBuilder.Object);
        }

        [Test]
        public void WithDocumentLegislativeAreas_PopulatesActiveLegislativeAreas()
        {
            // Act
            var result = WithDocumentLegislativeAreas_PopulatesLegislativeAreas(false);

            // ClassicAssert
            result.ActiveLegislativeAreas.Count.Should().Be(1);
            result.ArchivedLegislativeAreas.Count.Should().Be(0);
        }

        [Test]
        public void WithDocumentLegislativeAreas_PopulatesArchivedLegislativeAreas()
        {
            // Act
            var result = WithDocumentLegislativeAreas_PopulatesLegislativeAreas(true);

            // ClassicAssert
            result.ActiveLegislativeAreas.Count.Should().Be(0);
            result.ArchivedLegislativeAreas.Count.Should().Be(1);
        }

        private CABLegislativeAreasViewModel WithDocumentLegislativeAreas_PopulatesLegislativeAreas(bool isArchived)
        {
            // Arrange
            var legislativeAreaId = Guid.NewGuid();

            var documentLegislativeAreas = new List<DocumentLegislativeArea>
            {
                new()
                {
                    LegislativeAreaId = legislativeAreaId
                }
            };
            var legislativeAreas = new List<LegislativeAreaModel>
            {
                new()
                {
                    Id = legislativeAreaId
                }
            };
            var scopeOfAppointments = new List<DocumentScopeOfAppointment>
            {
                new()
                {
                    LegislativeAreaId = legislativeAreaId
                }
            };
            var expectedScopeOfAppointmentIds = scopeOfAppointments.Select(s => s.LegislativeAreaId);
            var cabLegislativeAreasItemViewModel = new CABLegislativeAreasItemViewModel
            {
                IsArchived = isArchived
            };

            _mockCabLegislativeAreasItemViewModelBuilder
                .Setup(m => m.WithDocumentLegislativeAreaDetails(
                    It.Is<LegislativeAreaModel>(la => la.Id == legislativeAreaId),
                    It.Is<DocumentLegislativeArea>(la => la.LegislativeAreaId == legislativeAreaId)))
                .Returns(_mockCabLegislativeAreasItemViewModelBuilder.Object);
            _mockCabLegislativeAreasItemViewModelBuilder
                .Setup(m => m.WithScopeOfAppointments(
                    It.Is<LegislativeAreaModel>(la => la.Id == legislativeAreaId),
                    It.Is<List<DocumentScopeOfAppointment>>(soas => soas.Any() && soas.All(s => expectedScopeOfAppointmentIds.Contains(s.LegislativeAreaId))),
                    It.IsAny<List<PurposeOfAppointmentModel>>(),
                    It.IsAny<List<CategoryModel>>(),
                    It.IsAny<List<SubCategoryModel>>(),
                    It.IsAny<List<ProductModel>>(),
                    It.IsAny<List<ProcedureModel>>(),
                    It.IsAny<List<DesignatedStandardModel>>(),
                    It.IsAny<List<PpeProductTypeModel>>(),
                    It.IsAny<List<ProtectionAgainstRiskModel>>(),
                    It.IsAny<List<AreaOfCompetencyModel>>()))
                .Returns(_mockCabLegislativeAreasItemViewModelBuilder.Object);
            _mockCabLegislativeAreasItemViewModelBuilder.Setup(m => m.WithNoOfProductsInScopeOfAppointment()).Returns(_mockCabLegislativeAreasItemViewModelBuilder.Object);
            _mockCabLegislativeAreasItemViewModelBuilder.Setup(m => m.Build()).Returns(cabLegislativeAreasItemViewModel);

            // Act
            var result = _sut.WithDocumentLegislativeAreas(
                documentLegislativeAreas,
                legislativeAreas,
                scopeOfAppointments,
                new List<PurposeOfAppointmentModel>(),
                new List<CategoryModel>(),
                new List<SubCategoryModel>(),
                new List<ProductModel>(),
                new List<ProcedureModel>(),
                new List<DesignatedStandardModel>(),
                new List<PpeProductTypeModel>(),
                new List<ProtectionAgainstRiskModel>(),
                new List<AreaOfCompetencyModel>()).Build();

            // ClassicAssert
            return result;
        }
    }
}
