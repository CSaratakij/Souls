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
            ShowCursor(!Cursor.visible);
        }

        public void ShowCursor(bool value)
        {
            Cursor.lockState = (value) ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = value;
        }
    }
}

