using AutoMapper;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Data.CosmosDb.Services;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.LegislativeAreas;

#pragma warning disable CS8618

namespace UKMCAB.Core.Tests.Services.CAB
{
    [TestFixture]
    public class LegislativeAreasServiceGetDocumentRelatedCollectionsTests
    {
        private Mock<IReadOnlyRepository<LegislativeArea>> _mockLegislativeAreaRepository;
        private Mock<IReadOnlyRepository<PurposeOfAppointment>> _mockPurposeOfAppointmentRepository;
        private Mock<IReadOnlyRepository<Category>> _mockCategoryRepository;
        private Mock<IReadOnlyRepository<SubCategory>> _mockSubCategoryRepository;
        private Mock<IReadOnlyRepository<Product>> _mockProductRepository;
        private Mock<IReadOnlyRepository<Procedure>> _mockProcedureRepository;
        private Mock<IReadOnlyRepository<DesignatedStandard>> _mockDesignatedStandardRepository;
        private Mock<IMapper> _mockMapper;

        private LegislativeAreaService _sut;

        [SetUp]
        public void Setup()
        {
            _mockLegislativeAreaRepository = new Mock<IReadOnlyRepository<LegislativeArea>>(MockBehavior.Strict);
            _mockPurposeOfAppointmentRepository = new Mock<IReadOnlyRepository<PurposeOfAppointment>>(MockBehavior.Strict);
            _mockCategoryRepository = new Mock<IReadOnlyRepository<Category>>(MockBehavior.Strict);
            _mockSubCategoryRepository = new Mock<IReadOnlyRepository<SubCategory>>(MockBehavior.Strict);
            _mockProductRepository = new Mock<IReadOnlyRepository<Product>>(MockBehavior.Strict);
            _mockProcedureRepository = new Mock<IReadOnlyRepository<Procedure>>(MockBehavior.Strict);
            _mockDesignatedStandardRepository = new Mock<IReadOnlyRepository<DesignatedStandard>>(MockBehavior.Strict);
            _mockMapper = new Mock<IMapper>(MockBehavior.Strict);

            _sut = new LegislativeAreaService(
                _mockLegislativeAreaRepository.Object,
                _mockPurposeOfAppointmentRepository.Object,
                _mockCategoryRepository.Object, 
                _mockProductRepository.Object, 
                _mockProcedureRepository.Object,
                _mockSubCategoryRepository.Object,
                _mockDesignatedStandardRepository.Object,
                _mockMapper.Object);
        }

        [Test]
        public async Task GetLegislativeAreasForDocumentAsync_ReturnsMappedLegislativeAreasForDocument()
        {
            // Arrange
            var legislativeAreaId = Guid.NewGuid();
            var legislativeAreas = new List<LegislativeArea>
            {
                new()
                {
                    Id = legislativeAreaId
                }
            };
            var expectedLegislativeAreaIds = legislativeAreas.Select(l => l.Id);
            var expectedResult = new List<LegislativeAreaModel>
            {
                new()
                {
                    Id = legislativeAreaId
                }
            };

            _mockLegislativeAreaRepository.Setup(m => m.QueryAsync(It.IsAny<Expression<Func<LegislativeArea, bool>>>()))
                .ReturnsAsync(legislativeAreas);
            _mockMapper.Setup(m => m.Map<List<LegislativeAreaModel>>(It.Is<List<LegislativeArea>>(las => las.Any() && las.All(l => expectedLegislativeAreaIds.Contains(l.Id)))))
                .Returns(new List<LegislativeAreaModel>
                {
                    new()
                    {
                        Id = legislativeAreaId
                    }
                });

            // Act
            var result = await _sut.GetLegislativeAreasForDocumentAsync(new Document());

            // ClassicAssert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public async Task GetPurposeOfAppointmentsForDocumentAsync_ReturnsMappedPurposeOfAppointmentsForDocument()
        {
            // Arrange
            var purposeOfAppointmentId = Guid.NewGuid();
            var purposeOfAppointments = new List<PurposeOfAppointment>
            {
                new()
                {
                    Id = purposeOfAppointmentId,
                }
            };
            var expectedPurposeOfAppointmentIds = purposeOfAppointments.Select(p => p.Id);
            var expectedResult = new List<PurposeOfAppointmentModel>
            {
                new()
                {
                    Id = purposeOfAppointmentId
                }
            };

            _mockPurposeOfAppointmentRepository.Setup(m => m.QueryAsync(It.IsAny<Expression<Func<PurposeOfAppointment, bool>>>()))
                .ReturnsAsync(purposeOfAppointments);
            _mockMapper.Setup(m => m.Map<List<PurposeOfAppointmentModel>>(It.Is<List<PurposeOfAppointment>>(poas => poas.Any() && poas.All(p => expectedPurposeOfAppointmentIds.Contains(p.Id)))))
                .Returns(new List<PurposeOfAppointmentModel>
                {
                    new()
                    {
                        Id = purposeOfAppointmentId
                    }
                });

            // Act
            var result = await _sut.GetPurposeOfAppointmentsForDocumentAsync(new Document());

            // ClassicAssert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public async Task GetCategoriesForDocumentAsync_ReturnsMappedCategoriesForDocument()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var categories = new List<Category>
            {
                new()
                {
                    Id = categoryId
                }
            };
            var expectedCategoryIds = categories.Select(c => c.Id);
            var expectedResult = new List<CategoryModel>
            {
                new()
                {
                    Id = categoryId
                }
            };

            _mockCategoryRepository.Setup(m => m.QueryAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(categories);
            _mockMapper.Setup(m => m.Map<List<CategoryModel>>(It.Is<List<Category>>(categories => categories.Any() && categories.All(c => expectedCategoryIds.Contains(c.Id)))))
                .Returns(new List<CategoryModel>
                {
                    new()
                    {
                        Id = categoryId
                    }
                });

            // Act
            var result = await _sut.GetCategoriesForDocumentAsync(new Document());

            // ClassicAssert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public async Task GetSubCategoriesForDocumentAsync_ReturnsMappedSubCategoriesForDocument()
        {
            // Arrange
            var subCategoryId = Guid.NewGuid();
            var subCategories = new List<SubCategory>
            {
                new()
                {
                    Id = subCategoryId
                }
            };
            var expectedSubCategoryIds = subCategories.Select(c => c.Id);
            var expectedResult = new List<SubCategoryModel>
            {
                new()
                {
                    Id = subCategoryId
                }
            };

            _mockSubCategoryRepository.Setup(m => m.QueryAsync(It.IsAny<Expression<Func<SubCategory, bool>>>()))
                .ReturnsAsync(subCategories);
            _mockMapper.Setup(m => m.Map<List<SubCategoryModel>>(It.Is<List<SubCategory>>(subCategories => subCategories.Any() && subCategories.All(s => expectedSubCategoryIds.Contains(s.Id)))))
                .Returns(new List<SubCategoryModel>
                {
                    new()
                    {
                        Id = subCategoryId
                    }
                });

            // Act
            var result = await _sut.GetSubCategoriesForDocumentAsync(new Document());

            // ClassicAssert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public async Task GetProductsForDocumentAsync_ReturnsMappedProductsForDocument()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var products = new List<Product>
            {
                new()
                {
                    Id = productId
                }
            };
            var expectedProductIds = products.Select(c => c.Id);
            var expectedResult = new List<ProductModel>
            {
                new()
                {
                    Id = productId
                }
            };

            _mockProductRepository.Setup(m => m.QueryAsync(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(products);
            _mockMapper.Setup(m => m.Map<List<ProductModel>>(It.Is<List<Product>>(products => products.Any() && products.All(p => expectedProductIds.Contains(p.Id)))))
                .Returns(new List<ProductModel>
                {
                    new()
                    {
                        Id = productId
                    }
                });

            // Act
            var result = await _sut.GetProductsForDocumentAsync(new Document());

            // ClassicAssert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public async Task GetProceduresForDocumentAsync_ReturnsMappedProceduresForDocument()
        {
            // Arrange
            var procedureId = Guid.NewGuid();
            var procedures = new List<Procedure>
            {
                new()
                {
                    Id = procedureId
                }
            };
            var expectedProcedureIds = procedures.Select(c => c.Id);
            var expectedResult = new List<ProcedureModel>
            {
                new()
                {
                    Id = procedureId
                }
            };

            _mockProcedureRepository.Setup(m => m.QueryAsync(It.IsAny<Expression<Func<Procedure, bool>>>()))
                .ReturnsAsync(procedures);
            _mockMapper.Setup(m => m.Map<List<ProcedureModel>>(It.Is<List<Procedure>>(procedures => procedures.Any() && procedures.All(p => expectedProcedureIds.Contains(p.Id)))))
                .Returns(new List<ProcedureModel>
                {
                    new()
                    {
                        Id = procedureId
                    }
                });

            // Act
            var result = await _sut.GetProceduresForDocumentAsync(new Document());

            // ClassicAssert
            result.Should().BeEquivalentTo(expectedResult);
        }
    }
}
