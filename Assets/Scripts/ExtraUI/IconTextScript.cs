using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IconTextScript : MonoBehaviour
{
    public Image iconImage = null;
    public TextMeshProUGUI textDisplay = null;

    public Sprite iconToDisplay = null;
    public string textToDisplay = "No text to display";

    void Start()
    {
        if (ElementsFound())
        {
            Setup(iconToDisplay, textToDisplay);
        }
    }

    public void Setup(Sprite i, string s)
    {
        iconToDisplay = i;
        textToDisplay = s;

        iconImage.sprite = i;
        textDisplay.text = s;
    }

    public bool ElementsFound()
    {
        return iconImage != null && textDisplay != null;
    }
}
