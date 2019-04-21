using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Souls
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        [Range(1.0f, 10.0f)]
        float sensitivity;

        [SerializeField]
        [Range(0.0f, 1.0f)]
        float rotationClamp;

        [SerializeField]
        [Range(0.0f, 1.0f)]
        float zoomOutClamp;

        [SerializeField]
        Vector3 offset;

        [SerializeField]
        Transform target;

        [SerializeField]
        LayerMask cameraObstacleMask;


        public State CameraState => state;

        public enum State
        {
            Normal,
            LockOn
        }

        State state;

        float hitMargin;
        float currentDistance;

        float minimumDistance;
        float maximumDistance;

        bool isForceRotateTarget;
        bool isInvertForwardAxis;

        bool isHitObstacle;
        bool isCurrentHitObstacle;
        bool isPreviousHitObstacle;

        Vector2 mouseInput;

        Vector3 rotationAxis;
        Vector3 orbitPosition;

        RaycastHit hit;


        void Reset()
        {
            sensitivity = 1.0f;
            rotationClamp = 0.4f;
            zoomOutClamp = 0.005f;
            offset = new Vector3(0.0f, 1.5f, -5.5f);
            target = (!target) ? GameObject.FindGameObjectWithTag("Player").transform : target;
            cameraObstacleMask = LayerMask.GetMask("Default");
        }

        void Awake()
        {
            hitMargin = 0.5f;
            minimumDistance = 0.5f;
            maximumDistance = Mathf.Abs(offset.z);
            currentDistance = maximumDistance;
        }

        //Test
        void Start()
        {
            Application.targetFrameRate = 60;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            InputHandler();
        }

        void FixedUpdate()
        {
            RotateHandler();
            OrbitHandler();
        }

        void InputHandler()
        {
            mouseInput.x = Input.GetAxis("Mouse X");
            mouseInput.y = Input.GetAxis("Mouse Y");

            rotationAxis.x += -mouseInput.y * sensitivity;
            rotationAxis.y += mouseInput.x * sensitivity;

            rotationAxis.x = Mathf.Clamp(rotationAxis.x, -75.0f, 75.0f);

            if (rotationAxis.y > 360.0f)
            {
                rotationAxis.y -= 360.0f;
            }
            else if (rotationAxis.y < -360.0f)
            {
                rotationAxis.y += 360.0f;
            }
        }

        void RotateHandler()
        {
            var checkPosition = (transform.rotation * offset) + target.position;
            var checkDirection = (checkPosition - target.position).normalized;

            isHitObstacle = Physics.Raycast(target.position, checkDirection, out hit, maximumDistance, cameraObstacleMask);

            isPreviousHitObstacle = isCurrentHitObstacle;
            isCurrentHitObstacle = isHitObstacle;

            var targetRotation = Quaternion.Euler(rotationAxis);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationClamp);

            if (isForceRotateTarget)
            {
                RotateTarget();
            }
        }

        void OrbitHandler()
        {
            if (!isPreviousHitObstacle && isCurrentHitObstacle)
            {
                currentDistance = (target.position - transform.position).magnitude;
            }

            if (isHitObstacle)
            {
                var targetDistance = hit.distance - hitMargin;

                currentDistance = targetDistance;
                currentDistance = Mathf.Clamp(currentDistance, (targetDistance < minimumDistance) ? minimumDistance : targetDistance, maximumDistance);

                var orbitPosition = (transform.rotation * new Vector3(offset.x, offset.y, -currentDistance)) + target.position;
                transform.position = orbitPosition;
            }
            else
            {
                currentDistance += Mathf.Lerp(currentDistance, maximumDistance, zoomOutClamp) * Time.fixedDeltaTime;
                currentDistance = Mathf.Clamp(currentDistance, minimumDistance, maximumDistance);

                var orbitPosition = (transform.rotation * new Vector3(offset.x, offset.y, -currentDistance)) + target.position;
                transform.position = orbitPosition;
            }
        }

        void RotateTarget()
        {
            switch (state)
            {
                case State.Normal:
                    if (isInvertForwardAxis)
                    {
                        var lookDir = transform.forward * -1.0f;
                        lookDir.y = 0.0f;

                        var lookRotation = Quaternion.LookRotation(lookDir);
                        target.rotation = Quaternion.Slerp(target.rotation, lookRotation, rotationClamp);
                    }
                    else
                    {
                        var targetRotationAxis = rotationAxis;
                        targetRotationAxis.x = 0.0f;

                        var targetRotation = Quaternion.Euler(targetRotationAxis);
                        target.rotation = Quaternion.Slerp(target.rotation, targetRotation, rotationClamp);
                    }
                    break;

                case State.LockOn:
                    /* var targetRotationAxis = rotationAxis; */
                    /* targetRotationAxis.x = 0.0f; */

                    /* var targetRotation = Quaternion.Euler(targetRotationAxis); */
                    /* target.rotation = Quaternion.Slerp(target.rotation, targetRotation, rotationClamp);*/
                    break;

                default:
                    break;
            }
        }

        public void ToggleState()
        {
            state = (state == State.Normal) ? State.LockOn : State.Normal;
        }

        public void InvertForwardAxis(bool value)
        {
            isInvertForwardAxis = value;
        }

        public void ForceRotateTarget(bool value)
        {
            isForceRotateTarget = value;
        }
    }
}

