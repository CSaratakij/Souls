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
        CameraController camera;


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
        }

        void LateUpdate()
        {
            CameraHandler();
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
        }

        void CameraHandler()
        {
            camera.ForceRotateTarget(input.magnitude > 0.2f);
            camera.InvertForwardAxis(lastNonZeroInputDir.y < 0.0f);
        }

        void MovementHandler()
        {
            if (!isMoveAble)
            {
                velocity = Vector3.zero;
                rigid.velocity = velocity;
                return;
            }

            //Flip forward dir, make camera handle rotation
            if (camera.CameraState == CameraController.State.Normal)
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
                    isMoveAble = true;
                    break;

                case GameState.Over:
                    isMoveAble = false;
                    break;

                default:
                    break;
            }
        }
    }
}

