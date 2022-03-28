namespace C4N.Collections.Sequence;

public partial class TextPipeReader
{
    struct SegmentPool
    {
        public SegmentPool(int unitMin, int unitMax)
        {
            this.UnitMin = unitMin;
            this.UnitMax = unitMax;
            this._freeList = null;
        }

        UnrolledSequenceSegment<char>? _freeList;

        public int UnitMin { get; }
        public int UnitMax { get; }

        int CalcCapacity(int size)
        {
            if (size < this.UnitMin) return this.UnitMin;
            if (size > this.UnitMax) return this.UnitMax;
            return size;
        }

        public UnrolledSequenceSegment<char> Rent(int min)
        {
            var head = this._freeList;
            head ??= new UnrolledSequenceSegment<char>(this.CalcCapacity(min));
            this._freeList = head.Next;
            return head;
        }

        public void Return(UnrolledSequenceSegment<char> segment)
        {
            segment.Next = this._freeList;
            this._freeList = segment;
        }
    }
}