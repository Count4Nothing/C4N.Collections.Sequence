namespace C4N.Collections.Sequence;

public struct ReadResult<T>
{
    public ReadResult(UnrolledSequence<T> buffer, bool completed)
    {
        this.Buffer = buffer;
        this.IsCompleted = completed;
    }

    public UnrolledSequence<T> Buffer { get; }
    public bool IsCompleted { get; }
}
