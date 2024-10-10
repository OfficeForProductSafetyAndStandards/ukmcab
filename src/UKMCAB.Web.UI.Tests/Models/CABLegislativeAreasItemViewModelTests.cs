using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;


namespace UKMCAB.Web.UI.Tests.Models
{
    [TestFixture]
    public class CABLegislativeAreasItemViewModelTests
    {
        [Test, TestCaseSource(nameof(GetTestCases))]
        public void CABLegislativeAreasItemViewModel_IsComplete_Should_Return_True(string procedure, bool canChooseSoa, bool? isProvisional, DateTime? reviewDate, bool expectedResult)
        {
            // Arrange
            var _sut = new CABLegislativeAreasItemViewModel();
            _sut.ScopeOfAppointments = new();
            var procedureName = procedure;
            var soa1 = new LegislativeAreaListItemViewModel();
            soa1.Procedures = new() {procedureName};
            _sut.ScopeOfAppointments.Add(soa1);
            _sut.IsProvisional = isProvisional;
            _sut.CanChooseScopeOfAppointment = canChooseSoa;
            _sut.ReviewDate = reviewDate;

            // ClassicAssert
            ClassicAssert.That( _sut.IsComplete.Equals(expectedResult));
        }

        public static IEnumerable<TestCaseData> GetTestCases()
        {
            yield return new TestCaseData("Schedule 4 – Module B Type examination", true, true, null, true);
            yield return new TestCaseData("", true, true, null, false);
            yield return new TestCaseData("", false, false, null, true);
            yield return new TestCaseData("", false, null, null, false);
            yield return new TestCaseData("Schedule 4 – Module B Type examination", true, true, new DateTime(2085, 1, 1), true);
            yield return new TestCaseData("Schedule 4 – Module B Type examination", true, true, new DateTime(2023, 1, 1), false);
        }

        [TestCase(true, true)]
        [TestCase(false, false)]
        [TestCase(null, false)]
        public void CABLegislativeAreasItemViewModel_IsNewlyCreated_Returns_Expected_Value(bool? newlyCreated, bool expected)
        {
            // Arrange
            var _sut = new CABLegislativeAreasItemViewModel();
            _sut.NewlyCreated = newlyCreated;

            // ClassicAssert
            ClassicAssert.AreEqual(expected, _sut.IsNewlyCreated);
        }
    }
}
