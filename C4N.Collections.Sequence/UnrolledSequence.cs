using System;
using System.Collections;
using System.Collections.Generic;

namespace C4N.Collections.Sequence;

public readonly partial struct UnrolledSequence<T> : IEnumerable<ReadOnlyMemory<T>>
{
    public static UnrolledSequence<T> Empty => new();

    public UnrolledSequence(SequencePosition head, SequencePosition tail)
    {
        this._headSegment = head.GetObject() as UnrolledSequenceSegment<T> ?? throw new NullReferenceException("Unsupported segment type");
        this._headInteger = head.GetInteger();
        this._tailSegment = tail.GetObject() as UnrolledSequenceSegment<T> ?? throw new NullReferenceException("Unsupported segment type");
        this._tailInteger = tail.GetInteger();
    }

    readonly UnrolledSequenceSegment<T> _headSegment;
    readonly int _headInteger;
    readonly UnrolledSequenceSegment<T> _tailSegment;
    readonly int _tailInteger;

    public SequencePosition Head => new(this._headSegment, this._headInteger);//inclusive
    public SequencePosition Tail => new(this._tailSegment, this._tailInteger);//exclusive
    public bool IsEmpty => this._headSegment == this._tailSegment && this._headInteger == this._tailInteger;
    public long Length => (this._tailSegment.TotalIndex - this._headSegment.TotalIndex) + (this._tailInteger - this._headInteger);

    private SequencePosition Seek(SequencePosition position, int delta)
    {
        var segment = (position.GetObject() as UnrolledSequenceSegment<T>)!;
        var integer = position.GetInteger();
        var index = delta + integer;
        do
        {
            var last = segment == this._tailSegment;
            var len = last ? this._tailInteger : segment!.Length;
            if (index < len) return new(segment, index);

            index -= len;
            segment = segment!.Next;
            if (last) break;
        }
        while (true);

        return this.Tail;
    }
    private SequencePosition SeekSegment(SequencePosition position, int count)
    {
        var segment = (position.GetObject() as UnrolledSequenceSegment<T>)!;
        var integer = position.GetInteger();
        while (count > 0)
        {
            var next = segment.Next;
            if (next is null) break;
            segment = next;
            integer = 0;
        }
        return new(segment, integer);
    }

    public UnrolledSequence<T> Slice(int index) => new(this.Seek(this.Head, index), this.Tail);
    public UnrolledSequence<T> Slice(int index, int length)
    {
        var start = this.Seek(this.Head, index);
        var end = this.Seek(start, length);
        return new(start, end);
    }
    public UnrolledSequence<T> SliceSegment(int count) => new(this.SeekSegment(this.Head, count), this.Tail);

    public long GetIndex(SequencePosition position)
    {
        var segment = position.GetObject() as UnrolledSequenceSegment<T>;
        var index = position.GetInteger();
        if (segment is null) Throw.NullReference(nameof(position));
        return segment!.TotalIndex + index;
    }
    public SequencePosition GetPosition(long position, SequencePosition origin)
    {
        var segment = origin.GetObject() as UnrolledSequenceSegment<T>;
        if (segment is null) Throw.NullReference(nameof(position));
        do
        {
            var delta = position - segment!.TotalIndex;
            if (delta < segment.Length) return new(segment, (int)delta);
            segment = segment.Next;
        }
        while (segment is not null);
        return this.Tail;
    }

    public BufferEnumerable EnumerateBuffer() => new(this);
    public IndexedEnumerable EnumerateIndexed() => new(this);
    public Enumerator GetEnumerator() => new(this);

    IEnumerator<ReadOnlyMemory<T>> IEnumerable<ReadOnlyMemory<T>>.GetEnumerator() => this.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}