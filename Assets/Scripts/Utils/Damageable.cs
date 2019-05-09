using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Souls
{
    public class Damageable : MonoBehaviour
    {
        public event Action<int> OnReceiveDamage;

        void OnDestroy()
        {
            OnReceiveDamage = null;
        }

        public void ReceiveDamage(int value)
        {
            OnReceiveDamage?.Invoke(value);
        }
    }
}

