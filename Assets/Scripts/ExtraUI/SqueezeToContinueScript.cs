using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Linq;

public class SqueezeToContinueScript : MonoBehaviour
{
    public Image iconObject;
    public Sprite[] iconImages;
    public UnityEvent doOnFinish;
    public float requiredPercentage;

    private bool activated;
    private bool hasReachedFloor;
    private float requiredActualForce;
    private float spriteChangeForce;

    private const float FLOOR_PERCENTAGE = 0.05f;

    private void Awake()
    {
        requiredActualForce = (_GlobalVariables.maxGripStrength[0] + _GlobalVariables.maxGripStrength[1]) / 2 * requiredPercentage;
        spriteChangeForce = requiredActualForce / iconImages.Length;

        activated = false;
    }

    private void Update()
    {
        //Debug.Log("Activated: " + activated);
        if (!activated)
            return;

        float appliedForce = _GlobalVariables.leftForce + _GlobalVariables.rightForce;
        if (appliedForce <= FLOOR_PERCENTAGE * _GlobalVariables.maxGripStrength.Sum(m => m))
        {
            hasReachedFloor = true;
        }

        if (hasReachedFloor)
        {
            if (appliedForce >= requiredActualForce)
            {
                Activate();
                doOnFinish.Invoke();
            }
            else
            {
                int index = Mathf.FloorToInt(appliedForce / spriteChangeForce);
                iconObject.sprite = iconImages[index];
            }
        }
    }

    public void Activate()
    {
        activated = true;
        hasReachedFloor = false;
    }
}
