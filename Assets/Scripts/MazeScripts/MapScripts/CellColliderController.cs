using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellColliderController : MonoBehaviour
{
    private Func<Collision2D, bool> CollisionAction = null;
    float lastCollisionTime = 0f;

    public void Start()
    {
        if (CollisionAction == null)
            CollisionAction = DefaultCollisionAction;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (Time.time > lastCollisionTime + 1f)
        {
            CollisionAction(collision);
            lastCollisionTime = Time.time;
        }
    }

    public void SetDelegateAction(Func<Collision2D, bool> del)
    {
        CollisionAction = del;
    }

    private bool DefaultCollisionAction(Collision2D c)
    {
        return true;
    }
}
