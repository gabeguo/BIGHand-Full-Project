using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HappySessionData : IGameSessionData
{
    public HappySessionData(Dictionary<string, object> data) : 
        base(data, new Dictionary<string, Type>()
        {
            { "game", typeof(string) },
            { "userId", typeof(int) },
            { "level", typeof(int) },
            { "stage", typeof(int) },
            { "date", typeof(double) },
            { "leftGrips", typeof(int) },
            { "leftHits", typeof(int) },
            { "leftFaces", typeof(int) },
            { "rightGrips", typeof(int) },
            { "rightHits", typeof(int) },
            { "rightFaces", typeof(int) },
            { "leftAvgGripTime", typeof(float) },
            { "rightAvgGripTime", typeof(float) }
        })
    {}
}
