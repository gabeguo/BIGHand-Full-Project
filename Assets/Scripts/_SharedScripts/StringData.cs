using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringData
{
    private string text;
    private IGameSessionData[] dataSet;

    public StringData(IGameSessionData[] dataSetParam)
    {
        dataSet = dataSetParam;
    }

    public StringData(IGameSessionData[] dataSetParam, string[] cols, float mult = 1,
        string format = "0.0", string unit = "", string altTite = null, int offset = 0)
    {
        dataSet = dataSetParam;
        AddData(cols, mult, format, unit, altTite, offset);
    }

    public StringData(IGameSessionData[] dataSetParam, string col, float mult = 1,
        string format = "0.0", string unit = "", string altTite = null, int offset = 0)
    {
        dataSet = dataSetParam;
        AddData(col, mult, format, unit, altTite, offset);
    }

    public void AddData(string[] cols, float mult = 1,
        string format = "0.0", string unit = "", string altTite = null, int offset = 0)
    {
        foreach (string col in cols)
        {
            AddData(col, mult, format, unit, altTite, offset);
        }
    }

    public void AddData(string col, float mult = 1,
        string format = "0.0", string unit = "", string altTite = null, int offset = 0)
    {
        text += (altTite ?? DeCamelCase(col)) + ":\n\n";

        for (int i = 0; i < dataSet.Length; i++)
        {
            float data = float.Parse(dataSet[i].GetStringRepresentation(col)) * mult;
            text += "Stage " + (i + 1 + offset) + ": " + data.ToString(format) + unit + "\n";
        }

        text += "\n";
    }

    public override string ToString()
    {
        return text;
    }

    private string DeCamelCase(string msg)
    {
        char[] msgArray = msg.ToCharArray();
        string result = msg.Substring(0, 1).ToUpper();
        for (int i = 1; i < msgArray.Length; i++)
        {
            if (char.IsUpper(msgArray[i]))
                result += " " + msgArray[i];
            else
                result += msgArray[i];
        }

        return result;
    }
}
