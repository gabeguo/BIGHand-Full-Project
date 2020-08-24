using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PigsSessionData : IGameSessionData
{
    public PigsSessionData(Dictionary<string, object> data) :
        base(data, new Dictionary<string, Type>()
        {
            { "game", typeof(string) },
            { "userId", typeof(int) },
            { "level", typeof(int) },
            { "stage", typeof(int) },
            { "date", typeof(double) },
            { "coinPercentage", typeof(float) }
        })
    {}
}
