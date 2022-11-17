using MoreLinq;
using System;
using System.Collections.Concurrent;

namespace UKMCAB.Common;

/// <summary>
/// Enhances ConcurrentQueue such that it will only ever fill to the maximum number of items and disregard the oldest.
/// </summary>
/// <typeparam name="T"></typeparam>
public class FixedSizedConcurrentQueue<T> : ConcurrentQueue<T>
{
    private readonly object _mutex = new();
    public int Size { get; }
    public FixedSizedConcurrentQueue(int size) => Size = size;
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
