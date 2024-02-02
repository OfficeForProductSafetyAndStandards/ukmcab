using Bogus;
using Microsoft.Azure.Cosmos;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UKMCAB.Data.CosmosDb.Services;
using UKMCAB.Data.CosmosDb.Utilities;
using UKMCAB.Data.Models.LegislativeAreas;

namespace UKMCAB.Data.Tests.Services;

[TestFixture]
public class ReadOnlyRepositoryTests
{
    private IReadOnlyRepository<Product> _repository;

    private Mock<CosmosClient> _mockCosmosClient;
    private Mock<ICosmosFeedIterator> _mockCosmosFeedIterator;
    private Mock<Container> _mockContainer;
    private readonly Faker _faker = new();

    [SetUp]
    public void Setup()
    {
        _mockCosmosClient = new Mock<CosmosClient>();
        _mockCosmosFeedIterator = new Mock<ICosmosFeedIterator>();
        _mockContainer = new Mock<Container>();

        _mockCosmosClient.Setup(x => x.GetContainer(It.IsAny<string>(), "products"))
            .Returns(_mockContainer.Object);

        _repository = new ReadOnlyRepository<Product>(_mockCosmosClient.Object, _mockCosmosFeedIterator.Object, "products");
    }

    [Test]
    public async Task ReadOnlyRepository_GetAllAsync_ShouldReturnAllEntitiesFromCosmosContainer()
    {
        // Arrange

        // Set up the FeedResponse to be returned by the FeedIterator.ReadNextAsync method - i.e. a single product.
        List<Product> resultOfReadNextAsync = new List<Product>()
        {
            new Product { Id = Guid.NewGuid(), Name = "product" },
        };
        Mock<FeedResponse<Product>> mockResponse = new Mock<FeedResponse<Product>>();
        mockResponse.Setup(r => r.Resource).Returns(resultOfReadNextAsync);

        // These lines of code will replicate two products being returned from the container.
        Mock<FeedIterator<Product>> mockIterator = new Mock<FeedIterator<Product>>();
        mockIterator.Setup(q => q.ReadNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => mockResponse.Object);
        mockIterator.SetupSequence(q => q.HasMoreResults)
            .Returns(true)
            .Returns(true)
            .Returns(false);

        _mockContainer.Setup(x => x.GetItemQueryIterator<Product>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
            .Returns(mockIterator.Object);

        // Act
        var products = await _repository.GetAllAsync();

        // Assert
        // The above mocks will result in a list of 2 products being returned.
        mockIterator.Verify(q => q.ReadNextAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        mockIterator.Verify(q => q.HasMoreResults, Times.Exactly(3));
        Assert.AreEqual(2, products.Count());
    }

    [Test]
    public async Task ReadOnlyRepository_QueryAsync_ShouldPassQueryThroughToCosmosContainer()
    {
        // Arrange

        // Cosmos Container.GetItemLinqQueryable method is horrific to unit test! ToFeedIterator() cannot be called on it in a unit test as
        // it throws an error if the query is not a CosmosLinqQuery. The only workaround is to wrap that call in a service which can be
        // mocked. Hence the creation of the ICosmosFeedIterator interface. You also need to mock a whole load of other classes, as below.

        // Set up a container to return an empty IOrderedList. Doesn't matter that it's empty as the FeedIterator.ReadNextAsync will be mocked next.
        _mockContainer.Setup(x => x.GetItemLinqQueryable<Product>(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>(), It.IsAny<CosmosLinqSerializerOptions>()))
            .Returns(new List<Product>().AsQueryable().OrderBy(x => x.Id));

        // Set up the FeedResponse to be returned by the FeedIterator.ReadNextAsync method - i.e. a single product.
        List< Product> resultOfReadNextAsync = new List<Product>()
        {
            new Product { Id = Guid.NewGuid(), Name = "product" },
        };
        Mock<FeedResponse<Product>> mockResponse = new Mock<FeedResponse<Product>>();
        mockResponse.Setup(r => r.Resource).Returns(resultOfReadNextAsync);

        // Set up a mock of the Cosmos "FeedIterator", which we want to return from our "ICosmosFeedIterator" instance.
        // These lines of code will replicate two products matching our query.
        Mock<FeedIterator<Product>> mockIterator = new Mock<FeedIterator<Product>>();
        mockIterator.Setup(q => q.ReadNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => mockResponse.Object);
        mockIterator.SetupSequence(q => q.HasMoreResults)
            .Returns(true)
            .Returns(true)
            .Returns(false);

        _mockCosmosFeedIterator.Setup(x => x.GetFeedIterator<Product>(It.IsAny<IQueryable<Product>>()))
            .Returns(mockIterator.Object);

        // Act
        var products = await _repository.QueryAsync(p => true);

        // Assert
        // The above mocks will result in a list of 2 products being returned.
        mockIterator.Verify(q => q.ReadNextAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        mockIterator.Verify(q => q.HasMoreResults, Times.Exactly(3));
        Assert.AreEqual(2, products.Count());
    }

    [Test]
    public async Task ReadOnlyRepository_GetAsync_ShouldPassIdThroughToCosmosContainer()
    {
        // Arrange
        var productId = Guid.NewGuid();

        // Act
        var returnedProduct = _repository.GetAsync(productId.ToString());

        // Assert
        _mockCosmosClient.Verify(x => x.GetContainer(It.IsAny<string>(), "products"), Times.Once);
        _mockContainer.Verify(x => x.ReadItemAsync<Product>(productId.ToString(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
