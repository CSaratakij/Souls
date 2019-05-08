using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Souls
{
    public class TargetFramerate : MonoBehaviour
    {
        [SerializeField]
        int targetFrameRate = 60;

        void Awake()
        {
            Application.targetFrameRate = targetFrameRate;
        }
    }
}

