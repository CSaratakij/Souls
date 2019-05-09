using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Souls
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Damageable))]
    public class EnemyController : MonoBehaviour
    {
        [SerializeField]
        Stat health;

        [SerializeField]
        Stat stamina;

        enum State
        {
            Idle,
            Walk,
            Attack,
            Dead
        }

        Animator anim;
        Collider collider;

        Damageable damageable;
        State currentState;

        Rigidbody rigid;


        void Awake()
        {
            Initialize();
            SubscribeEvent();
        }

        void Update()
        {
            AnimationHandler();
        }

        void OnDestroy()
        {
            UnsubscribeEvent();
        }

        void Initialize()
        {
            anim = GetComponent<Animator>();
            collider = GetComponent<Collider>();
            damageable = GetComponent<Damageable>();
            rigid = GetComponent<Rigidbody>();
        }

        void AnimationHandler()
        {
        }

        void SubscribeEvent()
        {
            damageable.OnReceiveDamage += OnReceiveDamage;
        }

        void UnsubscribeEvent()
        {
            damageable.OnReceiveDamage -= OnReceiveDamage;
        }

        void OnReceiveDamage(int value, Transform transform)
        {
            if (currentState == State.Dead)
            {
                return;
            }

            health.Remove(value);

            if (health.IsEmpty && currentState != State.Dead)
            {
                currentState = State.Dead;

                anim.applyRootMotion = true;
                anim.SetTrigger("Dead");

                rigid.isKinematic = true;
                collider.enabled = false;

                Debug.Log("Dead...");
            }
            else
            {
                if (currentState != State.Dead)
                {
                    anim.SetTrigger("Hit");
                }
            }
        }
    }
}

