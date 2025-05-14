using Bogus;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UKMCAB.Data.Interfaces.Services;
using UKMCAB.Data.Models.LegislativeAreas;
using UKMCAB.Data.PostgreSQL;
using UKMCAB.Data.PostgreSQL.Services;

namespace UKMCAB.Data.Tests.Services;

[TestFixture]
public class ReadOnlyRepositoryTests
{
    private Mock<ApplicationDataContext> _mocDbContext;
    private IReadOnlyRepository<Product> _repository;

    private readonly Faker _faker = new();

    [SetUp]
    public void Setup()
    {
        _mocDbContext = new Mock<ApplicationDataContext>();

        _repository = new PostgreReadOnlyRepository<Product>(_mocDbContext.Object);
    }

    [Test]
    public async Task ReadOnlyRepository_GetAllAsync_ShouldReturnAllEntitiesFromCosmosContainer()
    {
        // Arrange

        // Set up the FeedResponse to be returned by the FeedIterator.ReadNextAsync method - i.e. a single product.
        var dbProducts = new List<Product>()
        {
            new Product { Id = Guid.NewGuid(), Name = "product" },
        }.AsQueryable();
        var mockSet = new Mock<DbSet<Product>>();
        mockSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(dbProducts.Provider);
        mockSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(dbProducts.Expression);
        mockSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(dbProducts.ElementType);
        mockSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(dbProducts.GetEnumerator());

        _mocDbContext.Setup(c => c.Set<Product>()).Returns(mockSet.Object);
        _mocDbContext.Setup(c => c.Products).Returns(mockSet.Object);

        // Act
        var products = await _repository.GetAllAsync();

        // Assert
        // The above mocks will result in a list of 2 products being returned.
        Assert.That(1, Is.EqualTo(products.Count()));
    }

}
