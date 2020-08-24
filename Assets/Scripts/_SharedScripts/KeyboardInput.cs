using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardInput : MonoBehaviour
{
    /// <summary>
    /// The amount of force added (or subtracted) per update cycle when key is pressed
    /// </summary>
    const int keyRate = 1;

    // Update is called once per frame
    void Update()
    {
        //only apply keyboard directional input if arduino not connected and unpaused
        if (Time.timeScale != 0 && _GlobalVariables.keyboardActive)
        {
            UpdateForceInput();
        }
    }

    /// <summary>
    /// Updates the current forces in _GlobalVariables,
    /// based on keyboard input
    /// Precondition: Called only when _GlobalVariables.keyboardActive
    /// </summary>
    private void UpdateForceInput()
    {
        if (Input.GetKey("d"))  //increase right force
            _GlobalVariables.rightForce = Mathf.Min(600, _GlobalVariables.rightForce + keyRate);
        if (Input.GetKey("a"))  //decrease right force
            _GlobalVariables.rightForce = Mathf.Max(0, _GlobalVariables.rightForce - keyRate);
        if (Input.GetKey("w"))  //increase left force
            _GlobalVariables.leftForce = Mathf.Min(600, _GlobalVariables.leftForce + keyRate);
        if (Input.GetKey("s"))  //decrease left force
            _GlobalVariables.leftForce = Mathf.Max(0, _GlobalVariables.leftForce - keyRate);
    }
}
