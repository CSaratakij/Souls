using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Souls
{
    [RequireComponent(typeof(Damageable))]
    public class EnemyController : MonoBehaviour
    {
        [SerializeField]
        Stat health;

        [SerializeField]
        Stat stamina;

        Damageable damageable;


        void Awake()
        {
            Initialize();
            SubscribeEvent();
        }

        void OnDestroy()
        {
            UnsubscribeEvent();
        }

        void Initialize()
        {
            damageable = GetComponent<Damageable>();
        }

        void SubscribeEvent()
        {
            damageable.OnReceiveDamage += OnReceiveDamage;
        }

        void UnsubscribeEvent()
        {
            damageable.OnReceiveDamage -= OnReceiveDamage;
        }

        void OnReceiveDamage(int value)
        {
            health.Remove(value);

            if (health.IsEmpty)
            {
                gameObject.SetActive(false);
                Debug.Log("Dead...");
            }
        }
    }
}

