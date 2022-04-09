using System;

namespace C4N.Collections.Sequence;

public readonly partial struct UnrolledSequence<T>
{
    public readonly struct ElementEnumerable
    {
        public ref struct Enumerator
        {
            public Enumerator(UnrolledSequence<T> sequence)
            {
                this.bufferEnumerator = sequence.EnumerateBuffer().GetEnumerator();
                this.bufferEnumerator.MoveNext();
                this.enumerator = this.bufferEnumerator.Current.GetEnumerator();
            }
            UnrolledSequence<T>.BufferEnumerable.Enumerator bufferEnumerator;
            ReadOnlySpan<T>.Enumerator enumerator;
            public T Current => this.enumerator.Current;

            public bool MoveNext()
            {
                if (this.enumerator.MoveNext()) return true;
                if (!this.bufferEnumerator.MoveNext()) return false;
                this.enumerator = this.bufferEnumerator.Current.GetEnumerator();
                return this.MoveNext();
            }
            public void Dispose() { }
            public void Reset() => throw new NotSupportedException();
        }

        public ElementEnumerable(UnrolledSequence<T> sequence)
        {
            this.sequence = sequence;
        }

        readonly UnrolledSequence<T> sequence;
        public Enumerator GetEnumerator() => new(this.sequence);
    }
}