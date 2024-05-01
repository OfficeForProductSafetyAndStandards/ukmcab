using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Notify.Interfaces;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using UKMCAB.Core.Domain.Workflow;
using UKMCAB.Core.EmailTemplateOptions;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Data.CosmosDb.Services.CAB;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Services.ReviewDateReminder;

namespace UKMCAB.Web.UI.Tests
{
    [TestFixture]
    public class ReviewDateReminderBackgroundServiceTests
    {
        [Category("Review Date Reminder Happy Path")]
        [Test]
        public async Task ReviewDateReminderBackgroundService_Sends_Notification_When_PublishedCABsAndLAs_ReviewDateAreDue()
        {
            //Arrange
            var loggerMock = new Mock<ILogger<ReviewDateReminderBackgroundService>>();
            var cabRepoMock = new Mock<ICABRepository>();
            var workflowTaskMock = new Mock<IWorkflowTaskService>();
            var notificationMock = new Mock<IAsyncNotificationClient>();
            var mockEmailOptions = Options.Create(new CoreEmailTemplateOptions());
            var appHostMock = new Mock<IAppHost>();
            var linkGenTaskMock = new Mock<LinkGenerator>();
            var telemetry = new TelemetryClient();
            var delayerMock = new Mock<IDelayer>();
            var _sut = new ReviewDateReminderBackgroundService(loggerMock.Object, telemetry,cabRepoMock.Object, workflowTaskMock.Object, notificationMock.Object, mockEmailOptions, linkGenTaskMock.Object, appHostMock.Object, delayerMock.Object);

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var queryResults = new List<Document>
            {
                // Published cabs
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), StatusValue = Status.Published, RenewalDate = DateTime.Today, DocumentLegislativeAreas = new List<DocumentLegislativeArea>{new DocumentLegislativeArea { ReviewDate = DateTime.Today.AddMonths(1)}} },
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), StatusValue = Status.Published, RenewalDate = DateTime.Today.AddMonths(2), DocumentLegislativeAreas = new List<DocumentLegislativeArea>{new DocumentLegislativeArea { ReviewDate = DateTime.Today.AddMonths(2)}} },
            };

            delayerMock.Setup(x => x.Delay(It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(() => { return Task.CompletedTask; });
            cabRepoMock.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(queryResults);
            appHostMock.Setup(x => x.GetBaseUri()).Returns(new Uri("https://example.com"));

            //Act
            await _sut.StartAsync(cancellationToken);

            //Assert
            notificationMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(4));
            workflowTaskMock.Verify(x => x.CreateAsync(It.IsAny<WorkflowTask>()), Times.Exactly(4));
        }

        [Category("Review Date Reminder Happy Path")]
        [Test]
        public async Task ReviewDateReminderBackgroundService_SendsNo_Notification_When_PublishedCABsAndLAs_ReviewDateAreNotDue()
        {
            //Arrange
            var loggerMock = new Mock<ILogger<ReviewDateReminderBackgroundService>>();
            var cabRepoMock = new Mock<ICABRepository>();
            var workflowTaskMock = new Mock<IWorkflowTaskService>();
            var notificationMock = new Mock<IAsyncNotificationClient>();
            var mockEmailOptions = Options.Create(new CoreEmailTemplateOptions());
            var appHostMock = new Mock<IAppHost>();
            var linkGenTaskMock = new Mock<LinkGenerator>();
            var telemetry = new TelemetryClient();
            var delayerMock = new Mock<IDelayer>();
            var _sut = new ReviewDateReminderBackgroundService(loggerMock.Object, telemetry, cabRepoMock.Object, workflowTaskMock.Object, notificationMock.Object, mockEmailOptions, linkGenTaskMock.Object, appHostMock.Object, delayerMock.Object);

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var queryResults = new List<Document>
            {
                // Published cabs
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), StatusValue = Status.Published, RenewalDate = DateTime.Today.AddDays(21), DocumentLegislativeAreas = new List<DocumentLegislativeArea>{new DocumentLegislativeArea { ReviewDate = DateTime.Today.AddDays(1)}} },
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), StatusValue = Status.Published, RenewalDate = DateTime.Today.AddMonths(3), DocumentLegislativeAreas = new List<DocumentLegislativeArea>{new DocumentLegislativeArea { ReviewDate = DateTime.Today.AddMonths(4)}} },
            };

            delayerMock.Setup(x => x.Delay(It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(() => { return Task.CompletedTask; });
            cabRepoMock.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(queryResults);
            appHostMock.Setup(x => x.GetBaseUri()).Returns(new Uri("https://example.com"));

            //Act
            await _sut.StartAsync(cancellationToken);

            //Assert
            notificationMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            workflowTaskMock.Verify(x => x.CreateAsync(It.IsAny<WorkflowTask>()), Times.Never);
        }
    }
}
