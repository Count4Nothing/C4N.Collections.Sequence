using System;

namespace C4N.Collections.Sequence;

public abstract class UnrolledSequenceSource<T> : IDisposable
{
    public ReadResult<T> Read() => this.Read(0);
    public abstract ReadResult<T> Read(int sizeHint);
    public abstract void Advance(long consumed);
    public abstract void Advance(SequencePosition consumed);
    public abstract void Dispose();
}
