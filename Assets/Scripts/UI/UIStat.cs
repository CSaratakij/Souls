using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Souls
{
    public class UIStat : MonoBehaviour
    {
        [SerializeField]
        Slider slider;

        [SerializeField]
        Stat stat;

        void Awake()
        {
            SubscribeEvent();
        }

        void OnEnable()
        {
            UpdateUI(stat.Current);
        }

        void OnDestroy()
        {
            UnsubscribeEvent();
        }

        void UpdateUI(int value)
        {
            slider.value = value;
        }

        void SubscribeEvent()
        {
            stat.OnValueChanged += OnValueChanged;
        }
        
        void UnsubscribeEvent()
        {
            stat.OnValueChanged -= OnValueChanged;
        }

        void OnValueChanged(int value)
        {
            UpdateUI(value);
        }
    }
}

