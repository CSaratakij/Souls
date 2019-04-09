using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Souls
{
    public class GameController : MonoBehaviour
    {
        public static GameController Instance = null;

        public event Action<GameState> OnGameStateChange;
        public GameState State => state;


        GameState state;


        void Awake()
        {
            MakeSingleton();
        }

        void OnDestroy()
        {
            CleanUp();
        }

        void MakeSingleton()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void CleanUp()
        {
            OnGameStateChange = null;
        }

        void ChangeState(GameState state)
        {
            if (this.state == state)
                return;

            this.state = state;
            OnGameStateChange?.Invoke(state);
        }

        public void GameStart()
        {
            ChangeState(GameState.Start);
        }

        public void GameOver()
        {
            ChangeState(GameState.Over);
        }
    }
}

