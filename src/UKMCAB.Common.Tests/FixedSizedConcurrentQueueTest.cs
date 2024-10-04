using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace UKMCAB.Common.Tests;

public class FixedSizedConcurrentQueueTest
{
    [Test]
    public void FixedSizedConcurrentQueueTest1()
    {
        var q = new FixedSizedConcurrentQueue<object>(10);
        var arr = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.Cast<object>();

        q.EnqueueAll(arr);

        var data = q.DequeueAll().ToArray();
        ClassicAssert.That(data.SequenceEqual(arr), Is.True);
        ClassicAssert.That(q.DequeueAll(), Is.Empty);

        var arr2 = new[] { 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }.Cast<object>();
        q.EnqueueAll(arr2);

        data = q.DequeueAll().ToArray();
        ClassicAssert.That(data.SequenceEqual(arr2), Is.True);
        ClassicAssert.That(q.DequeueAll(), Is.Empty);

    }
}
