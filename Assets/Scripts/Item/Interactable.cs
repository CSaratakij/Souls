using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Souls
{
    public class Interactable : MonoBehaviour
    {
        public event Action OnInteract;
        bool isInteractable = false;

        void Update()
        {
            InputHandler();
        }

        void InputHandler()
        {
            if (!isInteractable)
                return;

            if (Input.GetButtonDown("Interact"))
            {
                OnInteract?.Invoke();
            }
        }

        void OnDestroy()
        {
            OnInteract = null;
        }

        void OnTriggerEnter(Collider col)
        {
            if (col.CompareTag("Player"))
            {
                isInteractable = true;
            }
        }

        void OnTriggerExit(Collider col)
        {
            if (col.CompareTag("Player"))
            {
                isInteractable = false;
            }
        }
    }
}

