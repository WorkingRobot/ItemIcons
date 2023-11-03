using System;
using System.Collections.Generic;

namespace ItemIcons.Config;

public static class StackRaii
{
    private readonly struct Raii<T> : IDisposable
    {
        public Stack<T> Stack { get; }

        public Raii(Stack<T> stack, T item)
        {
            Stack = stack;
            Stack.Push(item);
        }

        public void Dispose()
        {
            Stack.Pop();
        }
    }

    public static IDisposable PushRaii<T>(this Stack<T> stack, T item) =>
        new Raii<T>(stack, item);
}
