using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Souls
{
    public class CursorController : MonoBehaviour
    {
        [SerializeField]
        CursorLockMode lockMode;

        [SerializeField]
        bool isCursorVisible;


        void Awake()
        {
            Cursor.lockState = lockMode;
            Cursor.visible = isCursorVisible;
        }

        public void ToggleLockMode()
        {
            isCursorVisible = !isCursorVisible;
            Lock(isCursorVisible);
        }

        public void Lock(bool value)
        {
            isCursorVisible = !value;
            lockMode = (value) ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = isCursorVisible;
            Cursor.lockState = lockMode;
        }
    }
}

