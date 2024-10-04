namespace UKMCAB.Core.Tests.Services.CAB
{
    using AutoMapper;
    using Moq;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Mappers;
    using UKMCAB.Core.Services.CAB;
    using UKMCAB.Data.CosmosDb.Services;
    using Data.Models.LegislativeAreas;

    [TestFixture]
    public class LegislativeAreasServiceTests
    {
        private Mock<IReadOnlyRepository<LegislativeArea>> _mockLegislativeAreaRepository;
        private Mock<IReadOnlyRepository<PurposeOfAppointment>> _mockPurposeOfAppointmentRepository;
        private Mock<IReadOnlyRepository<Category>> _mockCategoryRepository;
        private Mock<IReadOnlyRepository<SubCategory>> _mockSubCategoryRepository;
        private Mock<IReadOnlyRepository<Product>> _mockProductRepository;
        private Mock<IReadOnlyRepository<Procedure>> _mockProcedureRepository;
        private Mock<IReadOnlyRepository<DesignatedStandard>> _mockDesignatedStandardRepository;

        private ILegislativeAreaService _legislativeAreaService;

        [SetUp]
        public void Setup()
        {
            _mockLegislativeAreaRepository = new Mock<IReadOnlyRepository<LegislativeArea>>();
            _mockPurposeOfAppointmentRepository = new Mock<IReadOnlyRepository<PurposeOfAppointment>>();
            _mockCategoryRepository = new Mock<IReadOnlyRepository<Category>>();
            _mockSubCategoryRepository = new Mock<IReadOnlyRepository<SubCategory>>();
            _mockProductRepository = new Mock<IReadOnlyRepository<Product>>();
            _mockProcedureRepository = new Mock<IReadOnlyRepository<Procedure>>();
            _mockDesignatedStandardRepository = new Mock<IReadOnlyRepository<DesignatedStandard>>(MockBehavior.Strict);

            var mapper = new MapperConfiguration(mc => { mc.AddProfile(new AutoMapperProfile()); }).CreateMapper();

            _legislativeAreaService = new LegislativeAreaService(_mockLegislativeAreaRepository.Object,
                _mockPurposeOfAppointmentRepository.Object,
                _mockCategoryRepository.Object, _mockProductRepository.Object, _mockProcedureRepository.Object,
                _mockSubCategoryRepository.Object, _mockDesignatedStandardRepository.Object, mapper);
        }

        #region GetAllLegislativeAreas

        [Test]
        public async Task LegislativeAreaService_GetAllLegislativeAreas_ShouldReturnAllLegislativeAreasFromRepository()
        {
            // Arrange
            _mockLegislativeAreaRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<LegislativeArea>()
                {
                    new() { Id = new Guid(), Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var legislativeAreas = await _legislativeAreaService.GetAllLegislativeAreasAsync();

            // ClassicAssert
            ClassicAssert.IsNotNull(legislativeAreas);
            ClassicAssert.AreEqual(3, legislativeAreas.Count());
            ClassicAssert.AreEqual("Name1", legislativeAreas.ElementAt(0).Name);
            ClassicAssert.AreEqual("Name2", legislativeAreas.ElementAt(1).Name);
            ClassicAssert.AreEqual("Name3", legislativeAreas.ElementAt(2).Name);
        }

        #endregion

        #region GetAvailableCabLegislativeAreas

        [Test]
        public async Task
            LegislativeAreaService_GetLegislativeAreas_ShouldNotReturnExcludedLegislativeAreasFromRepository()
        {
            // Arrange

            var cabSelectedLegislativeId1 = Guid.NewGuid();
            var cabSelectedLegislativeId2 = Guid.NewGuid();

            List<Guid> excludeLegislativeAreaIds = new List<Guid>()
                { cabSelectedLegislativeId1, cabSelectedLegislativeId2 };

            _mockLegislativeAreaRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<LegislativeArea>
                {
                    new() { Id = cabSelectedLegislativeId1, Name = "Name1" },
                    new() { Id = cabSelectedLegislativeId2, Name = "Name2" },
                    new() { Id = Guid.NewGuid(), Name = "Name3" },
                    new() { Id = Guid.NewGuid(), Name = "Name4" },
                });

            // Act
            var availableLegislativeAreas =
                await _legislativeAreaService.GetLegislativeAreasAsync(excludeLegislativeAreaIds);

            // ClassicAssert
            ClassicAssert.IsNotNull(availableLegislativeAreas);
            ClassicAssert.AreEqual(2, availableLegislativeAreas.Count());
            ClassicAssert.AreEqual("Name3", availableLegislativeAreas.ElementAt(0).Name);
            ClassicAssert.AreEqual("Name4", availableLegislativeAreas.ElementAt(1).Name);
        }

        #endregion

        #region GetLegislativeAreaById

        [Test]
        public void EmptyGuid_GetLegislativeAreaById_ShouldThrowException()
        {
            // Arrange & Act & ClassicAssert
            ClassicAssert.ThrowsAsync<Exception>(() => _legislativeAreaService.GetLegislativeAreaByIdAsync(Guid.Empty));
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

            // ClassicAssert
            ClassicAssert.IsNotNull(legislativeArea);
            ClassicAssert.AreEqual("Name1", legislativeArea!.Name);
        }

        #endregion
        
        #region GetLegislativeAreasByRoleId
        [Test]
        public void EmptyRoleId_GetLegislativeAreaByRoleId_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & ClassicAssert
            ClassicAssert.ThrowsAsync<ArgumentNullException>(() => _legislativeAreaService.GetLegislativeAreasByRoleId(string.Empty));
        }
        
        [Test]
        public async Task LegislativeAreasFound_GetLegislativeAreaByRoleId_ShouldReturnLegislativeAreaFromRepository()
        {
            // Arrange
            var testRoleId = "ogdRole";
            _mockLegislativeAreaRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<LegislativeArea, bool>>>()))
                .ReturnsAsync(new List<LegislativeArea>
                {
                    new() { Id = new Guid(), Name = "Name1", RoleId = testRoleId},
                    new() { Id = new Guid(), Name = "Name2", RoleId = "ogd"},
                    new() { Id = new Guid(), Name = "Name3", RoleId = "opss"},
                });

            // Act
            var legislativeAreas = (await _legislativeAreaService.GetLegislativeAreasByRoleId(testRoleId)).ToList();

            // ClassicAssert
            ClassicAssert.IsNotEmpty(legislativeAreas);
            ClassicAssert.AreEqual(testRoleId, legislativeAreas.First().RoleId);
        }
        
        [Test]
        public async Task NoLegislativeAreasFound_GetLegislativeAreaByRoleId_ShouldReturnEmptyList()
        {
            // Arrange
            var testRoleId = "ogdRole";
            _mockLegislativeAreaRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<LegislativeArea, bool>>>()))
                .ReturnsAsync(new List<LegislativeArea>());

            // Act
            var legislativeAreas = (await _legislativeAreaService.GetLegislativeAreasByRoleId(testRoleId)).ToList();

            // ClassicAssert
            ClassicAssert.IsEmpty(legislativeAreas);
        }
        #endregion

        #region GetNextScopeOfAppointmentOptionsForLegislativeArea

        [Test]
        public async Task
            LegislativeAreaService_GetNextScopeOfAppointmentOptionsForLegislativeArea_ShouldReturnPurposeOfAppointments()
        {
            // Arrange
            _mockPurposeOfAppointmentRepository
                .Setup(x => x.QueryAsync(It.IsAny<Expression<Func<PurposeOfAppointment, bool>>>()))
                .ReturnsAsync(new List<PurposeOfAppointment>()
                {
                    new() { Id = new Guid(), Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var nextScopeOptions =
                await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(Guid.NewGuid());

            // ClassicAssert
            ClassicAssert.IsNotNull(nextScopeOptions);
            ClassicAssert.IsNotNull(nextScopeOptions.PurposeOfAppointments);
            ClassicAssert.IsEmpty(nextScopeOptions.Categories);
            ClassicAssert.IsEmpty(nextScopeOptions.Subcategories);
            ClassicAssert.IsEmpty(nextScopeOptions.Products);
            ClassicAssert.IsEmpty(nextScopeOptions.Procedures);
            ClassicAssert.AreEqual(3, nextScopeOptions.PurposeOfAppointments.Count());
            ClassicAssert.AreEqual("Name1", nextScopeOptions.PurposeOfAppointments.ElementAt(0).Name);
            ClassicAssert.AreEqual("Name2", nextScopeOptions.PurposeOfAppointments.ElementAt(1).Name);
            ClassicAssert.AreEqual("Name3", nextScopeOptions.PurposeOfAppointments.ElementAt(2).Name);
        }

        [Test]
        public async Task
            LegislativeAreaService_GetNextScopeOfAppointmentOptionsForLegislativeArea_ShouldReturnCategories()
        {
            // Arrange
            _mockPurposeOfAppointmentRepository
                .Setup(x => x.QueryAsync(It.IsAny<Expression<Func<PurposeOfAppointment, bool>>>()))
                .ReturnsAsync(new List<PurposeOfAppointment>());

            _mockCategoryRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(new List<Category>()
                {
                    new() { Id = new Guid(), Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var nextScopeOptions =
                await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(Guid.NewGuid());

            // ClassicAssert
            ClassicAssert.IsNotNull(nextScopeOptions);
            ClassicAssert.IsEmpty(nextScopeOptions.PurposeOfAppointments);
            ClassicAssert.IsNotEmpty(nextScopeOptions.Categories);
            ClassicAssert.IsEmpty(nextScopeOptions.Subcategories);
            ClassicAssert.IsEmpty(nextScopeOptions.Products);
            ClassicAssert.IsEmpty(nextScopeOptions.Procedures);
            ClassicAssert.AreEqual(3, nextScopeOptions.Categories.Count());
            ClassicAssert.AreEqual("Name1", nextScopeOptions.Categories.ElementAt(0).Name);
            ClassicAssert.AreEqual("Name2", nextScopeOptions.Categories.ElementAt(1).Name);
            ClassicAssert.AreEqual("Name3", nextScopeOptions.Categories.ElementAt(2).Name);
        }

        [Test]
        public async Task
            LegislativeAreaService_GetNextScopeOfAppointmentOptionsForLegislativeArea_ShouldReturnProducts()
        {
            // Arrange
            _mockPurposeOfAppointmentRepository
                .Setup(x => x.QueryAsync(It.IsAny<Expression<Func<PurposeOfAppointment, bool>>>()))
                .ReturnsAsync(new List<PurposeOfAppointment>());

            _mockCategoryRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(new List<Category>());

            _mockProductRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(new List<Product>()
                {
                    new() { Id = new Guid(), Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var nextScopeOptions =
                await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(Guid.NewGuid());

            // ClassicAssert
            ClassicAssert.IsNotNull(nextScopeOptions);
            ClassicAssert.IsEmpty(nextScopeOptions.PurposeOfAppointments);
            ClassicAssert.IsEmpty(nextScopeOptions.Categories);
            ClassicAssert.IsEmpty(nextScopeOptions.Subcategories);
            ClassicAssert.IsNotEmpty(nextScopeOptions.Products);
            ClassicAssert.IsEmpty(nextScopeOptions.Procedures);
            ClassicAssert.AreEqual(3, nextScopeOptions.Products.Count());
            ClassicAssert.AreEqual("Name1", nextScopeOptions.Products.ElementAt(0).Name);
            ClassicAssert.AreEqual("Name2", nextScopeOptions.Products.ElementAt(1).Name);
            ClassicAssert.AreEqual("Name3", nextScopeOptions.Products.ElementAt(2).Name);
        }

        [Test]
        public async Task
            LegislativeAreaService_GetNextScopeOfAppointmentOptionsForLegislativeArea_ShouldReturnProcedures()
        {
            // Arrange
            _mockPurposeOfAppointmentRepository
                .Setup(x => x.QueryAsync(It.IsAny<Expression<Func<PurposeOfAppointment, bool>>>()))
                .ReturnsAsync(new List<PurposeOfAppointment>());

            _mockCategoryRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(new List<Category>());

            _mockProductRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(new List<Product>());

            _mockProcedureRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Procedure, bool>>>()))
                .ReturnsAsync(new List<Procedure>()
                {
                    new() { Id = new Guid(), Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var nextScopeOptions =
                await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(Guid.NewGuid());

            // ClassicAssert
            ClassicAssert.IsNotNull(nextScopeOptions);
            ClassicAssert.IsEmpty(nextScopeOptions.PurposeOfAppointments);
            ClassicAssert.IsEmpty(nextScopeOptions.Categories);
            ClassicAssert.IsEmpty(nextScopeOptions.Subcategories);
            ClassicAssert.IsEmpty(nextScopeOptions.Products);
            ClassicAssert.IsNotNull(nextScopeOptions.Procedures);
            ClassicAssert.AreEqual(3, nextScopeOptions.Procedures.Count());
            ClassicAssert.AreEqual("Name1", nextScopeOptions.Procedures.ElementAt(0).Name);
            ClassicAssert.AreEqual("Name2", nextScopeOptions.Procedures.ElementAt(1).Name);
            ClassicAssert.AreEqual("Name3", nextScopeOptions.Procedures.ElementAt(2).Name);
        }

        #endregion

        #region GetNextScopeOfAppointmentOptionsForPurposeOfAppointment

        [Test]
        public async Task
            LegislativeAreaService_GetNextScopeOfAppointmentOptionsForPurposeOfAppointment_ShouldReturnCategories()
        {
            // Arrange
            _mockCategoryRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(new List<Category>()
                {
                    new() { Id = new Guid(), Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var nextScopeOptions =
                await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForPurposeOfAppointmentAsync(
                    Guid.NewGuid());

            // ClassicAssert
            ClassicAssert.IsNotNull(nextScopeOptions);
            ClassicAssert.IsEmpty(nextScopeOptions.PurposeOfAppointments);
            ClassicAssert.IsNotEmpty(nextScopeOptions.Categories);
            ClassicAssert.IsEmpty(nextScopeOptions.Subcategories);
            ClassicAssert.IsEmpty(nextScopeOptions.Products);
            ClassicAssert.IsEmpty(nextScopeOptions.Procedures);
            ClassicAssert.AreEqual(3, nextScopeOptions.Categories.Count());
            ClassicAssert.AreEqual("Name1", nextScopeOptions.Categories.ElementAt(0).Name);
            ClassicAssert.AreEqual("Name2", nextScopeOptions.Categories.ElementAt(1).Name);
            ClassicAssert.AreEqual("Name3", nextScopeOptions.Categories.ElementAt(2).Name);
        }

        [Test]
        public async Task
            LegislativeAreaService_GetNextScopeOfAppointmentOptionsForPurposeOfAppointment_ShouldRemoveDuplicateCategories()
        {
            // Arrange - Category names are duplicated when it has subcategories available.
            _mockCategoryRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(new List<Category>()
                {
                    new() { Id = new Guid(), Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                    new() { Id = new Guid(), Name = "Name3" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var nextScopeOptions =
                await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForPurposeOfAppointmentAsync(
                    Guid.NewGuid());

            // ClassicAssert
            ClassicAssert.IsNotNull(nextScopeOptions);
            ClassicAssert.IsEmpty(nextScopeOptions.PurposeOfAppointments);
            ClassicAssert.IsNotEmpty(nextScopeOptions.Categories);
            ClassicAssert.IsEmpty(nextScopeOptions.Subcategories);
            ClassicAssert.IsEmpty(nextScopeOptions.Products);
            ClassicAssert.IsEmpty(nextScopeOptions.Procedures);
            ClassicAssert.AreEqual(3, nextScopeOptions.Categories.Count());
            ClassicAssert.AreEqual("Name1", nextScopeOptions.Categories.ElementAt(0).Name);
            ClassicAssert.AreEqual("Name2", nextScopeOptions.Categories.ElementAt(1).Name);
            ClassicAssert.AreEqual("Name3", nextScopeOptions.Categories.ElementAt(2).Name);
        }

        [Test]
        public async Task
            LegislativeAreaService_GetNextScopeOfAppointmentOptionsForPurposeOfAppointment_ShouldReturnProducts()
        {
            // Arrange
            _mockCategoryRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(new List<Category>());

            _mockProductRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(new List<Product>()
                {
                    new() { Id = new Guid(), Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var nextScopeOptions =
                await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForPurposeOfAppointmentAsync(
                    Guid.NewGuid());

            // ClassicAssert
            ClassicAssert.IsNotNull(nextScopeOptions);
            ClassicAssert.IsEmpty(nextScopeOptions.PurposeOfAppointments);
            ClassicAssert.IsEmpty(nextScopeOptions.Categories);
            ClassicAssert.IsEmpty(nextScopeOptions.Subcategories);
            ClassicAssert.IsNotEmpty(nextScopeOptions.Products);
            ClassicAssert.IsEmpty(nextScopeOptions.Procedures);
            ClassicAssert.AreEqual(3, nextScopeOptions.Products.Count());
            ClassicAssert.AreEqual("Name1", nextScopeOptions.Products.ElementAt(0).Name);
            ClassicAssert.AreEqual("Name2", nextScopeOptions.Products.ElementAt(1).Name);
            ClassicAssert.AreEqual("Name3", nextScopeOptions.Products.ElementAt(2).Name);
        }

        [Test]
        public async Task
            LegislativeAreaService_GetNextScopeOfAppointmentOptionsForPurposeOfAppointment_ShouldReturnProcedures()
        {
            // Arrange
            _mockCategoryRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(new List<Category>());

            _mockProductRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(new List<Product>());

            _mockProcedureRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Procedure, bool>>>()))
                .ReturnsAsync(new List<Procedure>()
                {
                    new() { Id = new Guid(), Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var nextScopeOptions =
                await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForPurposeOfAppointmentAsync(
                    Guid.NewGuid());

            // ClassicAssert
            ClassicAssert.IsNotNull(nextScopeOptions);
            ClassicAssert.IsEmpty(nextScopeOptions.PurposeOfAppointments);
            ClassicAssert.IsEmpty(nextScopeOptions.Categories);
            ClassicAssert.IsEmpty(nextScopeOptions.Subcategories);
            ClassicAssert.IsEmpty(nextScopeOptions.Products);
            ClassicAssert.IsNotEmpty(nextScopeOptions.Procedures);
            ClassicAssert.AreEqual(3, nextScopeOptions.Procedures.Count());
            ClassicAssert.AreEqual("Name1", nextScopeOptions.Procedures.ElementAt(0).Name);
            ClassicAssert.AreEqual("Name2", nextScopeOptions.Procedures.ElementAt(1).Name);
            ClassicAssert.AreEqual("Name3", nextScopeOptions.Procedures.ElementAt(2).Name);
        }

        #endregion

        #region GetNextScopeOfAppointmentOptionsForCategory

        [Test]
        public async Task LegislativeAreaService_GetNextScopeOfAppointmentOptionsForCategory_ShouldReturnSubcategories()
        {
            var categoryId = new Guid();

            // Arrange
            _mockCategoryRepository.SetupSequence(x => x.QueryAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(new List<Category>()
                {
                    new() { Id = categoryId, Name = "Name1" },
                });

            _mockSubCategoryRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<SubCategory, bool>>>()))
                .ReturnsAsync(new List<SubCategory>()
                {
                    new() { Id = new Guid(), Name = "Sub1", CategoryId = categoryId },
                    new() { Id = new Guid(), Name = "Sub2", CategoryId = categoryId },
                    new() { Id = new Guid(), Name = "Sub3", CategoryId = categoryId },
                });

            // Act
            var nextScopeOptions =
                await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForCategoryAsync(Guid.NewGuid());

            // ClassicAssert
            ClassicAssert.IsNotNull(nextScopeOptions);
            ClassicAssert.IsEmpty(nextScopeOptions.PurposeOfAppointments);
            ClassicAssert.IsEmpty(nextScopeOptions.Categories);
            ClassicAssert.IsNotEmpty(nextScopeOptions.Subcategories);
            ClassicAssert.IsEmpty(nextScopeOptions.Products);
            ClassicAssert.IsEmpty(nextScopeOptions.Procedures);
            ClassicAssert.AreEqual(3, nextScopeOptions.Subcategories.Count());
            ClassicAssert.AreEqual("Sub1", nextScopeOptions.Subcategories.ElementAt(0).Name);
            ClassicAssert.AreEqual("Sub2", nextScopeOptions.Subcategories.ElementAt(1).Name);
            ClassicAssert.AreEqual("Sub3", nextScopeOptions.Subcategories.ElementAt(2).Name);
        }

        [Test]
        public async Task LegislativeAreaService_GetNextScopeOfAppointmentOptionsForCategory_ShouldReturnProducts()
        {
            // Arrange - Note the empty Subcategory.
            _mockCategoryRepository.SetupSequence(x => x.QueryAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(new List<Category>()
                {
                    new() { Id = new Guid(), Name = "Name1" },
                });

            _mockProductRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(new List<Product>()
                {
                    new() { Id = new Guid(), Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var nextScopeOptions =
                await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForCategoryAsync(Guid.NewGuid());

            // ClassicAssert
            ClassicAssert.IsNotNull(nextScopeOptions);
            ClassicAssert.IsEmpty(nextScopeOptions.PurposeOfAppointments);
            ClassicAssert.IsEmpty(nextScopeOptions.Categories);
            ClassicAssert.IsEmpty(nextScopeOptions.Subcategories);
            ClassicAssert.IsNotEmpty(nextScopeOptions.Products);
            ClassicAssert.IsEmpty(nextScopeOptions.Procedures);
            ClassicAssert.AreEqual(3, nextScopeOptions.Products.Count());
            ClassicAssert.AreEqual("Name1", nextScopeOptions.Products.ElementAt(0).Name);
            ClassicAssert.AreEqual("Name2", nextScopeOptions.Products.ElementAt(1).Name);
            ClassicAssert.AreEqual("Name3", nextScopeOptions.Products.ElementAt(2).Name);
        }

        [Test]
        public async Task LegislativeAreaService_GetNextScopeOfAppointmentOptionsForCategory_ShouldReturnProcedures()
        {
            // Arrange - Note the empty Subcategory.
            _mockCategoryRepository.SetupSequence(x => x.QueryAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(new List<Category>()
                {
                    new() { Id = new Guid(), Name = "Name1" },
                });

            _mockProductRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(new List<Product>());

            _mockProcedureRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Procedure, bool>>>()))
                .ReturnsAsync(new List<Procedure>()
                {
                    new() { Id = new Guid(), Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var nextScopeOptions =
                await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForCategoryAsync(Guid.NewGuid());

            // ClassicAssert
            ClassicAssert.IsNotNull(nextScopeOptions);
            ClassicAssert.IsEmpty(nextScopeOptions.PurposeOfAppointments);
            ClassicAssert.IsEmpty(nextScopeOptions.Categories);
            ClassicAssert.IsEmpty(nextScopeOptions.Subcategories);
            ClassicAssert.IsEmpty(nextScopeOptions.Products);
            ClassicAssert.IsNotEmpty(nextScopeOptions.Procedures);
            ClassicAssert.AreEqual(3, nextScopeOptions.Procedures.Count());
            ClassicAssert.AreEqual("Name1", nextScopeOptions.Procedures.ElementAt(0).Name);
            ClassicAssert.AreEqual("Name2", nextScopeOptions.Procedures.ElementAt(1).Name);
            ClassicAssert.AreEqual("Name3", nextScopeOptions.Procedures.ElementAt(2).Name);
        }

        #endregion

        #region GetNextScopeOfAppointmentOptionsForSubCategory

        [Test]
        public async Task NoProductsFound_GetNextScopeOfAppointmentOptionsForSubCategory_ShouldReturnEmptyOptions()
        {
            // Arrange
            _mockProductRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(new List<Product>(0));

            // Act
            var nextScopeOptions =
                await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForSubCategoryAsync(Guid.NewGuid());

            // ClassicAssert
            ClassicAssert.IsNotNull(nextScopeOptions);
            ClassicAssert.IsEmpty(nextScopeOptions.PurposeOfAppointments);
            ClassicAssert.IsEmpty(nextScopeOptions.Categories);
            ClassicAssert.IsEmpty(nextScopeOptions.Subcategories);
            ClassicAssert.IsEmpty(nextScopeOptions.Products);
            ClassicAssert.IsEmpty(nextScopeOptions.Procedures);
        }

        [Test]
        public async Task LegislativeAreaService_GetNextScopeOfAppointmentOptionsForSubCategory_ShouldReturnProducts()
        {
            var subCategoryId = new Guid();

            // Arrange
            _mockProductRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(new List<Product>
                {
                    new() { Id = new Guid(), Name = "Prod1", SubCategoryId = subCategoryId },
                    new() { Id = new Guid(), Name = "Prod2", SubCategoryId = subCategoryId },
                    new() { Id = new Guid(), Name = "Prod3", SubCategoryId = subCategoryId },
                });

            // Act
            var nextScopeOptions =
                await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForSubCategoryAsync(Guid.NewGuid());

            // ClassicAssert
            ClassicAssert.IsNotNull(nextScopeOptions);
            ClassicAssert.IsEmpty(nextScopeOptions.PurposeOfAppointments);
            ClassicAssert.IsEmpty(nextScopeOptions.Categories);
            ClassicAssert.IsEmpty(nextScopeOptions.Subcategories);
            ClassicAssert.IsNotEmpty(nextScopeOptions.Products);
            ClassicAssert.IsEmpty(nextScopeOptions.Procedures);
            ClassicAssert.AreEqual(3, nextScopeOptions.Products.Count());
            ClassicAssert.AreEqual("Prod1", nextScopeOptions.Products.ElementAt(0).Name);
            ClassicAssert.AreEqual("Prod2", nextScopeOptions.Products.ElementAt(1).Name);
            ClassicAssert.AreEqual("Prod3", nextScopeOptions.Products.ElementAt(2).Name);
        }

        #endregion

        #region GetNextScopeOfAppointmentOptionsForProduct

        [Test]
        public async Task LegislativeAreaService_GetNextScopeOfAppointmentOptionsForProduct_ShouldReturnProcedures()
        {
            // Arrange
            _mockProcedureRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Procedure, bool>>>()))
                .ReturnsAsync(new List<Procedure>()
                {
                    new() { Id = new Guid(), Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var nextScopeOptions =
                await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForProductAsync(Guid.NewGuid());

            // ClassicAssert
            ClassicAssert.IsNotNull(nextScopeOptions);
            ClassicAssert.IsEmpty(nextScopeOptions!.PurposeOfAppointments);
            ClassicAssert.IsEmpty(nextScopeOptions.Categories);
            ClassicAssert.IsEmpty(nextScopeOptions.Subcategories);
            ClassicAssert.IsEmpty(nextScopeOptions.Products);
            ClassicAssert.IsNotEmpty(nextScopeOptions.Procedures);
            ClassicAssert.AreEqual(3, nextScopeOptions.Procedures.Count());
            ClassicAssert.AreEqual("Name1", nextScopeOptions.Procedures.ElementAt(0).Name);
            ClassicAssert.AreEqual("Name2", nextScopeOptions.Procedures.ElementAt(1).Name);
            ClassicAssert.AreEqual("Name3", nextScopeOptions.Procedures.ElementAt(2).Name);
        }

        #endregion


        #region GetPurposeOfAppointmentById

        [Test]
        public void EmptyGuid_GetPurposeOfAppointmentById_ShouldThrowException()
        {
            // Arrange & Act & ClassicAssert
            ClassicAssert.ThrowsAsync<Exception>(() => _legislativeAreaService.GetPurposeOfAppointmentByIdAsync(Guid.Empty));
        }

        [Test]
        public async Task
            LegislativeAreaService_GetPurposeOfAppointmentById_ShouldReturnPurposeOfAppointmentFromRepository()
        {
            // Arrange
            var testGuid = Guid.NewGuid();
            _mockPurposeOfAppointmentRepository
                .Setup(x => x.QueryAsync(It.IsAny<Expression<Func<PurposeOfAppointment, bool>>>()))
                .ReturnsAsync(new List<PurposeOfAppointment>
                {
                    new() { Id = testGuid, Name = "Name1" },
                    new() { Id = new Guid(), Name = "Name2" },
                    new() { Id = new Guid(), Name = "Name3" },
                });

            // Act
            var purposeOfAppointment = await _legislativeAreaService.GetPurposeOfAppointmentByIdAsync(testGuid);

            // ClassicAssert
            ClassicAssert.IsNotNull(purposeOfAppointment);
            ClassicAssert.AreEqual("Name1", purposeOfAppointment!.Name);
        }

        #endregion

        #region GetCategoryById

        [Test]
        public void EmptyGuid_GetCategoryById_ShouldThrowException()
        {
            // Arrange & Act & ClassicAssert
            ClassicAssert.ThrowsAsync<Exception>(() => _legislativeAreaService.GetCategoryByIdAsync(Guid.Empty));
        }

        [Test]
        public async Task LegislativeAreaService_GetCategoryById_ShouldReturnCategoryFromRepository()
        {
            // Arrange
            var testGuid = Guid.NewGuid();
            _mockCategoryRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(new List<Category>
                {
                    new()
                    {
                        Id = testGuid, Name = "Name1", LegislativeAreaId = new Guid(),
                        PurposeOfAppointmentId = new Guid()
                    },
                    new()
                    {
                        Id = new Guid(), Name = "Name2", LegislativeAreaId = new Guid(),
                        PurposeOfAppointmentId = new Guid()
                    },
                    new()
                    {
                        Id = new Guid(), Name = "Name3", LegislativeAreaId = new Guid(),
                        PurposeOfAppointmentId = new Guid()
                    },
                });

            // Act
            var category = await _legislativeAreaService.GetCategoryByIdAsync(testGuid);

            // ClassicAssert
            ClassicAssert.IsNotNull(category);
            ClassicAssert.AreEqual("Name1", category!.Name);
        }

        #endregion

        #region GetSubCategoryById

        [Test]
        public void EmptyGuid_GetSubCategoryById_ShouldThrowException()
        {
            // Arrange & Act & ClassicAssert
            ClassicAssert.ThrowsAsync<Exception>(() => _legislativeAreaService.GetSubCategoryByIdAsync(Guid.Empty));
        }

        [Test]
        public async Task LegislativeAreaService_GetSubCategoryById_ShouldReturnSubCategoryFromRepository()
        {
            // Arrange
            var testGuid = Guid.NewGuid();
            _mockSubCategoryRepository.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<SubCategory, bool>>>()))
                .ReturnsAsync(new List<SubCategory>
                {
                    new() { Id = testGuid, Name = "Name1", CategoryId = new Guid() },
                    new() { Id = new Guid(), Name = "Name2", CategoryId = new Guid() },
                    new() { Id = new Guid(), Name = "Name3", CategoryId = new Guid() },
                });

            // Act
            var subCategory = await _legislativeAreaService.GetSubCategoryByIdAsync(testGuid);

            // ClassicAssert
            ClassicAssert.IsNotNull(subCategory);
            ClassicAssert.AreEqual("Name1", subCategory!.Name);
        }

        #endregion
    }
}