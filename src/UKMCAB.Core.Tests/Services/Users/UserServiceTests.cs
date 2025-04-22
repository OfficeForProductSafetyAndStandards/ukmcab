using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using Notify.Interfaces;
using NUnit.Framework;
using UKMCAB.Common.Exceptions;
using UKMCAB.Core.EmailTemplateOptions;
using UKMCAB.Core.Services.Users;
using UKMCAB.Core.Services.Users.Models;
using UKMCAB.Core.Tests.FakeRepositories;
using UKMCAB.Data.Domain;
using UKMCAB.Data.Interfaces.Services.User;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Core.Tests.Services.Users
{
    public class UserServiceTests
    {
        private IUserService _sut;
        private IUserAccountRepository _fakeUserAccountRepository;
        private IUserAccountRequestRepository _fakeUserAccountRequestRepository;

        [SetUp]
        public void Setup()
        {
            _fakeUserAccountRepository = new FakeUserAccountRepository();
            _fakeUserAccountRequestRepository = new FakeUserAccountRequestRepository();
            var mockNotificationService = new Mock<IAsyncNotificationClient>();
            var mockOptions = Options.Create(new CoreEmailTemplateOptions());
            _sut = new UserService(_fakeUserAccountRepository, _fakeUserAccountRequestRepository,
                mockNotificationService.Object, mockOptions, null);
        }

        private void SeedUserAccount()
        {
            _fakeUserAccountRepository.CreateAsync(
                new UserAccount
                {
                    Id = "1",
                    FirstName = "Test1",
                    Surname = "Surname1",
                    EmailAddress = "test1@test.com",
                    ContactEmailAddress = "contactTest1@test.com",
                    OrganisationName = "UKMCAB",
                    Role = "OPSS",
                    CreatedUtc = DateTime.UtcNow.AddDays(-10),
                    LastLogonUtc = DateTime.UtcNow.AddDays(-10),
                    LastUpdatedUtc = DateTime.UtcNow.AddDays(-10),
                    AuditLog = new List<Audit>
                    {
                        new()
                        {
                            DateTime = DateTime.UtcNow.AddDays(-15),
                            Action = AuditUserActions.UserAccountRequest,
                            Comment = "test 1 user",
                            UserName = "Test1 Surname1",
                        },
                        new()
                        {
                            DateTime = DateTime.UtcNow.AddDays(-10),
                            Action = AuditUserActions.ApproveAccountRequest,
                            Comment = "test 1 user approved",
                            UserName = "Admin User",
                            UserRole = "OPSS",
                            UserId = "100"
                        }
                    }
                });
            _fakeUserAccountRepository.CreateAsync(
                new UserAccount
                {
                    Id = "2",
                    FirstName = "Test2",
                    Surname = "Surname2",
                    EmailAddress = "test2@test.com",
                    ContactEmailAddress = "contactTest2@test.com",
                    OrganisationName = "UKAS",
                    Role = "UKAS",
                    CreatedUtc = DateTime.UtcNow.AddDays(-20),
                    LastLogonUtc = DateTime.UtcNow.AddDays(-20),
                    LastUpdatedUtc = DateTime.UtcNow.AddDays(-20),
                    AuditLog = new List<Audit>
                    {
                        new()
                        {
                            DateTime = DateTime.UtcNow.AddDays(-25),
                            Action = AuditUserActions.UserAccountRequest,
                            Comment = "test 2 user",
                            UserName = "Test2 Surname2",
                        },
                        new()
                        {
                            DateTime = DateTime.UtcNow.AddDays(-20),
                            Action = AuditUserActions.ApproveAccountRequest,
                            Comment = "test 2 user approved",
                            UserName = "Admin User",
                            UserRole = "OPSS",
                            UserId = "100"
                        }
                    }
                });
            _fakeUserAccountRepository.CreateAsync(
                new UserAccount
                {
                    Id = "3",
                    FirstName = "Test3",
                    Surname = "Surname3",
                    EmailAddress = "test3@test.com",
                    ContactEmailAddress = "contactTest3@test.com",
                    OrganisationName = "OGD",
                    Role = "UKAS",
                    CreatedUtc = DateTime.UtcNow.AddDays(-30),
                    LastLogonUtc = DateTime.UtcNow.AddDays(-30),
                    LastUpdatedUtc = DateTime.UtcNow.AddDays(-30),
                    AuditLog = new List<Audit>
                    {
                        new()
                        {
                            DateTime = DateTime.UtcNow.AddDays(-35),
                            Action = AuditUserActions.UserAccountRequest,
                            Comment = "test 3 user",
                            UserName = "Test3 Surname3",
                        },
                        new()
                        {
                            DateTime = DateTime.UtcNow.AddDays(-30),
                            Action = AuditUserActions.ApproveAccountRequest,
                            Comment = "test 3 user approved",
                            UserName = "AnotherAdmin User",
                            UserRole = "OPSS",
                            UserId = "200"
                        }
                    }
                });
        }

        [Test]
        public async Task SubmitRequestAccountAsync_Add_Request()
        {
            var newRequest = new RequestAccountModel
            {
                SubjectId = "1",
                FirstName = "Test",
                Surname = "Surname",
                Comments = "test",
                EmailAddress = "test@test.com",
                ContactEmailAddress = "contacttest@test.com",
                Organisation = "ukmcab"
            };
            await _sut.SubmitRequestAccountAsync(newRequest);

            var results = await _sut.ListRequestsAsync(new UserAccountRequestListOptions(new SkipTake(0,10000), new SortBy(nameof(UserAccountRequest.CreatedUtc), null), UserAccountRequestStatus.Pending));

            Assert.That(results.Count(), Is.EqualTo(1));
            var createdRequest = results.First();
            Assert.That(createdRequest.Status == UserAccountRequestStatus.Pending, Is.True);
            Assert.That(createdRequest.SubjectId == "1", Is.True);
            var auditLog = createdRequest.AuditLog;
            Assert.That(auditLog.Count, Is.EqualTo(1));
            var auditEntry = auditLog.First();
            Assert.That(auditEntry.UserName, Is.EqualTo("Test Surname"));
        }

        [Test]
        public async Task SubmitRequestAccountAsync_Add_Duplicate_ThrowsError()
        {
            var newRequest = new RequestAccountModel
            {
                SubjectId = "1",
                FirstName = "Test",
                Surname = "Surname",
                Comments = "test",
                EmailAddress = "test@test.com",
                ContactEmailAddress = "contacttest@test.com",
                Organisation = "ukmcab"
            };
            await _sut.SubmitRequestAccountAsync(newRequest);
            Assert.ThrowsAsync<DomainException>(async () => await _sut.SubmitRequestAccountAsync(newRequest));
        }

        [Test]
        public async Task UpdateLastLogonDate_Succeeds()
        {
            SeedUserAccount();
            var user2Before = await _sut.GetAsync("2");
            var loggedOnBefore = user2Before.LastLogonUtc.Value;
            await _sut.UpdateLastLogonDate("2");
            var user2After = await _sut.GetAsync("2");

            Assert.That(loggedOnBefore, Is.Not.EqualTo(user2After.LastLogonUtc));
        }

        [Test]
        public async Task ApproveWithWrongStatusThrowsException()
        {
            ((FakeUserAccountRequestRepository)_fakeUserAccountRequestRepository).SeedData();

            Assert.ThrowsAsync<DomainException>(async () =>await _sut.ApproveAsync("rejected-user", "UKAS", new UserAccount()));
        }


    }
}
