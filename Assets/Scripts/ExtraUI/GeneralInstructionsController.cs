using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralInstructionsController : MonoBehaviour
{
    public SqueezeToContinueScript squeezeScript;

    GameObject onScreenControls;
    GameObject pauseCanvas;
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 0;
        onScreenControls = GameObject.Find("OnScreenControlCanvas");
        onScreenControls.SetActive(false);
        pauseCanvas = GameObject.Find("PauseCanvas");
        pauseCanvas.GetComponent<Canvas>().enabled = false;
        squeezeScript.Activate();
    }

    void Update()
    {
        if (Input.GetKeyDown("enter") || Input.GetKeyDown("return"))
        {
            CloseInstructions();
        }
    }

    public void CloseInstructions()
    {
        gameObject.SetActive(false);
        onScreenControls.SetActive(true);
        Time.timeScale = 1;
    }
}
