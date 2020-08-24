using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public static class MazeSmartFeedbackProcessor
{
    public const int LEFT = 0, RIGHT = 1, CENTER = 2, WIDE = 0, THIN = 1;
    private static int[] movementColls = new int[3], trackColls = new int[3];
    private static float avgCompletionTime = 0;

    public static void Collision(int dir, int track)
    {
        movementColls[dir] += 1;
        trackColls[track] += 1;
    }

    public static void LevelComplete(float time)
    {
        avgCompletionTime += time;
        avgCompletionTime /= 2;
    }

    public static void ResetVars()
    {
        movementColls = new int[3];
        trackColls = new int[2];
        avgCompletionTime = 0f;
    }

    public static string LevelFinish()
    {
        string result = "Smart Tips:\n\n";

        result += GetCompletionTimeTips() + GetMovementTips() + GetTrackTips();

        if (result == "Smart Tips:\n\n")
            result += "-Great job! You completed this level nearly perfectly!\n\n";         

        ResetVars();
        return result;
    }

    private static string GetMovementTips()
    {
        string result = "";
        if (movementColls.Sum((x) => x) - movementColls[CENTER] > 4) {
            if (movementColls[LEFT] > 2 * movementColls[RIGHT])
                result += "-You collide with the walls when turning left more often than when turning right, " +
                    "try to turn slower with that hand.\n\n";
            else if (movementColls[RIGHT] > 2 * movementColls[LEFT])
                result += "-You collide with the walls when turning right more often than when turning left, " +
                    "try to turn slower with that hand.\n\n";
        }

        if (movementColls[CENTER] >= 3 * (_GlobalVariables.difficulty + 1))
            result += "-You collide with the walls often when accelerating, try going slower or readjusting" +
                "more often.\n\n";

        return result;
    }

    private static string GetTrackTips()
    {
        if (movementColls.Sum((x) => x) > 4 && trackColls[THIN] > 2 * trackColls[WIDE])
            return "-You seem to have significantly increased difficulty on the thin track, try to readjust" +
                "more often and take it slower on the second stage.\n\n";
        return "";
    }

    private static string GetCompletionTimeTips()
    {
        if (avgCompletionTime > 15 * (_GlobalVariables.difficulty + 1))
            return "-Your completion time could be improved upon, try to complete the levels a little bit faster.\n\n";
        return "";
    }
}