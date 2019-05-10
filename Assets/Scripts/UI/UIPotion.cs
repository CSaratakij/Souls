using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Souls
{
    public class UIPotion : MonoBehaviour
    {
        const string FORMAT = "Potion : [ {0} ]";

        [SerializeField]
        Text label;

        [SerializeField]
        Stat stat;


        void Awake()
        {
            stat.OnValueChanged += OnValueChanged;
        }

        void OnEnable()
        {
            UpdateUI(stat.Current);
        }

        void OnDestroy()
        {
            stat.OnValueChanged -= OnValueChanged;
        }

        void OnValueChanged(int value)
        {
            UpdateUI(value);
        }

        void UpdateUI(int value)
        {
            label.text = string.Format(FORMAT, stat.Current);
            label.color = stat.IsEmpty ? Color.red : Color.black;
        }
    }
}

