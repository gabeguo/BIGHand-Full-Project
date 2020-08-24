using System;
using System.IO.Ports;
using System.IO;
using System.Threading;
using UnityEngine;

public static class ArduinoCommunicator
{
    /// <summary>
    /// Port names to be checked on each platform
    /// </summary>
    private const string macPortName = "/dev/tty.usbmodem1411";
    private const string winPortName = "COM6";

    /// <summary>
    /// The serial port object communications are active on
    /// </summary>
    private static SerialPort stream = null;

    /// <summary>
    /// Preconditions: 
    ///     -> Called from MainMenu Start() in its own separate thread
    ///     -> Reset has been pressed on Arduino
    /// Establishes connection to arduino and reads data until game ends
    /// </summary>
    public static void BeginConnection()
    {
        Debug.Log("Started ArduinoCommunicator");

        _GlobalVariables.communicatorThreadExists = true;
        try
        {
            DetectPlatform();

            FlushPort();

            if (stream != null)
            {
                ReadDataIndefinitely();
                stream.Close();
            }
        } catch (IOException) {
            if (stream != null) {
                stream.Close();
            }
        }

        Debug.Log("Killed...");
        _GlobalVariables.communicatorThreadExists = false;
        _GlobalVariables.portFound = false;
        _GlobalVariables.isReadingArduinoData = false;
    }

    /// <summary>
    /// Determines which operating system is being used,
    /// and adjusts the serial port variable accordingly
    /// </summary>
    private static void DetectPlatform()
    {
        while (!_GlobalVariables.portFound && !_GlobalVariables.killCommunicator)
        {
            if (ShouldEndThread())
            {
                stream = null;
                return;
            }

            try
            {
                stream = new SerialPort(winPortName);   //windows
                stream.Open();
                _GlobalVariables.portFound = true;
                //Debug.Log("Windows port");
            }
            catch (Exception) //TODO: figure out why exception is thrown
            {
                try
                {
                    stream = new SerialPort(macPortName);   //mac
                    stream.Open();
                    _GlobalVariables.portFound = true;
                    //Debug.Log("Mac port");
                }
                catch (Exception)
                {
                    stream = null;  //keyboard
                    //Debug.Log("No port found");
                }
            }
        }
    }

    /// <summary>
    /// Continues to read force grip data from the arduino
    /// until the game is terminated.
    /// Writes data to <c>_GlobalVariables</c>, so that other scripts can access.
    /// </summary>
    private static void ReadDataIndefinitely()
    {
        //Debug.Log("Read from arduino");
        float[] forces = new float[2];
        //read from arduino until stop
        while (!_GlobalVariables.killCommunicator)
        {
            //Debug.Log("arduino thread loop");
            if (stream != null)
            {
                if (stream.IsOpen)
                {
                    try
                    {
                        //read
                        string rawInput = stream.ReadLine();
                        forces = ConvertInputToForceData(rawInput);
                        //Debug.Log("Left force: " + forces[0] + ", Right force: " + forces[1]);

                        _GlobalVariables.isReadingArduinoData = true;
                    }
                    catch (TimeoutException)
                    {
                        //Debug.Log("No bytes to read...");

                        _GlobalVariables.isReadingArduinoData = false;
                    }
                }
                else
                {
                    _GlobalVariables.isReadingArduinoData = false;
                    if (forces == null)
                    {
                        forces = new float[2];  //set to zero force, so that we don't crash the game
                    }
                }
            }
            //write
            _GlobalVariables.leftForce = forces[0];
            _GlobalVariables.rightForce = forces[1];

            //determines whether or not we are permanently done with game
            if (ShouldEndThread())
            {
                return;  //terminate read loop
            }
        }
    }

    /// <summary></summary>
    /// <param>rawInput</param> the reading from the arduino sensor, in the format "value0 value1"
    /// <returns>An array of length two, with the first value as the left hand force, and the second value as the right hand force.</returns>
    /// <param name="rawInput">Raw input.</param>
    private static float[] ConvertInputToForceData(string rawInput)
    {
        string[] forcesAsStrings = rawInput.Split();
        float[] forces = new float[2];

        forces[0] = _GlobalVariables.leftForce;
        forces[1] = _GlobalVariables.rightForce;

        try
        {
            for (int i = 0; i < 2; i++)
            {
                forces[i] = Mathf.Max(0, float.Parse(forcesAsStrings[i]));
            }
        }
        catch (FormatException)
        {
            FlushPort();    //get rid of bad input
            //Debug.Log("Format exception: " + rawInput);
        }
        catch (IndexOutOfRangeException)
        {
            FlushPort();    //get rid of bad input
            //Debug.Log("Index out of bounds exception: " + rawInput);
        }
        return forces;
    }

    /// <summary>
    /// Flushes port buffers to ensure accurate data
    /// </summary>
    public static void FlushPort()
    {
        if (stream != null)
        {
            //Don't know which buffer info is received from, clearing both for safety
            try
            {
                stream.DiscardInBuffer();
                stream.DiscardOutBuffer();
                stream.Close();
                stream.Open();
            } catch (InvalidOperationException)
            {
                Debug.Log("Tried to flush port when not open");
            }
        }
    }

    private static bool ShouldEndThread()
    {
        if (!_GlobalVariables.mainThreadActive)  //scene is switching
        {
            //check if we have begun to read data; continue execution when data is read
            for (int i = 0; i < 50; i++)
            {
                Thread.Sleep(100);
                if (_GlobalVariables.mainThreadActive)
                {
                    //if connection re-established, keep thread
                    return false;
                }
            }
            Debug.Log("Time to end the thread");
            //if connection not re-established, kill thread
            return true;
        }
        else
        {
            //connection never lost, continue execution
            return false;
        }
    }
}

