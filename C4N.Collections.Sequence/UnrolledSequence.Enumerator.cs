using System;
using System.Collections;
using System.Collections.Generic;

namespace C4N.Collections.Sequence;

public readonly partial struct UnrolledSequence<T>
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

        public ReadOnlyMemory<T> Current => this.segment!.GetMemory(this.from, this.to - this.from);
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
}