using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class WhenPigsFlySmartFeedbackProcessor
{
    public const int UP = 0, DOWN = 1, LEFT = 2, RIGHT = 3;
    private static int[] missedCoins = new int[4];
    private static int[] totalCoins = new int[4];

    public static string LevelFinish()
    {
        string result = "Smart Tips:\n\n";

        result += GetFeedback();

        if (result == "Smart Tips:\n\n")
            result += "-Great job! You completed this level nearly perfectly!\n\n";

        ResetVars();
        return result;
    }

    public static void AddCoinMisses(int[] directions, int missed, int possible)
    {
        foreach (int d in directions)
        {
            missedCoins[d] += missed;
            totalCoins[d] += possible;
        }
    }

    private static string GetFeedback()
    {
        string result = "";
        float missPercentage = (float)missedCoins.Sum() / totalCoins.Sum();
        if (missPercentage >= 0.333f)
            return result;

        if (missedCoins[LEFT] > 2 * missedCoins[RIGHT])
            result += "-You tend to undershoot to the left of the coin, try to squeeze harder " +
                "with your right hand to keep closer to the coin.\n\n";
        else if (missedCoins[RIGHT] > 2 * missedCoins[LEFT])
            result += "-You tend to overshoot to the right of the coin, try to squeeze lighter " +
                "with your right hand to keep closer to the coin.\n\n";
        if (missedCoins[UP] > 2 * missedCoins[DOWN])
            result += "-You tend to overshoot above the coin, try to squeeze lighter " +
                "with your left hand to keep closer to the coin.\n\n";
        else if (missedCoins[DOWN] > 2 * missedCoins[UP])
            result += "-You tend to undershoot below the coin, try to squeeze harder " +
                "with your left hand to keep closer to the coin.\n\n";

        if (result == "")
            result = "-You are generally inaccurate when following the coin, try to keep closer to it.\n\n";

        return result;
    }

    public static void ResetVars()
    {
        missedCoins = new int[4];
        totalCoins = new int[4];
    }
}
