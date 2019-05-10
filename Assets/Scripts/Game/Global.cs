using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Souls
{
    public class Global : MonoBehaviour
    {
        public static Transform Player = null;

        void Awake()
        {
            if (Player == null)
            {
                Player = GameObject.FindGameObjectWithTag("Player").transform;
            }
        }
    }
}

