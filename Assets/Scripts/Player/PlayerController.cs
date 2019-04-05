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

        Vector2 input;
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
            camera.ForceRotateTarget(input.magnitude > 0.2f);
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
        }

        void MovementHandler()
        {
            if (!isMoveAble) {
                velocity = Vector3.zero;
                rigid.velocity = velocity;
                return;
            }

            velocity = ((transform.forward * input.y) + (transform.right * input.x)).normalized * moveForce;
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

