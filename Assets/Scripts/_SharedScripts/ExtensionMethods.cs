using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    public static double ToUnixEpochTime(this DateTime dateTime)
    {
        var unixTime = dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return unixTime.TotalSeconds;
    }

    public static DateTime FromUnixEpochTime(this DateTime dateTime, double unixSeconds)
    {
        DateTime newDate = new DateTime();
        newDate = newDate.AddYears(1970);
        newDate = newDate.AddSeconds(unixSeconds);

        return newDate;
    }

    public static T[] LooseRange<T>(this T[] array, int startIndex, int maxSize)
    {
        if (startIndex >= array.Length)
            return new T[0];

        int newSize = (maxSize + startIndex < array.Length ? maxSize : array.Length) - startIndex;
        T[] result = new T[newSize];

        for (int i = 0; i < newSize; i++)
            result[i] = array[i + startIndex];

        return result;
    }
}
