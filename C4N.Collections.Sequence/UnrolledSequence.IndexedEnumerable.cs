using System;
using System.Collections;
using System.Collections.Generic;

namespace C4N.Collections.Sequence;

public readonly partial struct UnrolledSequence<T>
{
    public readonly struct IndexedEnumerable : IEnumerable<(ReadOnlyMemory<T> Memory, long TotalIndex)>
    {
        public struct Enumerator : IEnumerator<(ReadOnlyMemory<T> Memory, long TotalIndex)>
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

            public (ReadOnlyMemory<T> Memory, long TotalIndex) Current
            {
                get
                {
                    var (segment, from, to) = (this.segment!, this.from, this.to);
                    return (segment.GetMemory(from, to - from), segment.TotalIndex + from);
                }
            }

            object IEnumerator.Current => this.Current;

            public bool MoveNext()
            {
                (this.segment, this.from) = this.segment is null
                                            ? (this.sequence._headSegment, this.sequence._headInteger)
                                            : (this.segment.Next, 0);
                this.to = (this.segment == this.sequence._tailSegment) ? this.sequence._tailInteger : (this.segment?.Length ?? 0);

                return this.segment is not null;
            }
            public void Dispose() { }

            public void Reset() => throw new NotSupportedException();
        }

        public IndexedEnumerable(UnrolledSequence<T> sequence) => this.sequence = sequence;

        readonly UnrolledSequence<T> sequence;

        public Enumerator GetEnumerator() => new(this.sequence);
        IEnumerator<(ReadOnlyMemory<T> Memory, long TotalIndex)> IEnumerable<(ReadOnlyMemory<T> Memory, long TotalIndex)>.GetEnumerator() => this.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}