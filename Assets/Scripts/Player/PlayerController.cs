using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Souls
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        float moveForce;

        [SerializeField]
        float runMultiplier;

        [SerializeField]
        new CameraController camera;

        [SerializeField]
        Animator anim;

        enum MoveState
        {
            Idle,
            Walk,
            Run
        }

        MoveState moveState;

        bool isMoveAble = false;
        bool isUseLockOn = false;

        Vector2 input;
        Vector2 lastNonZeroInputDir;

        Vector3 velocity;
        Rigidbody rigid;

        void Reset()
        {
            moveForce = 30.0f;
            camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
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
        }

        void OnDestroy()
        {
            UnsubscribeEvent();
        }

        void Initialize()
        {
            rigid = GetComponent<Rigidbody>();
        }

        void InputHandler()
        {
            if (!isMoveAble) {
                input = Vector2.zero;
                return;
            }

            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            input = input.normalized;

            lastNonZeroInputDir.x = (input.x > 0.0f || input.x < 0.0f) ? input.x : lastNonZeroInputDir.x;
            lastNonZeroInputDir.y = (input.y > 0.0f || input.y < 0.0f) ? input.y : lastNonZeroInputDir.y;

            if (input == Vector2.zero)
            {
                moveState = MoveState.Idle;
            }
            else
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    moveState = MoveState.Run;
                }
                else
                {
                    moveState = MoveState.Walk;
                }
            }

        }

        void AnimationHandler()
        {
            bool isWalk = (input.x > 0.0f || input.x < 0.0f) || (input.y > 0.0f || input.y < 0.0f);
            anim.SetBool("Walk", isWalk);
            anim.SetBool("Run", moveState == MoveState.Run);
        }

        void RotateHandler()
        {
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
            if (!isMoveAble)
            {
                velocity = Vector3.zero;
                rigid.velocity = velocity;
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

        void SubscribeEvent()
        {
            GameController.Instance.OnGameStateChange += OnGameStateChange;
        }

        void UnsubscribeEvent()
        {
            GameController.Instance.OnGameStateChange -= OnGameStateChange;
        }

        void OnGameStateChange(GameState state)
        {
            switch (state)
            {
                case GameState.Start:
                {
                    isMoveAble = true;
                    Debug.Log("Start");
                }
                break;

                case GameState.Over:
                {
                    isMoveAble = false;
                    Debug.Log("Stop");
                }
                break;

                default:
                    break;
            }
        }
    }
}

