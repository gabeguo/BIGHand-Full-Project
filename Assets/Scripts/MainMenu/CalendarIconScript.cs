using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class CalendarIconScript : MonoBehaviour
{
    // date that is currently being displayed on the calendar
    private DateTime currDate;
    // text that displays the date and time
    public Text displayText;
    // the calendar buttons for each game
    public CalendarGameButtonScript[] gameCalendarButtons;
    
    // Start is called before the first frame update
    void Start()
    {
        ResetDateToToday();
    }
    
    public DateTime GetCurrDate()
    {
        return currDate;
    }
    // sets the current displayed date to this one; changes calendar GUI accordingly
    public void SetDate(DateTime dateToDisplay)
    {
        currDate = dateToDisplay;
        displayText.text = currDate.ToString("MM/dd/yyyy");
        foreach (CalendarGameButtonScript calGameButton in gameCalendarButtons)
        {
            calGameButton.CheckDateAndSetSprite();
        }
    }
    public void PrevDay()
    {
        SetDate(currDate.AddDays(-1));
    }
    public void NextDay()
    {
        SetDate(currDate.AddDays(+1));
    }
    public void ResetDateToToday()
    {
        SetDate(DateTime.Now);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
