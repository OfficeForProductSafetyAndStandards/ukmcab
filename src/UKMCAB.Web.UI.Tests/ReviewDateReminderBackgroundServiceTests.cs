using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Notify.Interfaces;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UKMCAB.Core.EmailTemplateOptions;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Data.CosmosDb.Services.CAB;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.User;
using UKMCAB.Web.UI.Services;
using UKMCAB.Web.UI.Services.ReviewDateReminder;

namespace UKMCAB.Web.UI.Tests
{
    [TestFixture]
    public class ReviewDateReminderBackgroundServiceTests
    {
        [Category("Review Date Reminder Happy Path")]
        [Test]
        public async Task CheckAndSendReviewDateReminder_Is_Called_When_LastRunTimeIsInThePast()
        {
            //Arrange
            var loggerMock = new Mock<ILogger<ReviewDateReminderBackgroundService>>().Object;
            var cabRepoMock = new Mock<ICABRepository>();
            var workflowTaskMock = new Mock<IWorkflowTaskService>().Object;
            var notificationMock = new Mock<IAsyncNotificationClient>();
            var mockEmailOptions = Options.Create(new CoreEmailTemplateOptions());
            var appHostMock = new Mock<IAppHost>();
            var linkGenTaskMock = new Mock<LinkGenerator>().Object;
            var telemetry = new TelemetryClient();
            var delayerMock = new Mock<IDelayer>();
            var _sut = new ReviewDateReminderBackgroundService(loggerMock,telemetry,cabRepoMock.Object, workflowTaskMock, notificationMock.Object, mockEmailOptions, linkGenTaskMock,appHostMock.Object, delayerMock.Object);

            var _sut2 = new Mock<ReviewDateReminderBackgroundService> { CallBase = true };
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var queryResults = new List<Document>
            {
                // Publish:
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), StatusValue = Status.Published, RenewalDate = DateTime.Today, DocumentLegislativeAreas = new List<DocumentLegislativeArea>{new DocumentLegislativeArea { AppointmentDate = DateTime.Today.AddMonths(1)}} },
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), StatusValue = Status.Published, RenewalDate = DateTime.Today.AddMonths(2), DocumentLegislativeAreas = new List<DocumentLegislativeArea>{new DocumentLegislativeArea { AppointmentDate = DateTime.Today.AddMonths(2)}} },
            };

            delayerMock.Setup(x => x.Delay(It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(() => { return Task.CompletedTask; });
            cabRepoMock.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(queryResults);
            appHostMock.Setup(x => x.GetBaseUri()).Returns(new Uri("https://example.com"));

            //Act
            await _sut.StartAsync(cancellationToken);

            //Assert
            notificationMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce());
        }

    }
}
