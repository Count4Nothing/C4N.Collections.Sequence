namespace C4N.Collections.Sequence;

public class UnrolledSequenceSegment<T>
{
    private UnrolledSequenceSegment<T>? next;

    public int Length => this.Array.Length;

    internal UnrolledSequenceSegment<T>? Next
    {
        get => this.next;
        set
        {
            this.next = value;
            if (value is null) return;
            value.TotalIndex = this.TotalIndex + this.Length;
        }
    }
    public long TotalIndex { get; private set; }

    public T[] Array { get; }

    public UnrolledSequenceSegment(T[] array)
    {
        this.Array = array;
    }
    public UnrolledSequenceSegment(int capacity)
    {
        this.Array = new T[capacity];
    }
}
