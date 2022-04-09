using System;
using System.Runtime.CompilerServices;

namespace C4N.Collections.Sequence;

public abstract class UnrolledSequenceSource<T> : IDisposable
{
    public ReadResult<T> Read() => this.Read(0);
    public abstract ReadResult<T> Read(int sizeHint);
    public abstract void Advance(long consumed);
    public abstract void Advance(SequencePosition consumed);
    public abstract void Dispose();
}
public static partial class UnrolledSequenceSource
{
    static int ByteLOHThreshold { get; } = 85000;
    static int DefaultMinBufferSize { get; } = 4096;
    static int DefaultMaxBufferSize { get; } = 2048 * 1024;

    public static (int Min, int Max) RecommendedBufferSizeOf<T>()
    {
        var size = Unsafe.SizeOf<T>();
        var max = (ByteLOHThreshold - 1) / size;
        if (max <= 1) return (1, 1);
        if (DefaultMaxBufferSize < max) max = DefaultMaxBufferSize;
        var min = Math.Min(DefaultMinBufferSize, max);
        return (min, max);
    }
}