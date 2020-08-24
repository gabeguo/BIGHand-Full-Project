using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuDifficultySelector : MonoBehaviour
{
    //text field that displays game title when choosing difficulty
    public TextMeshProUGUI gameText;

    //the game that will be started if a difficulty is chosen
    private string selectedGame;

    //activates the difficulty selection screen
    public void ShowFor(string sceneName)
    {
        gameObject.SetActive(true);
        selectedGame = sceneName;
        gameText.text = selectedGame;
    }

    //triggers a scene change to the specified scene at the specified difficulty
    public void BeginOnDifficulty(int difficulty)
    {
        _GlobalVariables.difficulty = difficulty;
        SceneManager.LoadScene(selectedGame.Replace(" ", "") + "Scene");
    }

    //deactivates the difficulty selection screen
    public void Cancel()
    {
        selectedGame = "";
        gameObject.SetActive(false);
    }
}
