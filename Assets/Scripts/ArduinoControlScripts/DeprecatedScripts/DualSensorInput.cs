using System.IO.Ports;
using System.IO;
using System;
using UnityEngine;
/*
 * VERY IMPORTANT NOTE:
 * IF GAME DOES NOT RESPOND, PRESS "RESET" BUTTON ON ARDUINO
 */
public class DualSensorInput : MonoBehaviour
{
    /* To be used only for keyboard input */
    /// <summary>
    /// The change in force to be applied every update cycle that directional keys are
    /// pressed.
    /// </summary>
    public float keyRate;

    /// <summary>
    /// Names of ports for Gabe's (Mac) and Emery's (Windows) computers, respectively
    /// </summary>
    const string macPortName = "/dev/tty.usbmodem1411";
    const string winPortName = "COM3";

    /// <summary>
    /// The input stream which gets arduino input. Check for null before doing
    /// anything with stream
    /// </summary>
    SerialPort stream = null;

    private float[] forces = new float[2]; //stores the current forces being exerted; initialized to zero force

    // Precondition: Stream is null
    // Start is called before the first frame update
    void Start()
    {
        try
        {
            stream = new SerialPort(winPortName);   //windows
            stream.Open();
            Debug.Log("Windows port");
        }
        catch (Exception) //TODO: figure out why exception is thrown
        {
            try
            {
                stream = new SerialPort(macPortName);   //mac
                stream.Open();
                Debug.Log("Mac port");
            }
            catch (Exception)
            {
                stream = null;  //keyboard
                Debug.Log("Keyboard input");
            }
        }
        FlushPort();
    }

    // Update is called once per frame
    void Update()
    {
        if (stream == null)    //get keyboard input
        {
            //Get inputs and modify force values
            if (Input.GetKey("d"))  //increase right force
                forces[1] = Mathf.Min(600, forces[1] + keyRate);
            if (Input.GetKey("a"))  //decrease right force
                forces[1] = Mathf.Max(0, forces[1] - keyRate);
            if (Input.GetKey("w"))  //increase left force
                forces[0] = Mathf.Min(600, forces[0] + keyRate);
            if (Input.GetKey("s"))  //decrease left force
                forces[0] = Mathf.Max(0, forces[0] - keyRate);
        }
        else    //get sensor input
        {
            if (stream.IsOpen) //there is a force input
            {
                try {
                    string rawInput = stream.ReadLine();
                    forces = convertInputToForceData(rawInput);
                    //Debug.Log("Left force: " + forces[0] + ", Right force: " + forces[1]);
                }
                catch (TimeoutException)
                {
                    Debug.Log("No bytes to read...");
                }
            }
            else
            {
                forces = new float[2];  //set to zero force, so that we don't crash the game
            }
        }
    }

    /// <summary></summary>
    /// <param>rawInput</param> the reading from the arduino sensor, in the format "value0 value1"
    /// <returns>An array of length two, with the first value as the left hand force, and the second value as the right hand force.</returns>
    /// <param name="rawInput">Raw input.</param>
    private float[] convertInputToForceData(string rawInput)
    {
        string[] forcesAsStrings = rawInput.Split();
        float[] forces = new float[2];
        try
        {
            for (int i = 0; i < 2; i++)
            {
                forces[i] = float.Parse(forcesAsStrings[i]);
            }
        }
        catch (FormatException)
        {
            FlushPort();    //get rid of bad input
            Debug.Log("Format exception: " + rawInput);
        }
        catch (IndexOutOfRangeException)
        {
            FlushPort();    //get rid of bad input
            Debug.Log("Index out of bounds exception: " + rawInput);
        }
        return forces;
    }

    /// <summary>
    /// Public getter method.
    /// </summary>
    /// <returns>The current force being exerted on the left hand grip sensor.</returns>
    public float getLeftForce() {
        return Math.Max(forces[0], 0);  //always return non-negative value
    }
    /// <summary>
    /// Public getter method.
    /// </summary>
    /// <returns>The current force being exerted on the right hand grip sensor.</returns>
    public float getRightForce() {
        return Math.Max(forces[1], 0);
    }

    //Used to flush port to ensure fresh data
    public void FlushPort()
    {
        if (stream != null)
        {
            //Don't know which buffer info is received from, clearing both for safety
            stream.DiscardInBuffer();
            stream.DiscardOutBuffer();
            stream.Close();
            stream.Open();
        }
    }
}

