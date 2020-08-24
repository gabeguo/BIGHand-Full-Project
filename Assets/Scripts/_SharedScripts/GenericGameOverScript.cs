using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GenericGameOverScript : MonoBehaviour
{
    public GenericDataDisplayManager dataDisplay;
    public SqueezeToContinueScript squeezeScript;
    public Image[] stars;
    public Sprite fullStar, emptyStar;

    /// <summary>
    /// Only used for deactivating the screen at the start of the scene
    /// </summary>
    public void Start()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Only used to check for the enter key once this screen has been activated, later
    /// may be used for more complex UI navigation that better displays user progress from
    /// the current session
    /// </summary>
    public void Update()
    {
        if (Input.GetKeyDown("enter") || Input.GetKeyDown("return"))
        {
            NextViewPanel();
        }
    }

    public void NextViewPanel()
    {
        Debug.Log("Next viewpanel called");
        if (!dataDisplay.NextScreen())
        {
            Debug.Log("Back to menu...");
            ReturnToMenu();
        }
    }

    /// <summary>
    /// Sets the number of blue stars to the requested amount
    /// </summary>
    /// <param name="numStars">The number of stars to display</param>
    public void SetStars(int numStars)
    {
        squeezeScript.Activate();
        if (stars.Length != 3)
            throw new System.InvalidOperationException("Must have exactly 3 star object references for game over screen.");

        if (numStars < 0)
            numStars = 0;
        else if (numStars > 3)
            numStars = 3;

        for (int i = 0; i < numStars; i++)
        {
            stars[i].sprite = fullStar;
        }
        for (int i = numStars; i < stars.Length; i++)
        {
            stars[i].sprite = emptyStar;
        }
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }
}
