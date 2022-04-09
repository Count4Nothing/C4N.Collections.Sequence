using System;
using System.Collections;
using System.Collections.Generic;

namespace C4N.Collections.Sequence;

public readonly partial struct UnrolledSequence<T> : IEnumerable<ReadOnlyMemory<T>>
{
    public static UnrolledSequence<T> Empty => new();

    public UnrolledSequence(SequencePosition head, SequencePosition tail)
        : this((head.GetObject() as UnrolledSequenceSegment<T>)!, head.GetInteger(), (tail.GetObject() as UnrolledSequenceSegment<T>)!, tail.GetInteger())
    { }
    public UnrolledSequence(UnrolledSequenceSegment<T> head, int headInteger, UnrolledSequenceSegment<T> tail, int tailInteger)
    {
        if (head.TotalIndex - tail.TotalIndex > tailInteger - headInteger) Throw.Argument("tail must be greater than or equal to head.");

        this._headSegment = head ?? throw new NullReferenceException("Unsupported segment type");
        this._headInteger = headInteger;
        this._tailSegment = tail ?? throw new NullReferenceException("Unsupported segment type");
        this._tailInteger = tailInteger;
    }
    public UnrolledSequence(T[] array, int start, int length)
    {
        var segment = new ArrayUnrolledSequenceSegment<T>(array);
        this._headSegment = segment;
        this._headInteger = start;
        this._tailInteger = length;
        this._tailSegment = segment;
    }

    readonly UnrolledSequenceSegment<T> _headSegment;
    readonly int _headInteger;
    readonly UnrolledSequenceSegment<T> _tailSegment;
    readonly int _tailInteger;

    public SequencePosition Head => new(this._headSegment, this._headInteger);//inclusive
    public SequencePosition Tail => new(this._tailSegment, this._tailInteger);//exclusive
    public bool IsEmpty => this._headSegment == this._tailSegment && this._headInteger == this._tailInteger;
    public long Length => (this._tailSegment.TotalIndex - this._headSegment.TotalIndex) + (this._tailInteger - this._headInteger);
    public ReadOnlySpan<T> FirstSpan => this._headSegment.GetBuffer(this._headInteger);

    private SequencePosition Seek(SequencePosition position, long delta)
    {
        var segment = (position.GetObject() as UnrolledSequenceSegment<T>)!;
        var integer = position.GetInteger();
        var tailSegment = this._tailSegment;
        var tailInteger = this._tailInteger;

        do
        {
            var last = ReferenceEquals(segment, tailSegment);
            var len = (last ? tailInteger : segment!.Length) - integer;
            if (delta < len) return new(segment, (int)(delta + integer));

            delta -= len;
            segment = segment!.Next;
            integer = 0;
            if (last || segment is null) Throw.Argument(nameof(position));
        }
        while (true);
    }
    private SequencePosition SeekSegment(SequencePosition position, int count)
    {
        var segment = (position.GetObject() as UnrolledSequenceSegment<T>)!;
        var integer = position.GetInteger();
        var tailSegment = this._tailSegment;
        var tailInteger = this._tailInteger;
        while (count > 0)
        {
            if (ReferenceEquals(segment, tailSegment))
            {
                integer = tailInteger;
                break;
            }
            var next = segment.Next;
            if (next is null) Throw.ArgumentOutOfRange(nameof(count));
            segment = next!;
            integer = 0;
        }
        return new(segment, integer);
    }

    public UnrolledSequence<T> Slice(SequencePosition head) => new(head, this.Tail);
    public UnrolledSequence<T> Slice(SequencePosition head, SequencePosition tail) => new(head, tail);
    public UnrolledSequence<T> Slice(SequencePosition head, long length) => new(head, this.Seek(head, length));
    public UnrolledSequence<T> Slice(long index) => new(this.Seek(this.Head, index), this.Tail);
    public UnrolledSequence<T> Slice(long index, long length)
    {
        var start = this.Seek(this.Head, index);
        var end = this.Seek(start, length);
        return new(start, end);
    }
    public UnrolledSequence<T> Slice(long index, SequencePosition tail) => new(this.Seek(this.Head, index), tail);

    public UnrolledSequence<T> SliceSegment(int count) => new(this.SeekSegment(this.Head, count), this.Tail);

    public long GetIndex(SequencePosition position)
    {
        var segment = position.GetObject() as UnrolledSequenceSegment<T>;
        var index = position.GetInteger();
        if (segment is null) Throw.NullReference(nameof(position));
        return segment!.TotalIndex + index;
    }
    public SequencePosition GetPosition(long index, SequencePosition origin)
    {
        var segment = origin.GetObject() as UnrolledSequenceSegment<T>;
        var tailSegment = this._tailSegment;
        var tailInteger = this._tailInteger;
        if (segment is null) Throw.Argument(nameof(origin));
        do
        {
            var delta = index - segment!.TotalIndex;
            if (ReferenceEquals(segment, tailSegment) && delta >= tailInteger) Throw.ArgumentOutOfRange(nameof(index));
            if (delta < segment.Length) return new(segment, (int)delta);
            segment = segment.Next;
            if (segment is null) Throw.ArgumentOutOfRange(nameof(index));
        }
        while (true);
    }
    public SequencePosition GetPosition(long index) => this.GetPosition(index, this.Head);

    public BufferEnumerable EnumerateBuffer() => new(this);
    public IndexedEnumerable EnumerateIndexed() => new(this);
    public ElementEnumerable EnumerateElement() => new(this);
    public SegmentEnumerable EnumerateSegment() => new(this);
    public Enumerator GetEnumerator() => new(this);

    IEnumerator<ReadOnlyMemory<T>> IEnumerable<ReadOnlyMemory<T>>.GetEnumerator() => this.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}
