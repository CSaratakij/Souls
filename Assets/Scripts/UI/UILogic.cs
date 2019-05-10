using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Souls
{
    public class UILogic : MonoBehaviour
    {
        public void ResetGame()
        {
            GameController.Instance.GameReset();
            Time.timeScale = 1.0f;
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}

