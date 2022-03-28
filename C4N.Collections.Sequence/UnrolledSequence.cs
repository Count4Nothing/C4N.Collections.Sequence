using System;
using System.Collections;
using System.Collections.Generic;

namespace C4N.Collections.Sequence;

public readonly struct UnrolledSequence<T> : IEnumerable<ReadOnlyMemory<T>>
{
    public struct Enumerator : IEnumerator<ReadOnlyMemory<T>>
    {
        public Enumerator(UnrolledSequence<T> sequence)
        {
            this.sequence = sequence;
            this.segment = null;
            this.from = this.to = default;
        }

        readonly UnrolledSequence<T> sequence;

        UnrolledSequenceSegment<T>? segment;
        int from, to;

        public ReadOnlyMemory<T> Current => this.segment!.Array.AsMemory(this.from, this.to - this.from);
        object IEnumerator.Current => this.Current;

        public bool MoveNext()
        {
            (this.segment, this.from) = this.segment is null 
                                        ? (this.sequence._startSegment, this.sequence._startInteger) 
                                        : (this.segment.Next, 0);
            this.to = (this.segment == this.sequence._endSegment) ? this.sequence._endInteger : (this.segment?.Length ?? 0);

            return this.segment is not null;
        }
        public void Dispose() { }
        public void Reset() => throw new NotSupportedException();
    }

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

    public Enumerator GetEnumerator() => new(this);
    IEnumerator<ReadOnlyMemory<T>> IEnumerable<ReadOnlyMemory<T>>.GetEnumerator() => this.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}