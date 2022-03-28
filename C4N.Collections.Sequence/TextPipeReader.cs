using System;
using System.IO;

namespace C4N.Collections.Sequence;

public sealed partial class TextPipeReader : IDisposable
{
    static int DefaultUnitMin { get; } = 4096;
    static int DefaultUnitMax { get; } = 42000;

    public TextPipeReader(TextReader reader)
    {
        this._reader = reader;
        this._pool = new(DefaultUnitMin, DefaultUnitMax);
        this._startSegment = this._endSegment = null;
        this._startInteger = this._endInteger = default;
    }

    readonly TextReader _reader;
    SegmentPool _pool;
    UnrolledSequenceSegment<char>? _startSegment;
    int _startInteger;
    UnrolledSequenceSegment<char>? _endSegment;
    int _endInteger;

    long Buffered
    {
        get
        {
            var startSegment = this._startSegment;
            var endSegment = this._endSegment;
            var startIndex = this._startInteger;
            var endIndex = this._endInteger;
            return (endSegment?.TotalIndex - startSegment?.TotalIndex + (endIndex - startIndex)) ?? 0;
        }
    }
    SequencePosition StartPosition => new(this._startSegment, this._startInteger);
    SequencePosition EndPosition => new(this._endSegment, this._endInteger);
    bool IsEnd => this._endSegment?.Array.Length > this._endInteger;

    ReadResult ReadBuffer() => new(new(this.StartPosition, this.EndPosition), this.IsEnd);
    void LoadBuffer(int sizeHint)
    {
        if (this.IsEnd) return;
        var startSegment = this._startSegment;
        var endSegment = this._endSegment;
        var startIndex = this._startInteger;
        var endIndex = this._endInteger;
        var buffered = this.Buffered;
        var pool = this._pool;

        if (buffered <= 0) sizeHint = Math.Max(pool.UnitMin, sizeHint);
        if (sizeHint <= buffered) return;
        sizeHint -= (int)buffered;

        if ((startSegment, endSegment) is (null, _) or (_, null))//when initial load
        {
            startSegment = endSegment = this._pool.Rent(sizeHint);
            startIndex = endIndex = 0;
        }

        var reader = this._reader;

        while (true)
        {
            var array = endSegment.Array;
            var capacity = array.Length - endIndex;
            var written = reader.Read(array, endIndex, capacity);
            endIndex += Math.Max(0, written);
            if (written < capacity) break;
            sizeHint -= written;
            if (sizeHint <= 0) break;
            var segment = pool.Rent(sizeHint);
            endSegment.Next = segment;
            endSegment = segment;
            endIndex = 0;
        }

        this._startSegment = startSegment;
        this._startInteger = startIndex;
        this._endSegment = endSegment;
        this._endInteger = endIndex;
    }


    public ReadResult Read(int sizeHint)
    {
        this.LoadBuffer(sizeHint);
        return this.ReadBuffer();
    }
    public ReadResult Read() => this.Read(0);
    public void Advance(SequencePosition consumed)
    {
        var segment = this._startSegment;
        var startSegment = consumed.GetObject() as UnrolledSequenceSegment<char>;
        var startInteger = consumed.GetInteger();

        while (segment != startSegment)
        {
            if (segment is null) ThrowHelper.ThrowArgument($"{nameof(consumed)} is invalid");
            var next = segment!.Next;
            this._pool.Return(segment);
            segment = next;
        }

        this._startSegment = startSegment;
        this._startInteger = startInteger;
    }

    public void Advance(long consumed)
    {
        var segment = this._startSegment;
        var index = this._startInteger;
        var endSegment = this._endSegment;
        var endInteger = this._endInteger;
        while (segment is not null)
        {
            var isEnd = segment == endSegment;
            var capacity = (isEnd ? endInteger : segment.Length) - index;
            if(consumed <= capacity)
            {
                index += (int)consumed;
                break;
            }
            if(isEnd)
            {
                index = endInteger;
                break;
            }
            consumed -= capacity;
            segment = segment.Next;
            index = 0;
        }

        this._startSegment = segment;
        this._startInteger = index;
    }

    public void Dispose() => this._reader.Dispose();
}