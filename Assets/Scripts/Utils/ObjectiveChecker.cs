using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Souls
{
    public class ObjectiveChecker : MonoBehaviour
    {
        [SerializeField]
        Stat[] health;

        void Update()
        {
            if (Time.frameCount % 3 == 0)
            {
                CheckHandler();
            }
        }

        void CheckHandler()
        {
            foreach (Stat stat in health)
            {
                if (stat == null)
                {
                    Debug.Log("Null object..");
                    continue;
                }

                if (!stat.IsEmpty)
                    return;
            }

            GameController.Instance.GameOver();
        }
    }
}

