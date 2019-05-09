using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Souls
{
    public class Damageable : MonoBehaviour
    {
        public event Action<int, Transform> OnReceiveDamage;

        void OnDestroy()
        {
            OnReceiveDamage = null;
        }

        public void ReceiveDamage(int value, Transform transform)
        {
            OnReceiveDamage?.Invoke(value, transform);
        }
    }
}

