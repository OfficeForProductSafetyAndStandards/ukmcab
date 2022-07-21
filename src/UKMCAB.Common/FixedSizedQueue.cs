using MoreLinq;
using System.Collections.Concurrent;

namespace UKMCAB.Common;

public class FixedSizedQueue<T> : ConcurrentQueue<T>
{
    private readonly object _mutex = new();
    public int Size { get; }
    public FixedSizedQueue(int size) => Size = size;
    public new void Enqueue(T obj)
    {
        base.Enqueue(obj);
        lock (_mutex)
        {
            while (Count > Size)
            {
                TryDequeue(out _);
            }
        }
    }

    public IEnumerable<T> DequeueAll()
    {
        var buffer = new List<T>();
        while (TryDequeue(out T item))
        {
            buffer.Add(item);
        }
        return buffer;
    }

    public void EnqueueAll(IEnumerable<T> items) => items.ForEach(x => Enqueue(x));
}