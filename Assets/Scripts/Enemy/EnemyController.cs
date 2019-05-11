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
        int attackPoint = 22;

        [SerializeField]
        float attackRate = 0.8f;

        [SerializeField]
        float detectDistance = 5.5f;

        [SerializeField]
        Stat health;

        [SerializeField]
        Stat stamina;

        [SerializeField]
        Transform[] points;

        [SerializeField]
        LayerMask enemyLayer;

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
        float nextAttackDelay;

        bool previousReach;
        bool isFoundTarget;
        bool isNeedAttack;
        bool isConfirmAttack;
        bool isRestFromAttack;
        bool isAllowChangeState = true;

        Collider[] enemies;

        Animator anim;
        Collider collider;

        Transform target;
        Transform other;

        Vector3 origin;

        Damageable damageable;
        Rigidbody rigid;
        State currentState;

        NavMeshAgent navMeshAgent;


#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            DrawPoints();
            DrawAttackArea();
        }

        void DrawPoints()
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

        void DrawAttackArea()
        {
            Gizmos.color = Color.red;
            Vector3 checkPosition = transform.position + (transform.forward * 1.4f);
            Gizmos.DrawWireSphere(checkPosition + Vector3.up, 1.5f);
            Handles.Label(checkPosition, "Attack range");
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
            RotateHandler();
        }

        void FixedUpdate()
        {
            HitEnemyHandler();
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

            enemies = new Collider[5];

            if (points.Length > 1)
            {
                transform.position = points[0].position;

                navMeshAgent.SetDestination(points[0].position);
                navMeshAgent.acceleration = 3.5f;

                currentState = State.RunAround;
                nextPointDelay = Time.time + 3.0f;
            }

            origin = transform.position;
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

            if (relativeVector.magnitude >= detectDistance)
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
                    Vector3 currentPoint = points.Length > 0 ? points[currentPointIndex].position : origin;
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
                            navMeshAgent.SetDestination(points.Length > 0 ? points[currentPointIndex].position : origin);
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
                    bool isTargetTooFarAway = (relativeVector.magnitude >= 35.0f);

                    Debug.Log(isTargetTooFarAway);

                    if (!isReach)
                    {
                        if (isTargetTooFarAway)
                        {
                            Debug.Log("Too far aways");

                            navMeshAgent.acceleration = 3.5f;
                            currentPointIndex = 0;
                            navMeshAgent.SetDestination(points.Length > 0 ? points[0].position : origin);

                            nextPointDelay = Time.time + 3.0f;
                            isFoundTarget = false;

                            currentState = State.RunAround;
                        }
                        else
                        {
                            Debug.Log("Not far aways");

                            anim.SetBool("Run", true);
                            navMeshAgent.SetDestination(target.position);
                        }
                    }
                    else
                    {
                        anim.SetBool("Run", false);

                        currentState = State.Attack;
                        nextAttackDelay = Time.time + (attackRate * 0.5f);
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
                    if (health.IsEmpty)
                    {
                        return;
                    }

                    Debug.Log("Attack State..");

                    Vector3 currentPoint = transform.position;
                    Vector3 relativeVector = (target.position - currentPoint);

                    bool isTargetFarFromAttackRange = (relativeVector.magnitude >= 3.5f);

                    if (isTargetFarFromAttackRange)
                    {
                        isFoundTarget = true;

                        isConfirmAttack = false;
                        isNeedAttack = false;
                        nextAttackDelay = Time.time + attackRate;

                        navMeshAgent.isStopped = false;
                        navMeshAgent.acceleration = 6.0f;

                        currentState = State.RunToTarget;
                    }
                    else
                    {
                        if (Time.time > nextAttackDelay && !isNeedAttack)
                        {
                            isNeedAttack = true;
                            navMeshAgent.isStopped = true;
                            anim.SetTrigger("Attack");
                        }
                    }
                }
                break;

                default:
                    break;
            }
        }

        void RotateHandler()
        {
            if (currentState == State.Attack)
            {
                Vector3 lookDirection = (target.position - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.025f);
            }
        }

        void HitEnemyHandler()
        {
            if (health.IsEmpty)
            {
                return;
            }

            if (isConfirmAttack)
            {
                Vector3 checkPosition = rigid.position + (transform.forward * 1.4f);
                int hitCount = Physics.OverlapSphereNonAlloc(checkPosition + Vector3.up, 1.5f, enemies, enemyLayer);

                if (hitCount <= 0)
                {
                    isConfirmAttack = false;
                    isNeedAttack = false;
                    navMeshAgent.isStopped = false;
                    nextAttackDelay = Time.time + attackRate;
                    return;
                }

                Debug.Log("Check");

                for (int i = 0; i < hitCount; ++i)
                {
                    Debug.Log("Detect enemy : " + enemies[i].gameObject.name);
                    var damageable = enemies[i].gameObject.GetComponent<Damageable>();
                    damageable?.ReceiveDamage(attackPoint, transform);
                }

                isConfirmAttack = false;
                isNeedAttack = false;
                navMeshAgent.isStopped = false;
                nextAttackDelay = Time.time + attackRate;
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

                    isNeedAttack = false;
                    nextAttackDelay = Time.time + (attackRate * 0.8f);
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

            isRestFromAttack = true;
        }

        public void DoDamage()
        {
            isConfirmAttack = true;
        }

        public void PreventChangeState()
        {
            isAllowChangeState = false;
        }

        public void OnAttackEnd()
        {
            isRestFromAttack = true;
        }

        public void AllowChangeState()
        {
            isAllowChangeState = true;
        }
    }
}

