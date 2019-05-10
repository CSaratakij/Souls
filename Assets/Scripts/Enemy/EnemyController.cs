using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
    using UnityEditor;
#endif

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

        [SerializeField]
        Transform[] points;

        enum State
        {
            Idle,
            RunAround,
            RunToTarget,
            Attack,
            Dead
        }

        int currentPointIndex;
        float nextPointDelay;

        bool previousReach;
        bool isFoundTarget;

        Animator anim;
        Collider collider;

        Transform target;
        Transform other;

        Damageable damageable;
        Rigidbody rigid;
        State currentState;

        NavMeshAgent navMeshAgent;


#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            for (int i = 0; i < points.Length; i++)
            {
                if (points[i] == null) {
                    Debug.Log("Missing points");
                    continue;
                }

                Handles.Label(points[i].position, "Point" + i);
                Gizmos.DrawSphere(points[i].position, 0.5f);

                if (i + 1 >= points.Length)
                    break;

                Gizmos.DrawLine(points[i].position, points[i + 1].position);
            }
        }
#endif

        void Awake()
        {
            Initialize();
            SubscribeEvent();
        }

        void Update()
        {
            StateHandler();
        }

        void FixedUpdate()
        {
            /* MovementHandler(); */
        }

        void OnDestroy()
        {
            UnsubscribeEvent();
        }

        void Initialize()
        {
            target = Global.Player;
            anim = GetComponent<Animator>();
            collider = GetComponent<Collider>();
            damageable = GetComponent<Damageable>();
            rigid = GetComponent<Rigidbody>();
            navMeshAgent = GetComponent<NavMeshAgent>();

            if (points.Length > 1)
            {
                transform.position = points[0].position;

                navMeshAgent.SetDestination(points[0].position);
                navMeshAgent.acceleration = 3.5f;

                currentState = State.RunAround;
                nextPointDelay = Time.time + 3.0f;
            }
        }

        void StateHandler()
        {
            SeekTarget();
            RunAroundHandler();
            RunToTargetHandler();
            AttackHandler();
        }

        void SeekTarget()
        {
            if (isFoundTarget)
                return;

            Vector3 relativeVector = (target.position - transform.position);

            if (relativeVector.magnitude >= 5.5f)
                return;

            Vector3 forward = transform.forward;
            Vector3 toOther = relativeVector.normalized;

            float result = Vector3.Dot(forward, toOther);

            if (result > 0.0f)
            {
                isFoundTarget = true;
                navMeshAgent.acceleration = 6.0f;
                currentState = State.RunToTarget;
            }
        }

        void RunAroundHandler()
        {
            switch (currentState)
            {
                case State.RunAround:
                {
                    Vector3 currentPoint = points[currentPointIndex].position;
                    Vector3 relativeVector = (currentPoint - rigid.position);

                    bool isReach = (relativeVector.magnitude <= 0.25f);

                    if (isReach && !previousReach)
                    {
                        previousReach = true;
                        nextPointDelay = Time.time + 3.0f;
                        anim.SetBool("Run", false);
                    }

                    if (previousReach && Time.time > nextPointDelay)
                    {
                        int previousIndex = currentPointIndex;
                        currentPointIndex = (currentPointIndex + 1) >= points.Length ? 0 : (currentPointIndex + 1);

                        if (previousIndex != currentPointIndex)
                        {
                            anim.SetBool("Run", true);
                            navMeshAgent.SetDestination(points[currentPointIndex].position);
                        }

                        previousReach = false;
                    }
                }
                break;

                default:
                    break;
            }
        }

        void RunToTargetHandler()
        {
            switch (currentState)
            {
                case State.RunToTarget:
                {
                    Vector3 currentPoint = transform.position;
                    Vector3 relativeVector = (target.position - currentPoint);

                    bool isReach = (relativeVector.magnitude <= 3.0f);

                    if (!isReach)
                    {
                        anim.SetBool("Run", true);
                        navMeshAgent.SetDestination(target.position);
                    }
                    else
                    {
                        //Todo
                        //change to attack state here..
                        anim.SetBool("Run", false);
                        currentState = State.Attack;
                    }
                }
                break;

                default:
                    break;
            }
        }

        void AttackHandler()
        {
            switch (currentState)
            {
                case State.Attack:
                {
                    Debug.Log("Attack State..");
                }
                break;

                default:
                    break;
            }
        }

        void SubscribeEvent()
        {
            damageable.OnReceiveDamage += OnReceiveDamage;
        }

        void UnsubscribeEvent()
        {
            damageable.OnReceiveDamage -= OnReceiveDamage;
        }

        void OnReceiveDamage(int value, Transform other)
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

                rigid.isKinematic = true;
                collider.enabled = false;
                navMeshAgent.enabled = false;

                bool isDyingBackward = (Physics.Linecast(transform.position, transform.position + (transform.forward * 5.0f)));

                if (isDyingBackward)
                {
                    anim.SetTrigger("DeadBackward");
                }
                else
                {
                    anim.SetTrigger("Dead");
                }

                Debug.Log("Dead...");
            }
            else
            {
                if (currentState != State.Dead)
                {
                    anim.SetTrigger("Hit");
                    this.other = other;
                }
            }
        }

        public void OnHitEnd()
        {
            if (other.Equals(target) && !isFoundTarget)
            {
                isFoundTarget = true;
                currentState = State.RunToTarget;
            }
        }
    }
}

