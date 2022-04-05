using System;

namespace C4N.Collections.Sequence;

public abstract class UnrolledSequenceSegment<T>
{
    private UnrolledSequenceSegment<T>? next;
    public UnrolledSequenceSegment<T>? Next
    {
        get => this.next;
        set
        {
            this.next = value;
            if (value is null) return;
            value.TotalIndex = this.TotalIndex + this.Length;
        }
    }
    public long TotalIndex { get; protected set; }
    public abstract int Length { get; }
    public abstract Span<T> GetBuffer();
    public abstract Memory<T> GetMemory(int start, int length);
}

public class ArrayUnrolledSequenceSegment<T> : UnrolledSequenceSegment<T>
{
    public T[] Array { get; }
    public override int Length => this.Array.Length;
    public override Memory<T> GetMemory(int start, int length) => this.Array.AsMemory(start, length);
    public override Span<T> GetBuffer() => this.Array;

    public ArrayUnrolledSequenceSegment(T[] array)
    {
        this.Array = array;
    }
    public ArrayUnrolledSequenceSegment(int length) 
        : this(new T[length])
    { }
}