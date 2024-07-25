using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.Tests.Models
{
    [TestFixture]
    public class PaginationViewModelTests
    {
        [TestCaseSource(typeof(PaginationTestDataSource), nameof(PaginationTestDataSource.TestCases))]
        public void Pagination_PageRanges_Values_Correct(int pageNumber, int resultsPerPage, int total, int maxPageRange, ExpectedResult expected)
        {
            //Arrange 
            var sut = new PaginationViewModel() { 
                PageNumber = pageNumber,
                ResultsPerPage = resultsPerPage,
                Total = total, 
                MaxPageRange = maxPageRange,
                ResultType = "whatever"
            };

            //Act 
            var result = sut.PageRange();

            //Assert 
            Assert.IsNotNull(result);
            Assert.AreEqual(expected.range, result);

            Assert.AreEqual(expected.showPrevious, sut.ShowPrevious);
            Assert.AreEqual(expected.showNext, sut.ShowNext);
            Assert.AreEqual(expected.firstResult, sut.FirstResult);
            Assert.AreEqual(expected.lastResult, sut.LastResult);
        }

        internal class PaginationTestDataSource { 
            internal static IEnumerable TestCases { 
                get {
                    yield return new TestCaseData(1, 10, 1000, 5, new ExpectedResult() { showPrevious = false, showNext = true, firstResult = 1, lastResult = 10, range = new[] { 1, 2, 3, 4, 5 } });
                    yield return new TestCaseData(2, 10, 1000, 5, new ExpectedResult() { showPrevious = true, showNext = true, firstResult = 11, lastResult = 20, range = new[] { 1, 2, 3, 4, 5 } });
                    yield return new TestCaseData(3, 10, 1000, 5, new ExpectedResult() { showPrevious = true, showNext = true, firstResult = 21, lastResult = 30, range = new[] { 1, 2, 3, 4, 5 } });
                    yield return new TestCaseData(55, 10, 1000, 5, new ExpectedResult() { showPrevious = true, showNext = true, firstResult = 541, lastResult = 550, range = new[] { 1, 53, 54, 55, 56, 57 } });
                    yield return new TestCaseData(99, 10, 1000, 5, new ExpectedResult() { showPrevious = true, showNext = true, firstResult = 981, lastResult = 990, range = new[] { 1, 96, 97, 98, 99, 100 } });
                    yield return new TestCaseData(100, 10, 1000, 5, new ExpectedResult() { showPrevious = true, showNext = false, firstResult = 991, lastResult = 1000, range = new[] { 1, 96, 97, 98, 99, 100 } });

                    yield return new TestCaseData(100, 10, 1000, 3, new ExpectedResult() { showPrevious = true, showNext = false, firstResult = 991, lastResult = 1000, range = new[] { 1, 98, 99, 100 } });
                    yield return new TestCaseData(100, 10, 1000, 2, new ExpectedResult() { showPrevious = true, showNext = false, firstResult = 991, lastResult = 1000, range = new[] { 1, 98, 99, 100 } });
                    
                    yield return new TestCaseData(2, 20, 55, 5, new ExpectedResult() { showPrevious = true, showNext = true, firstResult = 21, lastResult = 40, range = new[] { 1, 2, 3 } });
                    yield return new TestCaseData(2, 20, 55, 3, new ExpectedResult() { showPrevious = true, showNext = true, firstResult = 21, lastResult = 40, range = new[] { 1, 2, 3 } });
                    
                    yield return new TestCaseData(2, 20, 226, 3, new ExpectedResult() { showPrevious = true, showNext = true, firstResult = 21, lastResult = 40, range = new[] { 1, 2, 3 } });

                    yield return new TestCaseData(2, 20, 226, 2, new ExpectedResult() { showPrevious = true, showNext = true, firstResult = 21, lastResult = 40, range = new[] { 1, 2, 3 } });
                    yield return new TestCaseData(7, 20, 226, 2, new ExpectedResult() { showPrevious = true, showNext = true, firstResult = 121, lastResult = 140, range = new[] { 1, 5, 6, 7 } });
                    yield return new TestCaseData(2, 20, 226, 1, new ExpectedResult() { showPrevious = true, showNext = true, firstResult = 21, lastResult = 40, range = new[] { 1, 2, 3 } });
                    yield return new TestCaseData(7, 20, 226, 1, new ExpectedResult() { showPrevious = true, showNext = true, firstResult = 121, lastResult = 140, range = new[] { 1, 5, 6, 7 } });
                }
            }
        }

        public record ExpectedResult()
        {
            internal bool showPrevious;
            internal bool showNext;
            internal int firstResult;
            internal int lastResult;
            internal IEnumerable<int> range;
        }               

    }
}
