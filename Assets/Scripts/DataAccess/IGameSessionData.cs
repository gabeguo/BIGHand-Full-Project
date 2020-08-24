using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class IGameSessionData
{
    private Dictionary<string, object> gameData;
    private Dictionary<string, Type> typeLookup;

    public IGameSessionData(Dictionary<string, object> data, Dictionary<string, Type> tl)
    {
        typeLookup = tl;
        PrintKeys(data);

        if (IsValidFormat(data))
            gameData = data;
    }

    public void PrintKeys(Dictionary<string, object> data)
    {
        string result = "Data Keys: ";
        foreach(string k in data.Keys)
        {
            result += k + " ";
        }
        //Debug.Log(result);
    }

    public DateTime? GetSessionDate()
    {
        if (gameData == null)
            return null;

        DateTime date = new DateTime();
        date = date.FromUnixEpochTime((double)gameData["date"]);
        return date;
    }

    public string GetGameName()
    {
        if (gameData == null)
            return null;

        return (string)gameData["game"];
    }

    public string GetInsertionString()
    {
        if (gameData == null)
            return null;

        string firstHalf = "INSERT INTO " + GetGameName() + " (";
        string secondHalf = ") VALUES (";
        string endCap = ");";

        foreach (string key in gameData.Keys)
        {
            if (key.Equals("game"))
                continue;

            firstHalf += key + ", ";
            secondHalf += GetStringRepresentation(key) + ", ";
        }

        firstHalf = firstHalf.Substring(0, firstHalf.Length - 2);
        secondHalf = secondHalf.Substring(0, secondHalf.Length - 2);

        return firstHalf + secondHalf + endCap;
    }

    public string GetStringRepresentation(string key)
    {
        Type objType = typeLookup[key];

        if (objType == typeof(string))
            return "'" + (string)gameData[key] + "'";
        else if (objType.IsPrimitive || objType == typeof(string) || objType == typeof(decimal))
            return gameData[key].ToString();
        else
            return Convert.ChangeType(gameData[key], objType).ToString();
    }

    public bool IsValidFormat(Dictionary<string, object> data)
    {
        foreach(string k in typeLookup.Keys)
        {
            if (!data.ContainsKey(k))
                throw new ArgumentException("Data dictionary is improperly formatted, needs: " + k);

            //if (data[k].GetType() != typeLookup[k])
            //return false;
        }

        return true;
    }
}
