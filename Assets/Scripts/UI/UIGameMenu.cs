using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Souls
{
    [RequireComponent(typeof(CursorController))]
    public class UIGameMenu : MonoBehaviour
    {
        [SerializeField]
        RectTransform[] menu;

        enum Menu
        {
            MainMenu,
            InGameMenu,
            PauseMenu,
            GameOver
        }

        CursorController cursorController;

        void Awake()
        {
            Initialize();
            SubscribeEvent();
        }

        void Start()
        {
            Show(Menu.MainMenu);
            cursorController.Lock(false);
        }

        void Update()
        {
            InputHandler();
        }

        void OnDestroy()
        {
            UnsubscribeEvent();
        }

        void Initialize()
        {
            cursorController = GetComponent<CursorController>();
        }

        void InputHandler()
        {
            if (GameController.Instance.State == GameState.Over)
            {
                return;
            }

            if (Input.GetButtonDown("Cancel"))
            {
                bool shouldPauseMenuVisible = menu[(int)Menu.PauseMenu].gameObject.activeSelf;
                shouldPauseMenuVisible = !shouldPauseMenuVisible;

                Menu targetMenu = (shouldPauseMenuVisible) ? Menu.PauseMenu : Menu.InGameMenu;
                Show(targetMenu);

                cursorController.Lock(!shouldPauseMenuVisible);
                Time.timeScale = (shouldPauseMenuVisible) ? 0.0f : 1.0f;
            }
        }

        void HideAll()
        {
            foreach (RectTransform rect in menu)
            {
                rect.gameObject.SetActive(false);
            }
        }

        void Show(Menu menu)
        {
            HideAll();
            Show(menu, true);
        }

        void Show(Menu menu, bool value)
        {
            this.menu[(int)menu].gameObject.SetActive(value);
        }

        void OnGameStateChange(GameState state)
        {
            switch (state)
            {
                case GameState.Start:
                {
                    Show(Menu.InGameMenu);
                    cursorController.Lock(true);
                }
                break;

                case GameState.Over:
                {
                    Show(Menu.GameOver);
                    cursorController.Lock(false);
                }
                break;

                default:
                    break;
            }
        }

        void SubscribeEvent()
        {
            GameController.Instance.OnGameStateChange += OnGameStateChange;
        }

        void UnsubscribeEvent()
        {
            GameController.Instance.OnGameStateChange -= OnGameStateChange;
        }
    }
}

