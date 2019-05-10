using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Souls
{
    [RequireComponent(typeof(Timer))]
    public class UIGameOver : MonoBehaviour
    {
        [SerializeField]
        Text txtGameResult;

        Timer timer;

        void Awake()
        {
            timer = GetComponent<Timer>();
            timer.OnTimeOut += OnTimeOut;
            GameController.Instance.OnGameStateChange += OnGameStateChange;
        }

        void OnDestroy()
        {
            timer.OnTimeOut -= OnTimeOut;
            GameController.Instance.OnGameStateChange -= OnGameStateChange;
        }

        void OnGameStateChange(GameState state)
        {
            switch (state)
            {
                case GameState.Over:
                {
                    Stat health = Global.Player.GetComponent<Stat>();
                    txtGameResult.text = (health.IsEmpty) ? "You dead" : "You alive";
                    timer.Countdown();
                }
                break;

                default:
                    break;
            }
        }

        void OnTimeOut()
        {
            GameController.Instance.GameReset();
            Time.timeScale = 1.0f;
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }
    }
}

