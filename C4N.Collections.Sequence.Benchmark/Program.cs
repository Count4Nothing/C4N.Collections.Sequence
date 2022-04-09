using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using C4N.Collections.Sequence;

BenchmarkRunner.Run<SequenceBenchmark>();

public class SequenceBenchmark
{
    UnrolledSequence<char> charSequence;

    public SequenceBenchmark()
    {
        this.charSequence = GenerateCharSequence(0xFFF, 0xFFFF);
    }

    static UnrolledSequence<char> GenerateCharSequence(int unit, int count)
    {
        const string chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        var root = default(UnrolledSequenceSegment<char>);
        var prev = root;
        foreach (var segmentChars in Enumerable.Repeat(chars, (count / chars.Length) + 1).SelectMany(s => s).Chunk(unit))
        {
            var segment = new ArrayUnrolledSequenceSegment<char>(segmentChars);
            if (prev is not null) prev.Next = segment;
            if (root is null) root = segment;
            prev = segment;
        }

        return new UnrolledSequence<char>(root, 0, prev, prev.Length);
    }

    [Benchmark]
    public char EnumerateElement()
    {
        var chara = '\0';
        foreach (var element in this.charSequence.EnumerateElement())
        {
            chara = element;
        }
        return chara;
    }

    [Benchmark]
    public char EnumerateBuffer()
    {
        var chara = '\0';
        foreach (var span in this.charSequence.EnumerateBuffer())
        {
            foreach (var element in span)
            {
                chara = element;
            }
        }
        return chara;
    }

    [Benchmark]
    public char Enumerate()
    {
        var chara = '\0';
        foreach (var memory in this.charSequence)
        {
            foreach (var element in memory.Span)
            {
                chara = element;
            }
        }
        return chara;
    }

    [Benchmark]
    public char EnumerateSegment()
    {
        var chara = '\0';
        foreach (var segment in this.charSequence.EnumerateSegment())
        {
            foreach (var element in segment.Span)
            {
                chara = element;
            }
        }
        return chara;
    }
}