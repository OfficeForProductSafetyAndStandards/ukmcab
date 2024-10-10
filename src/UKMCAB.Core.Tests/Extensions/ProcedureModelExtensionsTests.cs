using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Core.Extensions;

namespace UKMCAB.Core.Tests.Extensions
{
    [TestFixture]
    public class ProcedureModelExtensionsTests
    {
        [Test]
        public void GetNamesByIds_ProcedureIdsMatch_ReturnsListOfProcedureModelNames()
        {
            // Arrange
            var procedureId = Guid.NewGuid();
            var sut = new List<ProcedureModel>
            {
                new()
                {
                    Name = "Test name 1",
                    Id = procedureId
                },
                new()
                {
                    Name = "Test name 2"
                }
            };
            var expectedResult = new List<string> { "Test name 1" };

            // Act
            var result = sut.GetNamesByIds(new List<Guid> { procedureId });

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }
    }
}
