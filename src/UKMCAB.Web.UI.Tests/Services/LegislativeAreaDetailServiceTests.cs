using Moq;
using NUnit.Framework;
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
            Assert.AreEqual(legislativeArea.Name, result.Name);
            Assert.AreEqual(documentLegislativeArea.IsProvisional, result.IsProvisional);
            Assert.AreEqual(documentLegislativeArea.AppointmentDate, result.AppointmentDate);
            Assert.AreEqual(documentLegislativeArea.ReviewDate, result.ReviewDate);
            Assert.AreEqual(documentLegislativeArea.Reason, result.Reason);
            Assert.AreEqual(documentLegislativeArea.RequestReason, result.RequestReason);
            Assert.AreEqual(legislativeArea.HasDataModel, result.CanChooseScopeOfAppointment);
            Assert.AreEqual(legislativeAreaId, result.LegislativeAreaId);
        }
    }
}
