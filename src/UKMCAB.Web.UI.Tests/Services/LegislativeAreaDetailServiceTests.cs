using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Threading.Tasks;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Web.UI.Services;
using UKMCAB.Data.Models;
using System;
using System.Collections.Generic;
using UKMCAB.Core.Domain.LegislativeAreas;

namespace UKMCAB.Web.UI.Tests.Services
{
    [TestFixture]
    public class LegislativeAreaDetailServiceTests
    {
        private Mock<ILegislativeAreaService> _mockLegislativeAreaService = null!;
        private ILegislativeAreaDetailService _sut = null!;


        [SetUp]
        public void Setup()
        {
            _mockLegislativeAreaService = new Mock<ILegislativeAreaService>(MockBehavior.Strict);
            _sut = new LegislativeAreaDetailService(_mockLegislativeAreaService.Object);
        }

        [Test]
        public async Task ShouldReturnTheCABLegislativeAreasItemViewModel_When_DocumentLegislativeArea_And_LegislativeAreaModel_AreFound()
        {
            //Arrange
            var legislativeAreaId = Guid.NewGuid();

            var legislativeArea = new LegislativeAreaModel
            {
                Name = "Test name",
                HasDataModel = true,
                Id = legislativeAreaId,
            };

            var documentLegislativeArea = new DocumentLegislativeArea
            {
                IsProvisional = true,
                AppointmentDate = new DateTime(2024, 1, 1),
                ReviewDate = new DateTime(2024, 1, 1),
                Reason = "Test reason",
                RequestReason = "Test request reason",
                LegislativeAreaId = legislativeAreaId,
                NewlyCreated = false
            };

            _mockLegislativeAreaService.Setup(m => m.GetLegislativeAreaByIdAsync(legislativeAreaId)).ReturnsAsync(legislativeArea);

            //Act
            var result = await _sut.PopulateCABLegislativeAreasItemViewModelAsync(
                new Document
                {
                    DocumentLegislativeAreas = new List<DocumentLegislativeArea>
                    {
                        documentLegislativeArea
                    }
                },
                legislativeAreaId);

            //Assert
            ClassicAssert.AreEqual(legislativeArea.Name, result.Name);
            ClassicAssert.AreEqual(documentLegislativeArea.IsProvisional, result.IsProvisional);
            ClassicAssert.AreEqual(documentLegislativeArea.AppointmentDate, result.AppointmentDate);
            ClassicAssert.AreEqual(documentLegislativeArea.ReviewDate, result.ReviewDate);
            ClassicAssert.AreEqual(documentLegislativeArea.Reason, result.Reason);
            ClassicAssert.AreEqual(documentLegislativeArea.RequestReason, result.RequestReason);
            ClassicAssert.AreEqual(legislativeArea.HasDataModel, result.CanChooseScopeOfAppointment);
            ClassicAssert.AreEqual(legislativeAreaId, result.LegislativeAreaId);
            ClassicAssert.AreEqual(documentLegislativeArea.NewlyCreated, result.NewlyCreated);
        }
    }
}
