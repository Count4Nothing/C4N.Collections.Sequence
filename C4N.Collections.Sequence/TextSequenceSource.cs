using System;
using System.IO;

namespace C4N.Collections.Sequence;

public sealed partial class TextSequenceSource : UnrolledSequenceSource<char>
{
    public TextSequenceSource(TextReader reader)
    {
        var (unitMin, unitMax) = UnrolledSequenceSource.RecommendedBufferSizeOf<char>();
        this._reader = reader;
        this._pool = new(unitMin, unitMax);
        this._headSegment = this._tailSegment = null;
        this._headInteger = this._tailInteger = default;
    }

    readonly TextReader _reader;
    SegmentPool _pool;
    ArrayUnrolledSequenceSegment<char>? _headSegment;
    int _headInteger;
    ArrayUnrolledSequenceSegment<char>? _tailSegment;
    int _tailInteger;

    long Buffered
    {
        get
        {
            var headSegment = this._headSegment;
            var tailSegment = this._tailSegment;
            var headIndex = this._headInteger;
            var tailIndex = this._tailInteger;
            return (tailSegment?.TotalIndex - headSegment?.TotalIndex + (tailIndex - headIndex)) ?? 0;
        }
    }
    SequencePosition HeadPosition => new(this._headSegment, this._headInteger);
    SequencePosition TailPosition => new(this._tailSegment, this._tailInteger);
    bool IsEnd => this._tailSegment?.Length > this._tailInteger;

    ReadResult<char> ReadBuffer() => new(new(this.HeadPosition, this.TailPosition), this.IsEnd);
    void LoadBuffer(int sizeHint)
    {
        if (this.IsEnd) return;
        var headSegment = this._headSegment;
        var tailSegment = this._tailSegment;
        var headIndex = this._headInteger;
        var tailIndex = this._tailInteger;
        var buffered = this.Buffered;
        var pool = this._pool;

        if (buffered <= 0) sizeHint = Math.Max(pool.UnitMin, sizeHint);
        if (sizeHint <= buffered) return;
        sizeHint -= (int)buffered;

        if (headSegment is null)//when initial load
        {
            headSegment = tailSegment = this._pool.Rent(sizeHint);
            headIndex = tailIndex = 0;
        }
        var reader = this._reader;

        while (true)
        {
            var capacity = tailSegment!.Length - tailIndex;
            var written = reader.Read(tailSegment.Array, tailIndex, capacity);
            tailIndex += Math.Max(0, written);
            if (written < capacity) break;
            sizeHint -= written;
            if (sizeHint <= 0) break;
            var segment = pool.Rent(sizeHint);
            tailSegment.Next = segment;
            tailSegment = segment;
            tailIndex = 0;
        }

        this._headSegment = headSegment;
        this._headInteger = headIndex;
        this._tailSegment = tailSegment;
        this._tailInteger = tailIndex;
    }


    public override ReadResult<char> Read(int sizeHint)
    {
        this.LoadBuffer(sizeHint);
        return this.ReadBuffer();
    }
    public override void Advance(SequencePosition consumed)
    {
        var headSegment = this._headSegment;
        var consumedSegment = consumed.GetObject() as UnrolledSequenceSegment<char>;
        var consumedInteger = consumed.GetInteger();

        while (headSegment != consumedSegment)
        {
            if (headSegment is null) Throw.Argument($"{nameof(consumed)} is invalid");
            var next = headSegment!.Next;
            this._pool.Return(headSegment);
            headSegment = next as ArrayUnrolledSequenceSegment<char>;
        }

        this._headSegment = consumedSegment as ArrayUnrolledSequenceSegment<char>;
        this._headInteger = consumedInteger;
    }

    public override void Advance(long consumed)
    {
        var segment = this._headSegment;
        var index = this._headInteger;
        var tailSegment = this._tailSegment;
        var tailInteger = this._tailInteger;
        while (segment is not null)
        {
            var isEnd = segment == tailSegment;
            var capacity = (isEnd ? tailInteger : segment.Length) - index;
            if(consumed <= capacity)
            {
                index += (int)consumed;
                break;
            }
            if(isEnd)
            {
                index = tailInteger;
                break;
            }
            consumed -= capacity;
            segment = segment.Next as ArrayUnrolledSequenceSegment<char>;
            index = 0;
        }

        this._headSegment = segment;
        this._headInteger = index;
    }

    public override void Dispose() => this._reader.Dispose();
}