namespace UKMCAB.Core.Tests.Services.CAB
{
    using AutoMapper;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Threading.Tasks;
    using UKMCAB.Core.Domain.LegislativeAreas;
    using UKMCAB.Core.Mappers;
    using UKMCAB.Core.Services.CAB;
    using UKMCAB.Data.CosmosDb.Services;
    using UKMCAB.Data.CosmosDb.Services.CAB;
    using UKMCAB.Data.Models;
    using UKMCAB.Data.Models.LegislativeAreas;

    [TestFixture]
    public class LegislativeAreasServiceTests
    {
        private Mock<IReadOnlyRepository<LegislativeArea>> _mockLegislativeAreaRepository;
        private Mock<IReadOnlyRepository<PurposeOfAppointment>> _mockPurposeOfAppointmentRepository;
        private Mock<IReadOnlyRepository<Category>> _mockCategoryRepository;
        private Mock<IReadOnlyRepository<Product>> _mockProductRepository;
        private Mock<IReadOnlyRepository<Procedure>> _mockProcedureRepository;

        private ILegislativeAreaService _legislativeAreaService;

        [SetUp]
        public void Setup()
        {
            _mockLegislativeAreaRepository = new Mock<IReadOnlyRepository<LegislativeArea>>();
            _mockPurposeOfAppointmentRepository = new Mock<IReadOnlyRepository<PurposeOfAppointment>>();
            _mockCategoryRepository = new Mock<IReadOnlyRepository<Category>>();
            _mockProductRepository = new Mock<IReadOnlyRepository<Product>>();
            _mockProcedureRepository = new Mock<IReadOnlyRepository<Procedure>>() ;

            var mapper = new MapperConfiguration(mc => { mc.AddProfile(new AutoMapperProfile()); }).CreateMapper();

            _legislativeAreaService = new LegislativeAreaService(_mockLegislativeAreaRepository.Object, _mockPurposeOfAppointmentRepository.Object,
                _mockCategoryRepository.Object, _mockProductRepository.Object, _mockProcedureRepository.Object, mapper);
        }

        #region GetAllLegislativeAreas

        [Test]
        public async Task LegislativeAreaService_GetAllLegislativeAreas_ShouldReturnAllLegislativeAreasFromRepository()
        {
            // Arrange
            _mockLegislativeAreaRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<LegislativeArea>() {
                    new() { Id = new Guid(), Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var legislativeAreas = await _legislativeAreaService.GetAllLegislativeAreasAsync();

            // Assert
            Assert.IsNotNull(legislativeAreas);
            Assert.AreEqual(3, legislativeAreas.Count());
            Assert.AreEqual("Name1", legislativeAreas.ElementAt(0).Name);
            Assert.AreEqual("Name2", legislativeAreas.ElementAt(1).Name);
            Assert.AreEqual("Name3", legislativeAreas.ElementAt(2).Name);
        }

        #endregion
        
        #region GetLegislativeAreaById
        [Test]
        public void EmptyGuid_GetLegislativeAreaById_ShouldThrowException()
        {
            // Arrange & Act & Assert
           Assert.ThrowsAsync<Exception>( () => _legislativeAreaService.GetLegislativeAreaByIdAsync(Guid.Empty));
        }

        [Test]
        public async Task LegislativeAreaService_GetLegislativeAreaById_ShouldReturnLegislativeAreaFromRepository()
        {
            // Arrange
            var testGuid = Guid.NewGuid();
            _mockLegislativeAreaRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<LegislativeArea, bool>>>()))
                .ReturnsAsync(new List<LegislativeArea>
                {
                    new() { Id = testGuid, Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var legislativeArea = await _legislativeAreaService.GetLegislativeAreaByIdAsync(testGuid);

            // Assert
            Assert.IsNotNull(legislativeArea);
            Assert.AreEqual("Name1", legislativeArea!.Name);
        }
        #endregion

        #region GetNextScopeOfAppointmentOptionsForLegislativeArea

        [Test]
        public async Task LegislativeAreaService_GetNextScopeOfAppointmentOptionsForLegislativeArea_ShouldReturnPurposeOfAppointments()
        {
            // Arrange
            _mockPurposeOfAppointmentRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<PurposeOfAppointment, bool>>>()))
                .ReturnsAsync(new List<PurposeOfAppointment>() {
                    new() { Id = new Guid(), Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var nextScopeOptions = await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(Guid.NewGuid());

            // Assert
            Assert.IsNotNull(nextScopeOptions);
            Assert.IsNotNull(nextScopeOptions.PurposeOfAppointments);
            Assert.IsNull(nextScopeOptions.Categories);
            Assert.IsNull(nextScopeOptions.Subcategories);
            Assert.IsNull(nextScopeOptions.Products);
            Assert.IsNull(nextScopeOptions.Procedures);
            Assert.AreEqual(3, nextScopeOptions.PurposeOfAppointments.Count());
            Assert.AreEqual("Name1", nextScopeOptions.PurposeOfAppointments.ElementAt(0).Name);
            Assert.AreEqual("Name2", nextScopeOptions.PurposeOfAppointments.ElementAt(1).Name);
            Assert.AreEqual("Name3", nextScopeOptions.PurposeOfAppointments.ElementAt(2).Name);
        }

        [Test]
        public async Task LegislativeAreaService_GetNextScopeOfAppointmentOptionsForLegislativeArea_ShouldReturnCategories()
        {
            // Arrange
            _mockPurposeOfAppointmentRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<PurposeOfAppointment, bool>>>()))
                .ReturnsAsync(new List<PurposeOfAppointment>());

            _mockCategoryRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(new List<Category>() {
                    new() { Id = new Guid(), Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var nextScopeOptions = await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(Guid.NewGuid());

            // Assert
            Assert.IsNotNull(nextScopeOptions);
            Assert.IsNull(nextScopeOptions.PurposeOfAppointments);
            Assert.IsNotNull(nextScopeOptions.Categories);
            Assert.IsNull(nextScopeOptions.Subcategories);
            Assert.IsNull(nextScopeOptions.Products);
            Assert.IsNull(nextScopeOptions.Procedures);
            Assert.AreEqual(3, nextScopeOptions.Categories.Count());
            Assert.AreEqual("Name1", nextScopeOptions.Categories.ElementAt(0).Name);
            Assert.AreEqual("Name2", nextScopeOptions.Categories.ElementAt(1).Name);
            Assert.AreEqual("Name3", nextScopeOptions.Categories.ElementAt(2).Name);
        }

        [Test]
        public async Task LegislativeAreaService_GetNextScopeOfAppointmentOptionsForLegislativeArea_ShouldReturnProducts()
        {
            // Arrange
            _mockPurposeOfAppointmentRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<PurposeOfAppointment, bool>>>()))
                .ReturnsAsync(new List<PurposeOfAppointment>());

            _mockCategoryRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(new List<Category>());

            _mockProductRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(new List<Product>() {
                    new() { Id = new Guid(), Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var nextScopeOptions = await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(Guid.NewGuid());

            // Assert
            Assert.IsNotNull(nextScopeOptions);
            Assert.IsNull(nextScopeOptions.PurposeOfAppointments);
            Assert.IsNull(nextScopeOptions.Categories);
            Assert.IsNull(nextScopeOptions.Subcategories);
            Assert.IsNotNull(nextScopeOptions.Products);
            Assert.IsNull(nextScopeOptions.Procedures);
            Assert.AreEqual(3, nextScopeOptions.Products.Count());
            Assert.AreEqual("Name1", nextScopeOptions.Products.ElementAt(0).Name);
            Assert.AreEqual("Name2", nextScopeOptions.Products.ElementAt(1).Name);
            Assert.AreEqual("Name3", nextScopeOptions.Products.ElementAt(2).Name);
        }

        [Test]
        public async Task LegislativeAreaService_GetNextScopeOfAppointmentOptionsForLegislativeArea_ShouldReturnProcedures()
        {
            // Arrange
            _mockPurposeOfAppointmentRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<PurposeOfAppointment, bool>>>()))
                .ReturnsAsync(new List<PurposeOfAppointment>());

            _mockCategoryRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(new List<Category>());

            _mockProductRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(new List<Product>());

            _mockProcedureRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Procedure, bool>>>()))
                .ReturnsAsync(new List<Procedure>() {
                    new() { Id = new Guid(), Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var nextScopeOptions = await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(Guid.NewGuid());

            // Assert
            Assert.IsNotNull(nextScopeOptions);
            Assert.IsNull(nextScopeOptions.PurposeOfAppointments);
            Assert.IsNull(nextScopeOptions.Categories);
            Assert.IsNull(nextScopeOptions.Subcategories);
            Assert.IsNull(nextScopeOptions.Products);
            Assert.IsNotNull(nextScopeOptions.Procedures);
            Assert.AreEqual(3, nextScopeOptions.Procedures.Count());
            Assert.AreEqual("Name1", nextScopeOptions.Procedures.ElementAt(0).Name);
            Assert.AreEqual("Name2", nextScopeOptions.Procedures.ElementAt(1).Name);
            Assert.AreEqual("Name3", nextScopeOptions.Procedures.ElementAt(2).Name);
        }

        #endregion

        #region GetNextScopeOfAppointmentOptionsForPurposeOfAppointment

        [Test]
        public async Task LegislativeAreaService_GetNextScopeOfAppointmentOptionsForPurposeOfAppointment_ShouldReturnCategories()
        {
            // Arrange
            _mockCategoryRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(new List<Category>() {
                    new() { Id = new Guid(), Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var nextScopeOptions = await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForPurposeOfAppointmentAsync(Guid.NewGuid());

            // Assert
            Assert.IsNotNull(nextScopeOptions);
            Assert.IsNull(nextScopeOptions.PurposeOfAppointments);
            Assert.IsNotNull(nextScopeOptions.Categories);
            Assert.IsNull(nextScopeOptions.Subcategories);
            Assert.IsNull(nextScopeOptions.Products);
            Assert.IsNull(nextScopeOptions.Procedures);
            Assert.AreEqual(3, nextScopeOptions.Categories.Count());
            Assert.AreEqual("Name1", nextScopeOptions.Categories.ElementAt(0).Name);
            Assert.AreEqual("Name2", nextScopeOptions.Categories.ElementAt(1).Name);
            Assert.AreEqual("Name3", nextScopeOptions.Categories.ElementAt(2).Name);
        }

        [Test]
        public async Task LegislativeAreaService_GetNextScopeOfAppointmentOptionsForPurposeOfAppointment_ShouldRemoveDuplicateCategories()
        {
            // Arrange - Category names are duplicated when it has subcategories available.
            _mockCategoryRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(new List<Category>() {
                    new() { Id = new Guid(), Name = "Name1", Subcategory = "" },
                    new() { Id = new Guid(), Name = "Name2", Subcategory = "Sub1" },
                    new() { Id = new Guid(), Name = "Name2", Subcategory = "Sub2" },
                    new() { Id = new Guid(), Name = "Name3", Subcategory = "Sub3" },
                    new() { Id = new Guid(), Name = "Name3", Subcategory = "Sub4" },
                    new() { Id = new Guid(), Name = "Name3", Subcategory = "Sub5" },
                });

            // Act
            var nextScopeOptions = await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForPurposeOfAppointmentAsync(Guid.NewGuid());

            // Assert
            Assert.IsNotNull(nextScopeOptions);
            Assert.IsNull(nextScopeOptions.PurposeOfAppointments);
            Assert.IsNotNull(nextScopeOptions.Categories);
            Assert.IsNull(nextScopeOptions.Subcategories);
            Assert.IsNull(nextScopeOptions.Products);
            Assert.IsNull(nextScopeOptions.Procedures);
            Assert.AreEqual(3, nextScopeOptions.Categories.Count());
            Assert.AreEqual("Name1", nextScopeOptions.Categories.ElementAt(0).Name);
            Assert.AreEqual("Name2", nextScopeOptions.Categories.ElementAt(1).Name);
            Assert.AreEqual("Name3", nextScopeOptions.Categories.ElementAt(2).Name);
        }

        [Test]
        public async Task LegislativeAreaService_GetNextScopeOfAppointmentOptionsForPurposeOfAppointment_ShouldReturnProducts()
        {
            // Arrange
            _mockCategoryRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(new List<Category>());

            _mockProductRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(new List<Product>() {
                    new() { Id = new Guid(), Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var nextScopeOptions = await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForPurposeOfAppointmentAsync(Guid.NewGuid());

            // Assert
            Assert.IsNotNull(nextScopeOptions);
            Assert.IsNull(nextScopeOptions.PurposeOfAppointments);
            Assert.IsNull(nextScopeOptions.Categories);
            Assert.IsNull(nextScopeOptions.Subcategories);
            Assert.IsNotNull(nextScopeOptions.Products);
            Assert.IsNull(nextScopeOptions.Procedures);
            Assert.AreEqual(3, nextScopeOptions.Products.Count());
            Assert.AreEqual("Name1", nextScopeOptions.Products.ElementAt(0).Name);
            Assert.AreEqual("Name2", nextScopeOptions.Products.ElementAt(1).Name);
            Assert.AreEqual("Name3", nextScopeOptions.Products.ElementAt(2).Name);
        }

        [Test]
        public async Task LegislativeAreaService_GetNextScopeOfAppointmentOptionsForPurposeOfAppointment_ShouldReturnProcedures()
        {
            // Arrange
            _mockCategoryRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(new List<Category>());

            _mockProductRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(new List<Product>());

            _mockProcedureRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Procedure, bool>>>()))
                .ReturnsAsync(new List<Procedure>() {
                    new() { Id = new Guid(), Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var nextScopeOptions = await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForPurposeOfAppointmentAsync(Guid.NewGuid());

            // Assert
            Assert.IsNotNull(nextScopeOptions);
            Assert.IsNull(nextScopeOptions.PurposeOfAppointments);
            Assert.IsNull(nextScopeOptions.Categories);
            Assert.IsNull(nextScopeOptions.Subcategories);
            Assert.IsNull(nextScopeOptions.Products);
            Assert.IsNotNull(nextScopeOptions.Procedures);
            Assert.AreEqual(3, nextScopeOptions.Procedures.Count());
            Assert.AreEqual("Name1", nextScopeOptions.Procedures.ElementAt(0).Name);
            Assert.AreEqual("Name2", nextScopeOptions.Procedures.ElementAt(1).Name);
            Assert.AreEqual("Name3", nextScopeOptions.Procedures.ElementAt(2).Name);
        }

        #endregion

        #region GetNextScopeOfAppointmentOptionsForCategory

        [Test]
        public async Task LegislativeAreaService_GetNextScopeOfAppointmentOptionsForCategory_ShouldReturnSubcategories()
        {
            // Arrange
            _mockCategoryRepository.SetupSequence(x => x.QueryAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(new List<Category>() {
                    new() { Id = new Guid(), Name = "Name1", Subcategory = "Sub1" },
                })
                .ReturnsAsync(new List<Category>() {
                    new() { Id = new Guid(), Name = "Name1", Subcategory = "Sub1" },
                    new() { Id = new Guid(), Name = "Name1", Subcategory = "Sub2" },
                    new() { Id = new Guid(), Name = "Name1", Subcategory = "Sub3" },
                });

            // Act
            var nextScopeOptions = await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForCategoryAsync(Guid.NewGuid());

            // Assert
            Assert.IsNotNull(nextScopeOptions);
            Assert.IsNull(nextScopeOptions.PurposeOfAppointments);
            Assert.IsNull(nextScopeOptions.Categories);
            Assert.IsNotNull(nextScopeOptions.Subcategories);
            Assert.IsNull(nextScopeOptions.Products);
            Assert.IsNull(nextScopeOptions.Procedures);
            Assert.AreEqual(3, nextScopeOptions.Subcategories.Count());
            Assert.AreEqual("Sub1", nextScopeOptions.Subcategories.ElementAt(0).Name);
            Assert.AreEqual("Sub2", nextScopeOptions.Subcategories.ElementAt(1).Name);
            Assert.AreEqual("Sub3", nextScopeOptions.Subcategories.ElementAt(2).Name);
        }

        [Test]
        public async Task LegislativeAreaService_GetNextScopeOfAppointmentOptionsForCategory_ShouldReturnProducts()
        {
            // Arrange - Note the empty Subcategory.
            _mockCategoryRepository.SetupSequence(x => x.QueryAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(new List<Category>() {
                    new() { Id = new Guid(), Name = "Name1", Subcategory = "" },
                });

            _mockProductRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(new List<Product>() {
                    new() { Id = new Guid(), Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var nextScopeOptions = await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForCategoryAsync(Guid.NewGuid());

            // Assert
            Assert.IsNotNull(nextScopeOptions);
            Assert.IsNull(nextScopeOptions.PurposeOfAppointments);
            Assert.IsNull(nextScopeOptions.Categories);
            Assert.IsNull(nextScopeOptions.Subcategories);
            Assert.IsNotNull(nextScopeOptions.Products);
            Assert.IsNull(nextScopeOptions.Procedures);
            Assert.AreEqual(3, nextScopeOptions.Products.Count());
            Assert.AreEqual("Name1", nextScopeOptions.Products.ElementAt(0).Name);
            Assert.AreEqual("Name2", nextScopeOptions.Products.ElementAt(1).Name);
            Assert.AreEqual("Name3", nextScopeOptions.Products.ElementAt(2).Name);
        }

        [Test]
        public async Task LegislativeAreaService_GetNextScopeOfAppointmentOptionsForCategory_ShouldReturnProcedures()
        {
            // Arrange - Note the empty Subcategory.
            _mockCategoryRepository.SetupSequence(x => x.QueryAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(new List<Category>() {
                    new() { Id = new Guid(), Name = "Name1", Subcategory = "" },
                });

            _mockProductRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(new List<Product>());

            _mockProcedureRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Procedure, bool>>>()))
                .ReturnsAsync(new List<Procedure>() {
                    new() { Id = new Guid(), Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var nextScopeOptions = await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForCategoryAsync(Guid.NewGuid());

            // Assert
            Assert.IsNotNull(nextScopeOptions);
            Assert.IsNull(nextScopeOptions.PurposeOfAppointments);
            Assert.IsNull(nextScopeOptions.Categories);
            Assert.IsNull(nextScopeOptions.Subcategories);
            Assert.IsNull(nextScopeOptions.Products);
            Assert.IsNotNull(nextScopeOptions.Procedures);
            Assert.AreEqual(3, nextScopeOptions.Procedures.Count());
            Assert.AreEqual("Name1", nextScopeOptions.Procedures.ElementAt(0).Name);
            Assert.AreEqual("Name2", nextScopeOptions.Procedures.ElementAt(1).Name);
            Assert.AreEqual("Name3", nextScopeOptions.Procedures.ElementAt(2).Name);
        }

        #endregion

        #region GetNextScopeOfAppointmentOptionsForSubcategory

        [Test]
        public async Task LegislativeAreaService_GetNextScopeOfAppointmentOptionsForSubcategory_ShouldReturnProducts()
        {
            // Arrange
            _mockCategoryRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(new List<Category>());

            _mockProductRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(new List<Product>() {
                    new() { Id = new Guid(), Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var nextScopeOptions = await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForSubcategoryAsync(Guid.NewGuid());

            // Assert
            Assert.IsNotNull(nextScopeOptions);
            Assert.IsNull(nextScopeOptions.PurposeOfAppointments);
            Assert.IsNull(nextScopeOptions.Categories);
            Assert.IsNull(nextScopeOptions.Subcategories);
            Assert.IsNotNull(nextScopeOptions.Products);
            Assert.IsNull(nextScopeOptions.Procedures);
            Assert.AreEqual(3, nextScopeOptions.Products.Count());
            Assert.AreEqual("Name1", nextScopeOptions.Products.ElementAt(0).Name);
            Assert.AreEqual("Name2", nextScopeOptions.Products.ElementAt(1).Name);
            Assert.AreEqual("Name3", nextScopeOptions.Products.ElementAt(2).Name);
        }

        [Test]
        public async Task LegislativeAreaService_GetNextScopeOfAppointmentOptionsForSubcategory_ShouldReturnProcedures()
        {
            // Arrange
            _mockCategoryRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(new List<Category>());

            _mockProductRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(new List<Product>());

            _mockProcedureRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Procedure, bool>>>()))
                .ReturnsAsync(new List<Procedure>() {
                    new() { Id = new Guid(), Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var nextScopeOptions = await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForSubcategoryAsync(Guid.NewGuid());

            // Assert
            Assert.IsNotNull(nextScopeOptions);
            Assert.IsNull(nextScopeOptions.PurposeOfAppointments);
            Assert.IsNull(nextScopeOptions.Categories);
            Assert.IsNull(nextScopeOptions.Subcategories);
            Assert.IsNull(nextScopeOptions.Products);
            Assert.IsNotNull(nextScopeOptions.Procedures);
            Assert.AreEqual(3, nextScopeOptions.Procedures.Count());
            Assert.AreEqual("Name1", nextScopeOptions.Procedures.ElementAt(0).Name);
            Assert.AreEqual("Name2", nextScopeOptions.Procedures.ElementAt(1).Name);
            Assert.AreEqual("Name3", nextScopeOptions.Procedures.ElementAt(2).Name);
        }

        #endregion

        #region GetNextScopeOfAppointmentOptionsForProduct

        [Test]
        public async Task LegislativeAreaService_GetNextScopeOfAppointmentOptionsForProduct_ShouldReturnProcedures()
        {
            // Arrange
            _mockProcedureRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Procedure, bool>>>()))
                .ReturnsAsync(new List<Procedure>() {
                    new() { Id = new Guid(), Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var nextScopeOptions = await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForProductAsync(Guid.NewGuid());

            // Assert
            Assert.IsNotNull(nextScopeOptions);
            Assert.IsNull(nextScopeOptions.PurposeOfAppointments);
            Assert.IsNull(nextScopeOptions.Categories);
            Assert.IsNull(nextScopeOptions.Subcategories);
            Assert.IsNull(nextScopeOptions.Products);
            Assert.IsNotNull(nextScopeOptions.Procedures);
            Assert.AreEqual(3, nextScopeOptions.Procedures.Count());
            Assert.AreEqual("Name1", nextScopeOptions.Procedures.ElementAt(0).Name);
            Assert.AreEqual("Name2", nextScopeOptions.Procedures.ElementAt(1).Name);
            Assert.AreEqual("Name3", nextScopeOptions.Procedures.ElementAt(2).Name);
        }

        #endregion


        #region GetPurposeOfAppointmentById
        [Test]
        public void EmptyGuid_GetPurposeOfAppointmentById_ShouldThrowException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsAsync<Exception>(() => _legislativeAreaService.GetPurposeOfAppointmentByIdAsync(Guid.Empty));
        }

        [Test]
        public async Task LegislativeAreaService_GetPurposeOfAppointmentById_ShouldReturnPurposeOfAppointmentFromRepository()
        {
            // Arrange
            var testGuid = Guid.NewGuid();
            _mockPurposeOfAppointmentRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<PurposeOfAppointment, bool>>>()))
                .ReturnsAsync(new List<PurposeOfAppointment>
                {
                    new() { Id = testGuid, Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var purposeOfAppointment = await _legislativeAreaService.GetPurposeOfAppointmentByIdAsync(testGuid);

            // Assert
            Assert.IsNotNull(purposeOfAppointment);
            Assert.AreEqual("Name1", purposeOfAppointment!.Name);
        }
        #endregion

        #region GetCategoryById
        [Test]
        public void EmptyGuid_GetCategoryById_ShouldThrowException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsAsync<Exception>(() => _legislativeAreaService.GetCategoryByIdAsync(Guid.Empty));
        }

        [Test]
        public async Task LegislativeAreaService_GetCategoryById_ShouldReturnCategoryFromRepository()
        {
            // Arrange
            var testGuid = Guid.NewGuid();
            _mockCategoryRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(new List<Category>
                {
                    new() { Id = testGuid, Name = "Name1", LegislativeAreaId = new Guid(),  PurposeOfAppointmentId = new Guid()},
                    new() { Id = new Guid(), Name = "Name2", LegislativeAreaId = new Guid(),  PurposeOfAppointmentId = new Guid() },
                    new() { Id = new Guid(), Name = "Name3", LegislativeAreaId = new Guid(),  PurposeOfAppointmentId =new Guid() },
                });

            // Act
            var category = await _legislativeAreaService.GetPurposeOfAppointmentByIdAsync(testGuid);

            // Assert
            Assert.IsNotNull(category);
            Assert.AreEqual("Name1", category!.Name);
        }
        #endregion
    }
}
