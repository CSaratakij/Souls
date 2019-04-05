﻿using System.Collections;
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
        [Range(0.1f, 1.0f)]
        float rotationClamp;

        [SerializeField]
        [Range(0.1f, 1.0f)]
        float zoomOutSpeed;

        [SerializeField]
        [Range(0.1f, 1.0f)]
        float orbitClamp;

        [SerializeField]
        Vector3 offset;

        [SerializeField]
        Transform target;

        [SerializeField]
        LayerMask cameraObstacleMask;


        float minimumDistance;
        float maximumDistance;
        float currentDistance;

        bool isForceRotateTarget;
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
            orbitClamp = 0.3f;
            zoomOutSpeed = 0.02f;
            offset = new Vector3(0.0f, 1.0f, -5.0f);
            target = (!target) ? GameObject.FindGameObjectWithTag("Player").transform : target;
            cameraObstacleMask = LayerMask.GetMask("Default");
        }

        void Awake()
        {
            minimumDistance = 2.0f;
            maximumDistance = Mathf.Abs(offset.z);
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
                var targetDistance = hit.distance;

                currentDistance -= Mathf.Lerp(currentDistance, targetDistance, orbitClamp) * Time.deltaTime;
                currentDistance = Mathf.Clamp(currentDistance, (targetDistance < minimumDistance) ? minimumDistance : targetDistance, maximumDistance);

                var orbitPosition = (transform.rotation * new Vector3(offset.x, offset.y, -currentDistance)) + target.position;
                transform.position = orbitPosition;
            }
            else
            {
                currentDistance = (target.position - transform.position).magnitude;

                currentDistance += Mathf.Lerp(currentDistance, maximumDistance, zoomOutSpeed) * Time.deltaTime;
                currentDistance = Mathf.Clamp(currentDistance, minimumDistance, maximumDistance);

                var orbitPosition = (transform.rotation * new Vector3(offset.x, offset.y, -currentDistance)) + target.position;
                transform.position = orbitPosition;
            }
        }

        void RotateTarget()
        {
            var targetRotationAxis = rotationAxis;
            targetRotationAxis.x = 0.0f;

            var targetRotation = Quaternion.Euler(targetRotationAxis);
            target.rotation = Quaternion.Slerp(target.rotation, targetRotation, rotationClamp);
        }

        public void ForceRotateTarget(bool value)
        {
            isForceRotateTarget = value;
        }
    }
}

