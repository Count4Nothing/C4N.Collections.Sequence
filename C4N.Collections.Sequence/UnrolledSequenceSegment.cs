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
    public virtual Span<T> GetBuffer() => this.GetMemory().Span;
    public virtual Span<T> GetBuffer(int start) => this.GetBuffer().Slice(start);
    public virtual Span<T> GetBuffer(int start, int length) => this.GetBuffer().Slice(start, length);
    public abstract Memory<T> GetMemory();
    public virtual Memory<T> GetMemory(int start) => this.GetMemory().Slice(start);
    public virtual Memory<T> GetMemory(int start, int length) => this.GetMemory().Slice(start, length);
}

public readonly struct ReadOnlyUnrolledSequenceSegment<T>
{
    public ReadOnlyUnrolledSequenceSegment(UnrolledSequenceSegment<T> segment, int offset, int length)
    {
        this.segment = segment;
        this.offset = offset;
        this.Length = length;
    }

    readonly int offset;
    readonly UnrolledSequenceSegment<T> segment;

    public int Length { get; }
    public ReadOnlyMemory<T> Memory => this.segment.GetMemory(this.offset, this.Length);
    public ReadOnlySpan<T> Span => this.segment.GetBuffer(this.offset, this.Length);

    public SequencePosition GetPosition(int index)
    {
        if ((uint)index >= (uint)this.Length) Throw.IndexOutOfRange(nameof(index));
        return new(this.segment, this.offset + index);
    }

    public long TotalIndex => this.segment.TotalIndex;
}


public class ArrayUnrolledSequenceSegment<T> : UnrolledSequenceSegment<T>
{
    public T[] Array { get; }
    public override int Length => this.Array.Length;
    public override Span<T> GetBuffer() => this.Array;
    public override Span<T> GetBuffer(int start) => this.Array.AsSpan(start);
    public override Span<T> GetBuffer(int start, int length) => this.Array.AsSpan(start, length);
    public override Memory<T> GetMemory() => this.Array;
    public override Memory<T> GetMemory(int start) => this.Array.AsMemory(start);
    public override Memory<T> GetMemory(int start, int length) => this.Array.AsMemory(start, length);

    public ArrayUnrolledSequenceSegment(T[] array)
    {
        this.Array = array;
    }
    public ArrayUnrolledSequenceSegment(int length)
        : this(new T[length])
    { }
}