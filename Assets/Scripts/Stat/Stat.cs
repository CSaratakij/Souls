using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Souls
{
    public class Stat : MonoBehaviour, IStat<int>
    {
        [SerializeField]
        int current = 0;

        [SerializeField]
        int max = 0;

        public event Action<int> OnValueChanged;
        public event Action<int> OnMaxValueChanged;

        public int Current => current;
        public int Max => max;
        public bool IsEmpty => (Current <= 0);


        public void Restore(int value)
        {
            current = (current + value) > max ? max : (current + value);
            OnValueChanged?.Invoke(current);
        }

        public void Remove(int value)
        {
            current = (current - value) < 0 ? 0 : (current - value);
            OnValueChanged?.Invoke(current);
        }

        public void FullRestore()
        {
            current = max;
            OnValueChanged?.Invoke(current);
        }

        public void Clear()
        {
            current = 0;
            OnValueChanged?.Invoke(current);
        }

        public void AddMax(int value)
        {
            max += value;
            OnMaxValueChanged?.Invoke(max);
        }

        public void RemoveMax(int value)
        {
            max = (max - value) < 0 ? 0 : (max - value);
            OnMaxValueChanged?.Invoke(max);
        }
    }
}

