using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconInstructionsHolderPanel : MonoBehaviour
{
    public GameObject iconPrefab;
    public Sprite[] icons;
    public string[] texts;

    private void Start()
    {
        if (icons.Length != texts.Length)
            throw new System.InvalidOperationException("Icon holders must have the same number of sprites" +
                "and texts in order to be used.");

        for (int i = 0; i < icons.Length; i++)
        {
            IconTextScript newIcon = Instantiate(iconPrefab, transform).GetComponent<IconTextScript>();
            newIcon.Setup(icons[i], texts[i]);
        }
    }
}
