using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UKMCAB.Core.Services.CAB;
using Microsoft.ApplicationInsights;
using UKMCAB.Data.CosmosDb.Services.CachedCAB;
using UKMCAB.Data.Search.Services;
using UKMCAB.Data.CosmosDb.Services.CAB;
using UKMCAB.Data.Models;
using UKMCAB.Core.Mappers;
using System.Linq;
using AutoMapper;
using Bogus;
using UKMCAB.Data.Storage;
using UKMCAB.Core.Security;

namespace UKMCAB.Core.Tests.Services.CAB
{
    [TestFixture]
    public partial class CABAdminServiceTests
    {     
        private Mock<ICABRepository> _mockCABRepository = null!;
        private Mock<ICachedPublishedCABService> _mockCachedPublishedCAB = null!;
        private Mock<ICachedSearchService> _mockCachedSearchService = null!;
        private Mock<IFileStorage> _mockFileStorage = null!;
        private TelemetryClient _telemetryClient = null!;        
        private ICABAdminService _sut = null!;
        private readonly Faker _faker = new();

        [SetUp] 
        public void Setup() {
            _mockCABRepository = new Mock<ICABRepository>();
            _mockCachedPublishedCAB = new Mock<ICachedPublishedCABService>();
            _mockCachedSearchService = new Mock<ICachedSearchService>();
            _telemetryClient = new TelemetryClient();
            _mockFileStorage = new Mock<IFileStorage>();
            var mapper = new MapperConfiguration(mc => { mc.AddProfile(new AutoMapperProfile()); }).CreateMapper();

            _sut = new CABAdminService(_mockCABRepository.Object, _mockCachedSearchService.Object,_mockCachedPublishedCAB.Object, new Mock<ILegislativeAreaService>().Object, _mockFileStorage.Object, _telemetryClient, mapper);
        }

        [Theory]
        [TestCase("opss")]
        public async Task FindAllCABManagementQueueDocumentsForUserRole_OPSS_ShouldReturnCorrectResults(string role)
        {
            // Arrange
            var queryResults = new List<Document>
            {
                // Draft:
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), StatusValue = Status.Draft, SubStatus = SubStatus.None, CreatedByUserGroup = Roles.UKAS.Id },
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), StatusValue = Status.Draft, SubStatus = SubStatus.None, CreatedByUserGroup = Roles.OPSS.Id },

                // Pending draft:
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), StatusValue = Status.Published, SubStatus = SubStatus.PendingApprovalToUnpublish, CreatedByUserGroup = Roles.UKAS.Id },
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), StatusValue = Status.Archived, SubStatus = SubStatus.PendingApprovalToUnarchive, CreatedByUserGroup = Roles.UKAS.Id },
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), StatusValue = Status.Published, SubStatus = SubStatus.PendingApprovalToUnpublish, CreatedByUserGroup = Roles.OPSS.Id },
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), StatusValue = Status.Archived, SubStatus = SubStatus.PendingApprovalToUnarchive, CreatedByUserGroup = Roles.OPSS.Id },

                // Pending publish:
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), StatusValue = Status.Draft, SubStatus = SubStatus.PendingApprovalToPublish, CreatedByUserGroup = Roles.UKAS.Id },
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), StatusValue = Status.Archived, SubStatus = SubStatus.PendingApprovalToUnarchivePublish, CreatedByUserGroup = Roles.UKAS.Id },
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), StatusValue = Status.Draft, SubStatus = SubStatus.PendingApprovalToPublish, CreatedByUserGroup = Roles.OPSS.Id },
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), StatusValue = Status.Archived, SubStatus = SubStatus.PendingApprovalToUnarchivePublish, CreatedByUserGroup = Roles.OPSS.Id },

                // Pending archive:
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), StatusValue = Status.Published, SubStatus = SubStatus.PendingApprovalToArchive, CreatedByUserGroup = Roles.UKAS.Id },
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), StatusValue = Status.Published, SubStatus = SubStatus.PendingApprovalToArchive, CreatedByUserGroup = Roles.OPSS.Id },
            };

            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(queryResults);

            // Act
            var result = await _sut.FindAllCABManagementQueueDocumentsForUserRole(role);

            // Assert
            Assert.AreEqual(12, result.AllCabs.Count());
            Assert.AreEqual(2, result.DraftCabs.Count());
            Assert.AreEqual(4, result.PendingDraftCabs.Count());
            Assert.AreEqual(4, result.PendingPublishCabs.Count());
            Assert.AreEqual(2, result.PendingArchiveCabs.Count());

            Assert.AreEqual(result.AllCabs[0].CABId, queryResults[0].CABId);
            Assert.AreEqual(result.AllCabs[1].CABId, queryResults[1].CABId);
            Assert.AreEqual(result.AllCabs[2].CABId, queryResults[2].CABId);
            Assert.AreEqual(result.AllCabs[3].CABId, queryResults[3].CABId);
            Assert.AreEqual(result.AllCabs[4].CABId, queryResults[4].CABId);
            Assert.AreEqual(result.AllCabs[5].CABId, queryResults[5].CABId);
            Assert.AreEqual(result.AllCabs[6].CABId, queryResults[6].CABId);
            Assert.AreEqual(result.AllCabs[7].CABId, queryResults[7].CABId);
            Assert.AreEqual(result.AllCabs[8].CABId, queryResults[8].CABId);
            Assert.AreEqual(result.AllCabs[9].CABId, queryResults[9].CABId);
            Assert.AreEqual(result.AllCabs[10].CABId, queryResults[10].CABId);
            Assert.AreEqual(result.AllCabs[11].CABId, queryResults[11].CABId);

            Assert.AreEqual(result.DraftCabs[0].CABId, queryResults[0].CABId);
            Assert.AreEqual(result.DraftCabs[1].CABId, queryResults[1].CABId);

            Assert.AreEqual(result.PendingDraftCabs[0].CABId, queryResults[2].CABId);
            Assert.AreEqual(result.PendingDraftCabs[1].CABId, queryResults[3].CABId);
            Assert.AreEqual(result.PendingDraftCabs[2].CABId, queryResults[4].CABId);
            Assert.AreEqual(result.PendingDraftCabs[3].CABId, queryResults[5].CABId);

            Assert.AreEqual(result.PendingPublishCabs[0].CABId, queryResults[6].CABId);
            Assert.AreEqual(result.PendingPublishCabs[1].CABId, queryResults[7].CABId);
            Assert.AreEqual(result.PendingPublishCabs[2].CABId, queryResults[8].CABId);
            Assert.AreEqual(result.PendingPublishCabs[3].CABId, queryResults[9].CABId);

            Assert.AreEqual(result.PendingArchiveCabs[0].CABId, queryResults[10].CABId);
            Assert.AreEqual(result.PendingArchiveCabs[1].CABId, queryResults[11].CABId);
        }

        [Theory]
        [TestCase("ukas")]
        public async Task FindAllCABManagementQueueDocumentsForUserRole_UKAS_ShouldReturnCorrectResults(string role)
        {
            // Arrange
            var queryResults = new List<Document>
            {
                // Draft:
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), StatusValue = Status.Draft, SubStatus = SubStatus.None, CreatedByUserGroup = Roles.UKAS.Id },

                // Pending draft:
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), StatusValue = Status.Published, SubStatus = SubStatus.PendingApprovalToUnpublish, CreatedByUserGroup = Roles.UKAS.Id },
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), StatusValue = Status.Archived, SubStatus = SubStatus.PendingApprovalToUnarchive, CreatedByUserGroup = Roles.UKAS.Id },

                // Pending publish:
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), StatusValue = Status.Draft, SubStatus = SubStatus.PendingApprovalToPublish, CreatedByUserGroup = Roles.UKAS.Id },
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), StatusValue = Status.Archived, SubStatus = SubStatus.PendingApprovalToUnarchivePublish, CreatedByUserGroup = Roles.UKAS.Id },

                // Pending archive:
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), StatusValue = Status.Published, SubStatus = SubStatus.PendingApprovalToArchive, CreatedByUserGroup = Roles.UKAS.Id },
            };

            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(queryResults);

            // Act
            var result = await _sut.FindAllCABManagementQueueDocumentsForUserRole(role);

            // Assert
            Assert.AreEqual(6, result.AllCabs.Count());
            Assert.AreEqual(1, result.DraftCabs.Count());
            Assert.AreEqual(2, result.PendingDraftCabs.Count());
            Assert.AreEqual(2, result.PendingPublishCabs.Count());
            Assert.AreEqual(1, result.PendingArchiveCabs.Count());

            Assert.AreEqual(result.AllCabs[0].CABId, queryResults[0].CABId);
            Assert.AreEqual(result.AllCabs[1].CABId, queryResults[1].CABId);
            Assert.AreEqual(result.AllCabs[2].CABId, queryResults[2].CABId);
            Assert.AreEqual(result.AllCabs[3].CABId, queryResults[3].CABId);
            Assert.AreEqual(result.AllCabs[4].CABId, queryResults[4].CABId);
            Assert.AreEqual(result.AllCabs[5].CABId, queryResults[5].CABId);

            Assert.AreEqual(result.DraftCabs[0].CABId, queryResults[0].CABId);

            Assert.AreEqual(result.PendingDraftCabs[0].CABId, queryResults[1].CABId);
            Assert.AreEqual(result.PendingDraftCabs[1].CABId, queryResults[2].CABId);

            Assert.AreEqual(result.PendingPublishCabs[0].CABId, queryResults[3].CABId);
            Assert.AreEqual(result.PendingPublishCabs[1].CABId, queryResults[4].CABId);

            Assert.AreEqual(result.PendingArchiveCabs[0].CABId, queryResults[5].CABId);
        }
    }
}
