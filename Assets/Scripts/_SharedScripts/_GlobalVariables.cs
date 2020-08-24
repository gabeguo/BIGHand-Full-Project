using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class _GlobalVariables
{
    /// <summary>
    /// State variables.
    /// </summary>
    public static float leftForce = UNINITIALIZED, rightForce = UNINITIALIZED;  //current grip force exerted
    public static bool mainThreadActive; //whether or not input from arduino is currently being read (game is being played)
    public static bool communicatorThreadExists = false; //whether or not the communicator thread has been created yet
    public static bool portFound = false; //whether or not a serial port device has been detected
    public static bool killCommunicator = false; //whether or not to trigger a force kill of the other thread
    public static bool isReadingArduinoData = false; //whether or not the arduino is sending data at the current moment

    /// <summary>
    /// General information.
    /// </summary>
    public static string userId = "user"; //Identifier for current user, will eventually be unique db generated string
    public static float[] maxGripStrength = { UNINITIALIZED, UNINITIALIZED };  //max grip force, in Newtons, for L & R
    public static int difficulty = UNINITIALIZED; //Difficulty value for level generation
    public static bool keyboardActive;  //whether or not keyboard input is used

    /// <summary>
    /// Constants
    /// </summary>
    public const int UNINITIALIZED = -1;    //user has not yet calibrated the grip
    public const int LEFT_INDEX = 0;   //index of maxGripStrength for left
    public const int RIGHT_INDEX = 1;  //index of maxGripStrength for right

    public static IDataRepository dataRep;
}
