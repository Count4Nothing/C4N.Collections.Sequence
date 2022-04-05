﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace C4N.Collections.Sequence;

public readonly partial struct UnrolledSequence<T> : IEnumerable<ReadOnlyMemory<T>>
{
    public static UnrolledSequence<T> Empty => new();

    public UnrolledSequence(SequencePosition start, SequencePosition end)
    {
        this._startSegment = start.GetObject() as UnrolledSequenceSegment<T> ?? throw new NullReferenceException("Unsupported segment type");
        this._startInteger = start.GetInteger();
        this._endSegment = end.GetObject() as UnrolledSequenceSegment<T> ?? throw new NullReferenceException("Unsupported segment type");
        this._endInteger = end.GetInteger();
    }

    readonly UnrolledSequenceSegment<T> _startSegment;
    readonly int _startInteger;
    readonly UnrolledSequenceSegment<T> _endSegment;
    readonly int _endInteger;

    public SequencePosition Start => new(this._startSegment, this._startInteger);//inclusive
    public SequencePosition End => new(this._endSegment, this._endInteger);//exclusive
    public bool IsEmpty => this._startSegment == this._endSegment && this._startInteger == this._endInteger;
    public long Length => (this._endSegment.TotalIndex - this._startSegment.TotalIndex) + (this._endInteger - this._startInteger);

    public UnrolledSequence<T> Slice(int index)
    {
        var segment = this._startSegment;
        index += this._startInteger;

        while (segment is not null)
        {
            var len = (segment == this._endSegment) ? this._endInteger : segment.Length;
            if (index < len) return new(new(segment, index), this.End);

            index -= len;
            segment = segment.Next;
        }

        return Empty;
    }

    public long GetIndex(SequencePosition position)
    {
        var segment = position.GetObject() as UnrolledSequenceSegment<T> ?? throw new NullReferenceException(nameof(position));
        var index = position.GetInteger();
        return segment.TotalIndex + index;
    }

    public BufferEnumerable EnumerateBuffer() => new(this);
    public Enumerator GetEnumerator() => new(this);
    
    IEnumerator<ReadOnlyMemory<T>> IEnumerable<ReadOnlyMemory<T>>.GetEnumerator() => this.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}