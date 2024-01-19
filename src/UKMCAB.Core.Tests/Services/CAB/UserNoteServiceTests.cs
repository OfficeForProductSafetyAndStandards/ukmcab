namespace UKMCAB.Core.Tests.Services.CAB
{
    using Bogus;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using UKMCAB.Common;
    using UKMCAB.Core.Services.CAB;
    using UKMCAB.Data.CosmosDb.Services.CAB;
    using UKMCAB.Data.CosmosDb.Services.CachedCAB;
    using UKMCAB.Data.Models;
    using UKMCAB.Data.Models.Users;

    [TestFixture]
    public class UserNoteServiceTests
    {
        private Mock<ICABRepository> _mockCABRepository;
        private Mock<ICachedPublishedCABService> _mockCachedPublishedCAB;
        private UserNoteService _userNoteService;
        private readonly Faker _faker = new();

        [SetUp]
        public void Setup()
        {
            _mockCABRepository = new Mock<ICABRepository>();
            _mockCachedPublishedCAB = new Mock<ICachedPublishedCABService>();

            _userNoteService = new UserNoteService(_mockCABRepository.Object, _mockCachedPublishedCAB.Object);
        }

        [Test]
        public async Task UserNoteService_GetAllUserNotesForCabDocumentId_ShouldReturnAllUserNotesForThatCabDocument()
        {
            // Arrange
            var cabDocumentId = Guid.NewGuid();
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>() {
                    new Document { id = cabDocumentId.ToString(),
                        GovernmentUserNotes = new List<UserNote> {
                            new UserNote { Id = Guid.NewGuid(), Note = "First user note" },
                            new UserNote { Id = Guid.NewGuid(), Note = "Second user note" },
                            new UserNote { Id = Guid.NewGuid(), Note = "Third user note" },
                        }
                    },
                });

            // Act
            var userNotes = await _userNoteService.GetAllUserNotesForCabDocumentId(cabDocumentId);

            // Assert
            Assert.IsNotNull(userNotes);
            Assert.AreEqual(3, userNotes.Count());
            Assert.AreEqual("First user note", userNotes.ElementAt(0).Note);
            Assert.AreEqual("Second user note", userNotes.ElementAt(1).Note);
            Assert.AreEqual("Third user note", userNotes.ElementAt(2).Note);
        }

        [Test]
        public async Task UserNoteService_GetUserNote_ShouldReturnUserNoteWithMatchingId()
        {
            // Arrange
            var cabId = Guid.NewGuid();
            var cabDocumentId = Guid.NewGuid();
            var userNoteId = Guid.NewGuid();
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>() {
                    new Document { id = cabDocumentId.ToString(), CABId = cabId.ToString(), URLSlug = "urlSlug", 
                        GovernmentUserNotes = new List<UserNote> {
                            new UserNote { Id = Guid.NewGuid(), Note = "Non-matching note" },
                            new UserNote { Id = userNoteId, Note = "Matching note" },
                            new UserNote { Id = Guid.NewGuid(), Note = "Non-matching note" },
                            new UserNote { Id = Guid.NewGuid(), Note = "Non-matching note" },
                        }
                    },
                });

            // Act
            var userNote = await _userNoteService.GetUserNote(cabDocumentId, userNoteId);

            // Assert
            Assert.IsNotNull(userNote);
            Assert.AreEqual(userNoteId, userNote.Id);
        }

        [Test]
        public async Task UserNoteService_GetUserNote_ShouldThrowExceptionIfCabDocumentNotFound()
        {
            // Arrange
            var cabDocumentId = Guid.NewGuid();
            var userNoteId = Guid.NewGuid();
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>());

            // Act
            Exception exception = Assert.ThrowsAsync<Exception>(async () => await _userNoteService.GetUserNote(cabDocumentId, userNoteId));

            // Assert
            Assert.AreEqual($"CAB document not found. Document ID {cabDocumentId}. Note: this parameter is the Document.id, not the Document.CABId.", exception.Message);
        }

        [Test]
        public async Task UserNoteService_GetUserNote_ShouldThrowExceptionIfUserNoteNotFound()
        {
            // Arrange
            var cabId = Guid.NewGuid();
            var cabDocumentId = Guid.NewGuid();
            var userNoteId = Guid.NewGuid();
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>() {
                    new Document { id = cabDocumentId.ToString(), CABId = cabId.ToString(), URLSlug = "urlSlug",
                        GovernmentUserNotes = new List<UserNote> {
                            new UserNote { Id = Guid.NewGuid(), Note = "Non-matching note" },
                            new UserNote { Id = Guid.NewGuid(), Note = "Non-matching note" },
                            new UserNote { Id = Guid.NewGuid(), Note = "Non-matching note" },
                        }
                    },
                });

            // Act
            Exception exception = Assert.ThrowsAsync<Exception>(async () => await _userNoteService.GetUserNote(cabDocumentId, userNoteId));

            // Assert
            Assert.AreEqual($"UserNote not found. UserNote ID: {userNoteId}.", exception.Message);
        }

        [Test]
        public async Task UserNoteService_CreateUserNote_ShouldCreateNewNoteWithCorrectParams()
        {
            // Arrange
            var cabId = Guid.NewGuid();
            var cabDocumentId = Guid.NewGuid();
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>() {
                    new Document { id = cabDocumentId.ToString(), CABId = cabId.ToString(), URLSlug = "urlSlug",
                        GovernmentUserNotes = new List<UserNote> {
                            new UserNote { Id = Guid.NewGuid(), Note = "Non-matching note" },
                            new UserNote { Id = Guid.NewGuid(), Note = "Non-matching note" },
                            new UserNote { Id = Guid.NewGuid(), Note = "Non-matching note" },
                        }
                    },
                });

            var userAccount = new UserAccount()
            {
                Id = "userid",
                FirstName = "First",
                Surname = "Last",
                Role = "role"
            };
            string note = "Note content text.";

            // Act
            await _userNoteService.CreateUserNote(userAccount, cabDocumentId, note);

            // Assert
            _mockCABRepository.Verify(x => x.UpdateAsync(It.Is<Document>(x =>
                x.id == cabDocumentId.ToString() && 
                x.GovernmentUserNotes.Count == 4 &&
                x.GovernmentUserNotes.Last().Note == note &&
                x.GovernmentUserNotes.Last().UserName == "First Last" &&
                x.GovernmentUserNotes.Last().UserId == "userid" &&
                x.GovernmentUserNotes.Last().UserRole == "role" &&
                x.GovernmentUserNotes.Last().DateTime.ToStringBeisFormat() == DateTime.Now.ToStringBeisFormat())), Times.Once);

            _mockCachedPublishedCAB.Verify(x => x.ClearAsync(cabId.ToString(), "urlSlug"), Times.Once);
        }

        [Test]
        public async Task UserNoteService_CreateUserNote_ShouldThrowExceptionIfCabDocumentNotFound()
        {
            // Arrange
            var cabDocumentId = Guid.NewGuid();
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>());

            // Act
            Exception exception = Assert.ThrowsAsync<Exception>(async () => await _userNoteService.CreateUserNote(new UserAccount(), cabDocumentId, _faker.Random.Word()));

            // Assert
            Assert.AreEqual($"CAB document not found. Document ID {cabDocumentId}. Note: this parameter is the Document.id, not the Document.CABId.", exception.Message);
            _mockCABRepository.Verify(x => x.UpdateAsync(It.IsAny<Document>()), Times.Never);
        }

        [Test]
        public async Task UserNoteService_DeleteUserNote_ShouldDeleteNoteWithoutUpdatingLastUpdateDate()
        {
            // Arrange
            var cabId = Guid.NewGuid();
            var cabDocumentId = Guid.NewGuid();
            var userNoteId = Guid.NewGuid();
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>() {
                    new Document { id = cabDocumentId.ToString(), CABId = cabId.ToString(), URLSlug = "urlSlug",
                        GovernmentUserNotes = new List<UserNote> {
                            new UserNote { Id = Guid.NewGuid(), Note = "Non-matching note" },
                            new UserNote { Id = userNoteId, Note = "Matching note" },
                            new UserNote { Id = Guid.NewGuid(), Note = "Non-matching note" },
                        }
                    },
                });

            // Act
            await _userNoteService.DeleteUserNote(cabDocumentId, userNoteId);

            // Assert
            _mockCABRepository.Verify(x => x.UpdateAsync(It.Is<Document>(x =>
                x.id == cabDocumentId.ToString() &&
                x.GovernmentUserNotes.Count == 2 &&
                x.GovernmentUserNotes.All(y => y.Id != userNoteId))), Times.Once);

            _mockCachedPublishedCAB.Verify(x => x.ClearAsync(cabId.ToString(), "urlSlug"), Times.Once);
        }

        [Test]
        public async Task UserNoteService_DeleteUserNote_ShouldThrowExceptionIfCabDocumentNotFound()
        {
            // Arrange
            var cabDocumentId = Guid.NewGuid();
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>());

            // Act
            Exception exception = Assert.ThrowsAsync<Exception>(async () => await _userNoteService.DeleteUserNote(cabDocumentId, Guid.NewGuid()));

            // Assert
            Assert.AreEqual($"CAB document not found. Document ID {cabDocumentId}. Note: this parameter is the Document.id, not the Document.CABId.", exception.Message);
            _mockCABRepository.Verify(x => x.UpdateAsync(It.IsAny<Document>()), Times.Never);
        }
    }
}
