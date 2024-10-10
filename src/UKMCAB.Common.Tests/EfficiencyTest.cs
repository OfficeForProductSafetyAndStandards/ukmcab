using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace UKMCAB.Common.Tests;

[TestFixture]
public class EfficiencyTest
{
    [Test]
    public async Task TestDoOnceAsync()
    {
        var i = 0;
        await Efficiency.DoOnceAsync("t11", "t21", () => { i++; return Task.CompletedTask; });
        await Efficiency.DoOnceAsync("t11", "t21", () => { i++; return Task.CompletedTask; });
        ClassicAssert.That(i, Is.EqualTo(1)); 
        await Efficiency.DoOnceAsync("t11", "t31", () => { i++; return Task.CompletedTask; });
        ClassicAssert.That(i, Is.EqualTo(2));
    }

    [Test]
    public void TestDoOnce()
    {
        var i = 0;
        Efficiency.DoOnce("t1", "t2", () => i++);
        Efficiency.DoOnce("t1", "t2", () => i++);
        ClassicAssert.That(i, Is.EqualTo(1));
        Efficiency.DoOnce("t1", "t3", () => i++);
        ClassicAssert.That(i, Is.EqualTo(2));
    }
}
