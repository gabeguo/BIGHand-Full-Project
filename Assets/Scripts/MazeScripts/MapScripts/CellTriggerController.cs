using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellTriggerController : MonoBehaviour
{
    private Func<Collider2D, bool> TriggerAction = null;

    public void Start()
    {
        if (TriggerAction == null)
            TriggerAction = DefaultTriggerAction;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("Trigger entered");
        TriggerAction(collision);
    }

    public void SetDelegateAction(Func<Collider2D, bool> del)
    {
        TriggerAction = del;
    }

    private bool DefaultTriggerAction(Collider2D c)
    {
        return true;
    }
}