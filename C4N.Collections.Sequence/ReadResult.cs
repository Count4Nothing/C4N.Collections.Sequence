namespace C4N.Collections.Sequence;

public struct ReadResult
{
    public ReadResult(UnrolledSequence<char> buffer, bool completed)
    {
        this.Buffer = buffer;
        this.IsCompleted = completed;
    }

    public UnrolledSequence<char> Buffer { get; }
    public bool IsCompleted { get; }
}
