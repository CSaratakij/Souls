using System;

namespace Souls
{
    public interface IStat<T>
    {
        event Action<T> OnValueChanged;
        event Action<T> OnMaxValueChanged;

        T Current { get; }
        T Max { get; }
        bool IsEmpty { get; }

        void Restore(T value);
        void Remove(T value);
        void FullRestore();
        void Clear();
        void AddMax(T value);
        void RemoveMax(T value);
    }
}

