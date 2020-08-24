using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnScreenControlCanvasScript : MonoBehaviour
{
    private Canvas pauseScreen;
	// creates reference to pause screen 
	void Start()
    {
        pauseScreen = GameObject.Find("PauseCanvas").GetComponent<Canvas>();
        pauseScreen.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Pause");
            togglePause();
        }
    }

    /// <summary>
    /// Toggles whether or not the game is paused each time the esc key is pressed or the pause button is pressed
    /// </summary>
    public void togglePause()
    {
        if (Time.timeScale == 0)
        {
            //resume game
            Time.timeScale = 1;
            pauseScreen.enabled = false;
        }
        else
        {
            //pause game
            Time.timeScale = 0;
            pauseScreen.enabled = true;
        }
    }
}
