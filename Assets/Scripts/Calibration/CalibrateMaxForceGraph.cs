using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Threading;

public class CalibrateMaxForceGraph : MonoBehaviour
{
    //array of the bars on the graph that represent left and right force
    public GameObject[] leftIndicatorGOs;
    public GameObject[] rightIndicatorsGOs;
    //the save button game object
    public GameObject saveButtonGO;
    //the text that shows plug in/reset instructions
    public GameObject changingInstructionGO;
    //the screen that directs arduino actions
    public GameObject overlayPanel;
    //the sprites that are subbed into the indicator arrays to signify forces
    public Sprite maxSprite, activeSprite, inactiveSprite;
    //the amount of newtons that trigger the next indicator sprite change
    public int newtonsPerIndicator;

    //image components of indicator arrays
    private Image[] leftIndicators;
    private Image[] rightIndicators;
    //text component of instruction text game object
    private TextMeshProUGUI changingInstructionText;
    //button component of save button game object
    private Button saveButton;
    //current measured max forces
    private int leftMaxForce, rightMaxForce;

    //initialize important variables and restart communicator thread
    void Start()
    {
        //kill any current running alternate threads
        //_GlobalVariables.killCommunicator = true;
        _GlobalVariables.mainThreadActive = true;

        //reset relevant variables if communicator is being created
        CheckAndStartCommunicator();

        //ensure that both indicator arrays are equal in length
        int numDivisions = leftIndicatorGOs.Length;
        if (numDivisions != rightIndicatorsGOs.Length)
            throw new InvalidOperationException("Number of left and right indicators must match for calibration UI to function.");

        //initialize image component arrays
        leftIndicators = new Image[numDivisions];
        rightIndicators = new Image[numDivisions];
        for (int i = 0; i < numDivisions; i++)
        {
            leftIndicators[i] = leftIndicatorGOs[i].GetComponent<Image>();
            rightIndicators[i] = rightIndicatorsGOs[i].GetComponent<Image>();
        }

        //initialize instructions text
        saveButton = saveButtonGO.GetComponent<Button>();
        changingInstructionText = changingInstructionGO.GetComponent<TextMeshProUGUI>();

        //wait until thread terminates if it hasn't already
        //while (_GlobalVariables.communictorThreadExists)
        //{
        //    ;
        //}

        //re-initialize global variables that are pertinent to communicator thread
        _GlobalVariables.leftForce = _GlobalVariables.UNINITIALIZED;
        _GlobalVariables.rightForce = _GlobalVariables.UNINITIALIZED;
        _GlobalVariables.maxGripStrength[0] = _GlobalVariables.UNINITIALIZED;
        _GlobalVariables.maxGripStrength[1] = _GlobalVariables.UNINITIALIZED;
        //_GlobalVariables.killCommunicator = false;

        //start new thread
    }

    private void CheckAndStartCommunicator()
    {
        if (!_GlobalVariables.communicatorThreadExists)
        {
            _GlobalVariables.killCommunicator = false;
            _GlobalVariables.keyboardActive = false;
            Debug.Log("Create arduino input thread");
            Thread thread = new Thread(ArduinoCommunicator.BeginConnection);
            Debug.Log("Start arduino input thread");
            thread.Start();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_GlobalVariables.keyboardActive || ShouldUseKeyboardInput())
        {
            DoForceCalculations();
            return;
        }

        CheckAndStartCommunicator();

        //Debug.Log("Is Arduino sending data: " + _GlobalVariables.isReadingArduinoData);
        if (!_GlobalVariables.portFound)
        {           //if the port is not discovered, write message and listen for keyboard command
            //Debug.Log("Port not found");
            overlayPanel.SetActive(true);
            changingInstructionText.text = "Please plug in the hand grips";
        }
        else if (!_GlobalVariables.isReadingArduinoData)
        {   //if the port is found but left and right force are not being updated
            //Debug.Log("Port found, but data not read");
            overlayPanel.SetActive(true);
            changingInstructionText.text = "Please press the reset button";
        }
        else
        {
            //Debug.Log("data being read");
            //if input detected deactivate overlay
            overlayPanel.SetActive(false);
            DoForceCalculations();
        }
    }

    private void DoForceCalculations()
    {
        int currLeftForce = (int)_GlobalVariables.leftForce;
        int currRightForce = (int)_GlobalVariables.rightForce;

        //update maximums
        if (leftMaxForce < currLeftForce)
        {
            leftMaxForce = currLeftForce;
        }
        if (rightMaxForce < currRightForce)
        {
            rightMaxForce = currRightForce;
        }

        int lMaxIndex = leftMaxForce / newtonsPerIndicator, rMaxIndex = rightMaxForce / newtonsPerIndicator;

        //show forces on graph
        UpdateForceDisplay(currLeftForce, currRightForce);

        //display maximums on graphs
        int maxLeftIndex = Mathf.Min(leftIndicators.Length - 1, lMaxIndex);
        leftIndicators[maxLeftIndex].sprite = maxSprite;

        int maxRightIndex = Mathf.Min(rightIndicators.Length - 1, rMaxIndex);
        rightIndicators[maxRightIndex].sprite = maxSprite;

        //if maximums are both greater than 0 then save button activated
        if (leftMaxForce > 0 && rightMaxForce > 0)
            saveButton.interactable = true;
    }

    private bool ShouldUseKeyboardInput()
    {
        if (Input.GetKey("k"))
        {
            _GlobalVariables.keyboardActive = true;
            _GlobalVariables.killCommunicator = true;
            overlayPanel.SetActive(false);
            return true;
        }
        return false;
    }

    public void GoToMenu()
    {
        //save maximum grip strength
        _GlobalVariables.maxGripStrength = new float[] { leftMaxForce, rightMaxForce };
        //print maximum grip strength from <c>_GlobalVariables</c> to verify
        Debug.Log("Max grip strength saved as: ("
                  + _GlobalVariables.maxGripStrength[0] + ", " + _GlobalVariables.maxGripStrength[1] + ")");

        SceneManager.LoadScene("MainMenu");
    }

    private void UpdateForceDisplay(int currLeft, int currRight)
    {
        int currLeftIndex = Mathf.Min(leftIndicators.Length - 1, currLeft / newtonsPerIndicator);
        int currRightIndex = Mathf.Min(rightIndicators.Length - 1, currRight / newtonsPerIndicator);

        SetGraph(leftIndicators, currLeftIndex, leftIndicators.Length);
        SetGraph(rightIndicators, currRightIndex, rightIndicators.Length);
    }

    private void SetGraph(Image[] imgArray, int splitIndex, int max)
    {
        for (int i = 0; i < splitIndex; i++)
        {
            imgArray[i].sprite = activeSprite;
        }
        for (int i = splitIndex; i < max; i++)
        {
            imgArray[i].sprite = inactiveSprite;
        }
    }

    private void OnDestroy()
    {
        _GlobalVariables.mainThreadActive = false;
    }
}
