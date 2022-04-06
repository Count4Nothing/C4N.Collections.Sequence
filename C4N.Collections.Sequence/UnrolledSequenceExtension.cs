using System;
using System.Collections.Generic;
using System.Text;

namespace C4N.Collections.Sequence;
public static class UnrolledSequenceExtension
{
    public static bool StartsWith<T>(this UnrolledSequence<T> sequence, ReadOnlySpan<T> span)
        where T : IEquatable<T>
    {
        if (span.IsEmpty) return true;
        if (sequence.IsEmpty) return false;
        foreach (var buffer in sequence.EnumerateBuffer())
        {
            if (buffer.Length >= span.Length) return buffer.StartsWith(span);
            if (!span.StartsWith(buffer)) return false;
            span = span.Slice(buffer.Length);
        }
        return false;
    }

    public static bool StartsWith(this UnrolledSequence<char> sequence, ReadOnlySpan<char> span, StringComparison comparison)
    {
        if (span.IsEmpty) return true;
        if (sequence.IsEmpty) return false;
        foreach (var buffer in sequence.EnumerateBuffer())
        {
            if (buffer.Length >= span.Length) return buffer.StartsWith(span, comparison);
            if (!span.StartsWith(buffer, comparison)) return false;
            span = span.Slice(buffer.Length);
        }
        return false;
    }

    public static bool StartsWith(this UnrolledSequence<char> sequence, string str, StringComparison comparison)
    {
        return sequence.StartsWith(str.AsSpan(), comparison);
    }
}
