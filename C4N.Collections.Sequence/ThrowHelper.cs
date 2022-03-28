using System;

namespace C4N;

static class ThrowHelper
{
    public static void ThrowArgument(string message) => throw new ArgumentException(message);
}