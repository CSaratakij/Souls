using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Souls
{
    [RequireComponent(typeof(Interactable))]
    public class RestPoint : MonoBehaviour
    {
        Interactable interactable;

        void Awake()
        {
            Initialize();
            SubscribeEvent();
        }

        void OnDestroy()
        {
            UnsubscribeEvent();
        }

        void Initialize()
        {
            interactable = GetComponent<Interactable>();
        }

        void SubscribeEvent()
        {
            interactable.OnInteract += OnInteract;
        }

        void UnsubscribeEvent()
        {
            interactable.OnInteract -= OnInteract;
        }

        void OnInteract()
        {
            ResetGame();
        }

        void ResetGame()
        {
            GameController.Instance.GameReset();
            Time.timeScale = 1.0f;
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }
    }
}

