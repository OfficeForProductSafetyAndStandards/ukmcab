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

    [TestCase(null, 10, "")]
    [TestCase("", 10, "")]
    [TestCase("Short", 10, "Short")]
    [TestCase("Exact length...", 15, "Exact length...")]
    [TestCase("This is a very long string that needs to be truncated.", 20, "This is a very lo...")]
    [TestCase("This is a very long string that needs to be truncated.", 30, "This is a very long string...")]
    [TestCase("This is a very long string. That has a full stop.", 30, "This is a very long string...")]
    [TestCase("This is a very long string that needs to be truncated.", 3, "...")]
    [TestCase("This is a very long string that needs to be truncated.", 0, "")]
    [TestCase("This is a very long string that needs to be truncated.", -5, "")]
    public void TruncateWithEllipsis_ShouldReturnExpectedResult(string input, int maxLength, string expected)
    {
        string result = input.TruncateWithEllipsis(maxLength);
        Assert.That(expected, Is.EqualTo(result));
    }
}