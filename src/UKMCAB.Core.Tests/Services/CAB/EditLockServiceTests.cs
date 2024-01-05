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
    public async Task CabNotFound_LockExistsForCabAsync_ReturnsNull()
    {
        _distCache.Setup(c => c.GetAsync<Dictionary<string, string>>(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new Dictionary<string, string>());

        Assert.IsNull(await _sut.LockExistsForCabAsync(_faker.Random.Word()));
    }

    [Test]
    public async Task CabFound_LockExistsForCabAsync_ReturnsUserId()
    {
        var testCabId = _faker.Random.Word();
        var testUserId = _faker.Random.Word();

        _distCache.Setup(c => c.GetAsync<Dictionary<string, string>>(It.IsAny<string>(), -1))
            .ReturnsAsync(new Dictionary<string, string>()
            {
                { testCabId, testUserId }
            });

        Assert.AreEqual(testUserId, await _sut.LockExistsForCabAsync(testCabId));
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
        var testCabId = _faker.Random.Word();
        var testUserId = _faker.Random.Word();
        await _sut.SetAsync(testCabId, testUserId);
        await _sut.RemoveEditLockForUserAsync(testUserId);
        _distCache.Verify(
            c => c.SetAsync(It.IsAny<string>(), It.Is<Dictionary<string, string>>(d => d.ContainsKey(testCabId)),
                TimeSpan.FromHours(1), -1),
            Times.Never);
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
        var testCabId = _faker.Random.Word();
        var testUserId = _faker.Random.Word();
        await _sut.SetAsync(testCabId, testUserId);
        await _sut.RemoveEditLockForCabAsync(testCabId);
        _distCache.Verify(
            c => c.SetAsync(It.IsAny<string>(), It.Is<Dictionary<string, string>>(d => !d.ContainsValue(testCabId)),
                TimeSpan.FromHours(1), -1),
            Times.Once);
    }
}