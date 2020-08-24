using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StringDisplay : MonoBehaviour
{
    public TextMeshProUGUI textView;

    private List<string> texts = new List<string>();
    private int index;

    public void SaveText(string text)
    {
        texts.Add(text);
    }

    public bool ShowDisplay()
    {
        if (texts.Count == 0)
            return false;

        gameObject.SetActive(true);
        index = 0;

        textView.text = texts[index];
        Debug.Log("Text: " + texts[index]);

        return true;
    }

    public bool NextData()
    {
        Debug.Log("Text Display ActiveSelf: " + gameObject.activeSelf);
        if (!gameObject.activeSelf || index >= texts.Count - 1)
            return false;

        textView.text = texts[++index];
        Debug.Log("Text: " + textView.text);
        return true;
    }
}
