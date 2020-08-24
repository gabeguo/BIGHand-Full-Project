using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MazeSessionData : IGameSessionData
{
    public MazeSessionData(Dictionary<string, object> data) :
    base(data, new Dictionary<string, Type>()
    {
        { "game", typeof(string) },
        { "userId", typeof(int) },
        { "level", typeof(int) },
        { "stage", typeof(int) },
        { "date", typeof(double) },
        { "time", typeof(int) },
        { "collisions", typeof(int) }
    })
    { }
}
