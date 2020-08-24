using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BalloonSessionData : IGameSessionData
{
    public BalloonSessionData(Dictionary<string, object> data) :
    base(data, new Dictionary<string, Type>()
    {
        { "game", typeof(string) },
        { "userId", typeof(int) },
        { "level", typeof(int) },
        { "stage", typeof(int) },
        { "date", typeof(double) },
        { "leftCollisions", typeof(int) },
        { "leftMaxStrength", typeof(int) },
        { "rightCollisions", typeof(int) },
        { "rightMaxStrength", typeof(int) }
    })
    { }
}
