using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public class MainMenuSelectionTrigger : MonoBehaviour
{
    //the name of the scene which will be changed to if a difficulty is selected
    public string sceneName;
    //the game object that holds the difficulty selection panel
    public GameObject diffSelect;

    private MainMenuDifficultySelector diffSelectScript;

    //initialize script reference
    void Start()
    {
        diffSelectScript = diffSelect.GetComponent<MainMenuDifficultySelector>();
    }
    
    //trigger the difficulty select screen when a specific game is chosen
    public void PanelClick()
    {
        diffSelectScript.ShowFor(sceneName);
    }
}
