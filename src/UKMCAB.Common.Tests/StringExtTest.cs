using NUnit.Framework;

namespace UKMCAB.Common.Tests;

public class StringExtTest
{
    [Test]
    public void TestJoin()
    {
        var d = StringExt.Join(", ", new[] { "", "54", "West Road", null, null, "", "Exeter", "Devon" });
        Assert.That(d, Is.EqualTo("54, West Road, Exeter, Devon"));
    }

    [Test]
    public void TestKeyify()
    {
        var d = StringExt.Keyify("test1", "test2", "test3");
        Assert.That(d, Is.EqualTo("test1-test2-test3"));
    }

}