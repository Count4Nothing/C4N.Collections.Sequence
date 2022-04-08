using System;

namespace C4N;

static class Throw
{
    public static void Argument(string message) => throw new ArgumentException(message);
    public static void ArgumentOutOfRange(string message) => throw new ArgumentOutOfRangeException(message);
    public static void NullReference(string message) => throw new NullReferenceException(message);
    public static void IndexOutOfRange(string message) => throw new IndexOutOfRangeException(message);
}