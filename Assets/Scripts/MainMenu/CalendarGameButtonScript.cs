using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

// for the buttons/subicons for the games on the calendar square/date
public class CalendarGameButtonScript : MonoBehaviour
{
    // text of this button
    public Text displayText;
    // display image
    private Image theImage;
    // the sprite that represents this game
    private Sprite theSprite;
    // the checkbox sprite (same for all calendar buttons)
    public Sprite checkboxSprite;
    // the sprite that is shown when game is completed
    public Sprite completeSprite;
    // the name of the game this icon represents
    public string TABLE_NAME;
    // the script for the calendar panel
    private CalendarIconScript calendarIconScript;
    
    // Start is called before the first frame update
    void Start()
    {
        theImage = GetComponent<Image>();
        theSprite = theImage.sprite;
        calendarIconScript = GameObject.Find("Calendar Panel").GetComponent<CalendarIconScript>();
        CheckDateAndSetSprite();
    }
    // checks whether or not game is complete, and sets sprite to have checkmark or not based on that
    public void CheckDateAndSetSprite()
    {
        if (HasBeenPlayedOnCurrentDate())
        {
            theImage.sprite = completeSprite;
        }
        else
        {
            theImage.sprite = theSprite;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    // called when hovered over; makes it say play me if game has not been played today
    public void ChangeOnHover()
    {
        if (!HasBeenPlayedOnCurrentDate())
        {
            MessagePlayMe();
        }
    }
    
    //has the game been played on the day the calendar is set to?
    private bool HasBeenPlayedOnCurrentDate()
    {
        return ((SqliteDatabaseRepository)_GlobalVariables.dataRep).WasGamePlayedOnDate(TABLE_NAME, calendarIconScript.GetCurrDate());
    }
    
    // makes text on button say "Play!" when hovered over
    public void MessagePlayMe()
    {
        displayText.text = "Play!";
        displayText.color = Color.green;
        theImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    }
    
    // eliminates text on button when not hovered over (inverse of ChangeOnHover())
    // button becomes normal again
    public void ClearMessage()
    {
        displayText.text = "";
        theImage.color = new Color(1, 1, 1, 1);
    }
}
