using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Souls
{
    namespace Souls
    {
        public class Timer : MonoBehaviour
        {
            [SerializeField]
            float current;

            [SerializeField]
            float max;

            public delegate void _Func();
            public event _Func OnTimeStart;
            public event _Func OnTimeOut;

            public float Current => current;
            public float Max => max;

            public bool IsStart { get; private set; }
            public bool IsPause { get; private set; }


            void Update()
            {
                TickHandler();
            }

            void OnDestroy()
            {
                CleanUp();
            }

            void TickHandler()
            {
                if (!IsStart || IsPause)
                    return;

                current -= 1.0f * Time.deltaTime;

                if (current <= 0.0f)
                    Stop();
            }

            void CleanUp()
            {
                OnTimeStart = null;
                OnTimeOut = null;
            }

            public void Countdown()
            {
                if (IsStart)
                    return;

                if (current <= 0.0f)
                    Reset();

                IsStart = true;
            }

            public void Pause(bool value)
            {
                IsPause = value;
            }

            public void Reset()
            {
                current = max;
                IsStart = false;
                IsPause = false;
            }

            public void Stop()
            {
                if (!IsStart)
                    return;

                IsStart = false;
                current = 0;
            }
        }
    }
}

