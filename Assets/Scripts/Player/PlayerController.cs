﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace Souls
{
    [RequireComponent(typeof(Damageable))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        [Range(0, 100)]
        int attackPoint = 25;

        [SerializeField]
        float moveForce;

        [SerializeField]
        float runMultiplier;

        [SerializeField]
        new CameraController camera;

        [SerializeField]
        Animator anim;

        [SerializeField]
        Stat health;

        [SerializeField]
        Stat stamina;

        [SerializeField]
        Stat potion;

        [SerializeField]
        LayerMask enemyLayer;

        enum MoveState
        {
            Idle,
            Walk,
            Run
        }

        MoveState moveState;

        float regainStaminaDelay;
        float removeStaminaDelay;
        float regainStaminaRate;

        bool isInputAble = false;
        bool isMoveAble = false;
        bool isUseOnlyRootMotionMovement = false;
        bool isUseLockOn = false;
        bool isBeginAttack = false;
        bool isAttack = false;
        bool isNeedPlayDead = false;
        bool isExualted = false;
        bool isConfirmAttack = false;
        bool isGuard = false;

        Damageable damageable;

        Vector2 input;
        Vector2 lastNonZeroInputDir;

        Vector3 velocity;

        Rigidbody rigid;
        Collider[] enemies;


#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector3 checkPosition = transform.position + (transform.forward * 1.8f);
            Gizmos.DrawWireSphere(checkPosition, 2.0f);
            Handles.Label(checkPosition, "Attack range");
        }
#endif

        void Reset()
        {
            moveForce = 22.0f;
            runMultiplier = 1.8f;
            anim = GetComponent<Animator>();
            camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
            var stats = GetComponents<Stat>();
            health = stats[0];
            stamina = stats[1];
        }

        void Awake()
        {
            Initialize();
            SubscribeEvent();
        }

        //Test
        void Start()
        {
            GameController.Instance.GameStart();
        }

        void Update()
        {
            InputHandler();
            AnimationHandler();
        }

        void LateUpdate()
        {
            RotateHandler();
        }

        void FixedUpdate()
        {
            MovementHandler();
            HitEnemyHandler();
        }

        void OnDestroy()
        {
            UnsubscribeEvent();
        }

        void Initialize()
        {
            enemies = new Collider[5];
            damageable = GetComponent<Damageable>();
            rigid = GetComponent<Rigidbody>();
        }

        void InputHandler()
        {
            if (!isMoveAble || !isInputAble) {
                input = Vector2.zero;
                return;
            }

            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            input = input.normalized;

            lastNonZeroInputDir.x = (input.x > 0.0f || input.x < 0.0f) ? input.x : lastNonZeroInputDir.x;
            lastNonZeroInputDir.y = (input.y > 0.0f || input.y < 0.0f) ? input.y : lastNonZeroInputDir.y;

            isGuard = health.IsEmpty ? false : Input.GetKey(KeyCode.Q);

            bool isCanUsePotion = (Input.GetButtonDown("UsePotion") && !potion.IsEmpty && health.Current < health.Max);

            if (isCanUsePotion)
            {
                potion.Remove(1);
                health.FullRestore();
            }

            if (input == Vector2.zero)
            {
                moveState = MoveState.Idle;
            }
            else
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    if (stamina.IsEmpty)
                    {
                        moveState = MoveState.Walk;
                        isExualted = true;
                    }
                    else
                    {
                        if (!isExualted)
                        {
                            moveState = MoveState.Run;
                            regainStaminaDelay = (Time.time + 1.3f);
                            regainStaminaRate = (regainStaminaDelay + 0.5f);
                        }
                    }
                }
                else
                {
                    moveState = MoveState.Walk;
                }
            }

            if (moveState == MoveState.Run)
            {
                if (Time.time > removeStaminaDelay)
                {
                    stamina.Remove(2);
                    removeStaminaDelay = (Time.time + 0.15f);
                }
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                isExualted = false;
            }

            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                if (!isExualted)
                {
                    regainStaminaDelay = (Time.time + 1.3f);
                    regainStaminaRate = (regainStaminaDelay + 0.25f);
                }
            }

            if (isGuard)
            {
                regainStaminaDelay = (Time.time + 1.3f);
                regainStaminaRate = (Time.time + 0.15f);
            }

            if (stamina.Current < stamina.Max)
            {
                if (Time.time > regainStaminaDelay && Time.time > regainStaminaRate)
                {
                    stamina.Restore(5);
                    regainStaminaRate = (Time.time + 0.15f);
                }
            }
        }

        void AnimationHandler()
        {
            bool isRun = moveState == MoveState.Run;
            bool isWalk = (input.x > 0.0f || input.x < 0.0f) || (input.y > 0.0f || input.y < 0.0f);

            anim.SetBool("Walk", isWalk);
            anim.SetBool("Run", moveState == MoveState.Run);
            anim.SetBool("Block", isGuard);

            if (Input.GetButtonDown("Fire1") && !stamina.IsEmpty && stamina.Current >= 14)
            {
                anim.SetTrigger("Slash");
            }

#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.F1) && isMoveAble)
            {
                health.Clear();
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                OnReceiveDamage(15, transform);
            }
#endif

            if (health.IsEmpty && !isNeedPlayDead)
            {
                isNeedPlayDead = true;
                isMoveAble = false;
                isInputAble = false;
                isGuard = false;

                anim.SetBool("Block", false);
                anim.applyRootMotion = true;

                if (Physics.Linecast(transform.position, transform.position + (transform.forward * 2.5f)))
                {
                    anim.SetTrigger("DeadBackward");
                }
                else
                {
                    anim.SetTrigger("Dead");
                }

                GameController.Instance.GameOver();
            }
        }

        void RotateHandler()
        {
            //Manual rotate if there is no specific look at target
            bool isPreventRotation = (!isInputAble || isUseOnlyRootMotionMovement || isGuard || Time.timeScale <= 0.0f || GameController.Instance.State == GameState.Over);

            if (isPreventRotation)
            {
                camera.ForceRotateTarget(false);
                camera.InvertForwardAxis(false);
                return;
            }

            if (input.y > 0.0f || input.y < 0.0f)
            {
                camera.ForceRotateTarget(input.magnitude > 0.2f);
                camera.InvertForwardAxis(lastNonZeroInputDir.y < 0.0f);
            }
            else if (input.x > 0.0f || input.x < 0.0f)
            {
                camera.ForceRotateTarget(false);
                camera.InvertForwardAxis(false);

                var targetRotationAxis = (lastNonZeroInputDir.x > 0.0f) ? camera.transform.right : -camera.transform.right;
                targetRotationAxis.y = 0.0f;

                var targetRotation = Quaternion.LookRotation(targetRotationAxis);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.2f);
            }
            else
            {
                camera.ForceRotateTarget(false);
                camera.InvertForwardAxis(false);
            }
        }

        void MovementHandler()
        {
            if (!isMoveAble || isGuard)
            {
                velocity = Vector3.zero;
                rigid.velocity = velocity;
                return;
            }

            if (isUseOnlyRootMotionMovement)
            {
                return;
            }

            float moveForce = this.moveForce;
            moveForce = (moveState == MoveState.Run) ? (moveForce * runMultiplier) : moveForce;

            //Flip forward dir, make camera handle rotation
            if (camera.CameraState == CameraController.State.Normal)
            {
                if (input.y > 0.0f || input.y < 0.0f)
                {
                    if (lastNonZeroInputDir.y < 0.0f)
                    {
                        velocity = ((transform.forward * Mathf.Abs(input.y)) + (transform.right * (input.x * lastNonZeroInputDir.y))).normalized * moveForce;
                    }
                    else
                    {
                        velocity = ((transform.forward * Mathf.Abs(input.y)) + (transform.right * input.x)).normalized * moveForce;
                    }
                }
                else
                {
                    //If player is not intent to move forward, apply force by input right/left axis
                    velocity = transform.forward * Mathf.Abs(input.x) * moveForce;
                }
            }
            //Lock on camera will always make forward axis face its target, no need to flip dir
            else if (camera.CameraState == CameraController.State.LockOn)
            {
                velocity = ((transform.forward * input.y) + (transform.right * input.x)).normalized * moveForce;
            }

            rigid.AddForce(velocity, ForceMode.Force);
        }

        void HitEnemyHandler()
        {
            if (isConfirmAttack)
            {
                Vector3 checkPosition = rigid.position + (transform.forward * 1.8f);
                int hitCount = Physics.OverlapSphereNonAlloc(checkPosition, 2.0f, enemies, enemyLayer);

                if (hitCount <= 0)
                {
                    isConfirmAttack = false;
                    return;
                }

                //if not focus on any enemy, find target here..
                for (int i = 0; i < hitCount; ++i)
                {
                    Debug.Log("Detect enemy : " + enemies[i].gameObject.name);
                    var damageable = enemies[i].gameObject.GetComponent<Damageable>();
                    damageable?.ReceiveDamage(attackPoint, transform);
                }

                isConfirmAttack = false;
            }
        }

        void SubscribeEvent()
        {
            damageable.OnReceiveDamage += OnReceiveDamage;
            GameController.Instance.OnGameStateChange += OnGameStateChange;
        }

        void UnsubscribeEvent()
        {
            damageable.OnReceiveDamage -= OnReceiveDamage;
            GameController.Instance.OnGameStateChange -= OnGameStateChange;
        }

        void OnReceiveDamage(int value, Transform other)
        {
            Vector3 forward = transform.forward;
            Vector3 toOther = (other.position - transform.position).normalized;

            float result = Vector3.Dot(forward, toOther);
            bool isReceiveAttackOnTheBack = (result < 0.0f);

            if (isGuard && !isReceiveAttackOnTheBack)
            {
                int currentStamina = stamina.Current;
                int damage = (int)(value * 0.7f);
                int totalHit = (currentStamina - damage);

                if (totalHit < 0)
                {
                    regainStaminaDelay = (Time.time + 0.5f);
                    regainStaminaRate = (regainStaminaDelay + 0.25f);

                    stamina.Clear();
                    health.Remove(Mathf.Abs(totalHit));

                    if (!health.IsEmpty)
                    {
                        anim.SetTrigger("Hit");
                    }
                }
                else
                {
                    regainStaminaDelay = (Time.time + 0.5f);
                    regainStaminaRate = (regainStaminaDelay + 0.25f);

                    stamina.Remove(damage);
                }
            }
            else
            {
                health.Remove(value);

                if (!health.IsEmpty)
                {
                    anim.SetTrigger("Hit");
                }
            }
        }

        void OnGameStateChange(GameState state)
        {
            switch (state)
            {
                case GameState.Start:
                {
                    isMoveAble = true;
                    isInputAble = true;
                    Debug.Log("Start");
                }
                break;

                case GameState.Over:
                {
                    isMoveAble = false;
                    isInputAble = false;
                    Debug.Log("Stop");
                }
                break;

                default:
                    break;
            }
        }

        public void BeginAttackEvent()
        {
            regainStaminaDelay = (Time.time + 1.3f);
            regainStaminaRate = (regainStaminaDelay + 0.5f);
            stamina.Remove(14);
        }

        public void EndAttackEvent()
        {
            isBeginAttack = false;
            isAttack = false;
        }

        public void DoDamage()
        {
            isConfirmAttack = true;
        }

        public void PreventPlayerControl()
        {
            BeginAttackEvent();
            isInputAble = false;
            /* anim.applyRootMotion = true; */
        }

        public void PreventPlayerMovement()
        {
            isUseOnlyRootMotionMovement = true;
        }

        public void RegainPlayerControl()
        {
            isInputAble = true;
            /* anim.applyRootMotion = false; */
        }

        public void RegainPlayerMovement()
        {
            isUseOnlyRootMotionMovement = false;
        }
    }
}

