using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bogus;
using Moq;
using NUnit.Framework;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Infrastructure.Cache;

namespace UKMCAB.Core.Tests.Services.CAB;

[TestFixture]
public class EditLockServiceTests
{
    private readonly Mock<IDistCache> _distCache = new();
    private IEditLockService _sut = null!;
    private readonly Faker _faker = new();

    [SetUp]
    public void Setup()
    {
        _sut = new EditLockService(_distCache.Object);
    }

    [Test]
    public async Task IsCabLockedForUser_LockExistsAndDoesNotMatchUser_ReturnsTrue()
    {
        var cabId = _faker.Random.Word();
        var userIdWithLock = _faker.Random.Word();
        var userId = _faker.Random.Word();

        _distCache.Setup(c => c.GetAsync<Dictionary<string, Tuple<string, DateTime>>>(It.IsAny<string>(), -1))
            .ReturnsAsync(new Dictionary<string, Tuple<string, DateTime>>
            {
                { cabId, new Tuple<string, DateTime>(userIdWithLock, DateTime.Now.AddMinutes(10)) }
            });

        Assert.True(await _sut.IsCabLockedForUser(cabId, userId));
    }

    [Test]
    public async Task IsCabLockedForUser_LockNotFound_ReturnsFalse()
    {
        var cabId = _faker.Random.Word();
        var userId = _faker.Random.Word();

        Assert.False(await _sut.IsCabLockedForUser(cabId, userId));
    }

    [Test]
    public async Task IsCabLockedForUser_LockExistsAndMatchesUser_ReturnsFalse()
    {
        var cabId = _faker.Random.Word();
        var userId = _faker.Random.Word();

        _distCache.Setup(c => c.GetAsync<Dictionary<string, Tuple<string, DateTime>>>(It.IsAny<string>(), -1))
            .ReturnsAsync(new Dictionary<string, Tuple<string, DateTime>>
            {
                { cabId, new Tuple<string, DateTime>(userId, DateTime.Now.AddMinutes(10)) }
            });

        Assert.False(await _sut.IsCabLockedForUser(cabId, userId));
    }

    [Test]
    public async Task IsCabLockedForUser_MultipleCabsLockExistsAndDoesNotMatchesUser_ReturnsTrue()
    {
        // Arrange
        var cabId = _faker.Random.Word();
        var cabId2 = _faker.Random.Word();
        var userIdWithLock = _faker.Random.Word();
        var userIdWithLock2 = _faker.Random.Word();

        _distCache.Setup(c => c.GetAsync<Dictionary<string, Tuple<string, DateTime>>>(It.IsAny<string>(), -1))
            .ReturnsAsync(new Dictionary<string, Tuple<string, DateTime>>
            {
                { cabId, new Tuple<string, DateTime>(userIdWithLock, DateTime.Now.AddMinutes(10)) },
                { cabId2, new Tuple<string, DateTime>(userIdWithLock2, DateTime.Now.AddMinutes(30)) }
            });

        // Act
        var userFound = await _sut.IsCabLockedForUser(cabId2, userIdWithLock);

        // Assert
        Assert.True(userFound);
    }

    [Test]
    public async Task IsCabLockedForUser_MultipleCabsLockExistsAndMatchesUser_ReturnsFalse()
    {
        // Arrange
        var cabId = _faker.Random.Word();
        var cabId2 = _faker.Random.Word();
        var userIdWithLock = _faker.Random.Word();
        var userIdWithLock2 = _faker.Random.Word();

        _distCache.Setup(c => c.GetAsync<Dictionary<string, Tuple<string, DateTime>>>(It.IsAny<string>(), -1))
            .ReturnsAsync(new Dictionary<string, Tuple<string, DateTime>>
            {
                { cabId, new Tuple<string, DateTime>(userIdWithLock, DateTime.Now.AddMinutes(10)) },
                { cabId2, new Tuple<string, DateTime>(userIdWithLock2, DateTime.Now.AddMinutes(30)) }
            });

        // Act
        var userFound = await _sut.IsCabLockedForUser(cabId2, userIdWithLock2);

        // Assert
        Assert.False(userFound);
    }

    [Test]
    public async Task UserNotFound_RemoveEditLockForUserAsync_CacheNotSet()
    {
        _distCache.Invocations.Clear();
        var userId = _faker.Random.Word();
        await _sut.RemoveEditLockForUserAsync(userId);
        _distCache.Verify(
            c => c.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan?>(), It.IsAny<int>()),
            Times.Never);
    }

    [Test]
    public async Task UserFound_RemoveEditLockForUserAsync_CacheSetWithoutKey()
    {
        var testCabIds = _faker.Make(3, () => _faker.Random.Word());
        var testUserId = _faker.Random.Word();
        foreach (var id in testCabIds)
        {
            await _sut.SetAsync(id, testUserId);
        }

        await _sut.RemoveEditLockForUserAsync(testUserId);
        foreach (var id in testCabIds)
        {
            _distCache.Verify(
                c => c.SetAsync(It.IsAny<string>(), It.Is<Dictionary<string, string>>(d => d.ContainsKey(id)),
                    TimeSpan.FromHours(1), -1),
                Times.Never);
        }
    }

    [Test]
    public async Task CabNotFound_RemoveEditLockForCabAsync_CacheNotSet()
    {
        _distCache.Invocations.Clear();
        var cabId = _faker.Random.Word();
        await _sut.RemoveEditLockForCabAsync(cabId);
        _distCache.Verify(
            c => c.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan?>(), It.IsAny<int>()),
            Times.Never);
    }

    [Test]
    public async Task CabFound_RemoveEditLockForCabAsync_CacheSetWithoutKey()
    {
        // Arrange
        var testCabId = _faker.Random.Word();
        var testUserId = _faker.Random.Word();
        await _sut.SetAsync(testCabId, testUserId);
        _distCache.Invocations.Clear();
        
        // Act
        await _sut.RemoveEditLockForCabAsync(testCabId);
        
        // Assert
        _distCache.Verify(
            c => c.SetAsync("CabEditLock", It.Is<Dictionary<string, Tuple<string, DateTime>>>(d => d.Count == 0),
                TimeSpan.FromHours(1), -1),
            Times.Once);
    }
}