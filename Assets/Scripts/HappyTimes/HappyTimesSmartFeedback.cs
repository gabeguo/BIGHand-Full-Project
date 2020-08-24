using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class HappyTimesSmartFeedback
{
    public const int EARLY = 0, LATE = 1, WIDE = 2, MISS = 3;
    public const int SQUEEZE = 0, RELEASE = 1, NEITHER = 2;
    public const int LEFT = 0, RIGHT = 1;
    private static int[,] grips = new int[2,4];

    public const int SQUEEZE_NO_FACE = 10;
    public const int SQUEEZE_FACE = 11;
    public const int RELEASE_NO_FACE = 20;
    public const int RELEASE_FACE = 21;
    public const int FACE_HITS = 30;
    public const int FACE_LEAVES = 31;

    private static int[] handCodes = new int[2];

    private static string[] handStrings = { "left", "right" };
    private static Dictionary<int, Action<int>> TIP_LOOKUP = new Dictionary<int, Action<int>> {
        { 103021, (x) => grips[x,EARLY] += 1 },
        { 1131, (x) => grips[x,LATE] += 1 },
        { 103031, (x) => grips[x,WIDE] += 1 },
        { 1020, (x) => grips[x,MISS] += 1 },
        { 1130, (x) => { /* Changed to smiley */ } }
    };

    public static string LevelFinish()
    {
        string result = "Smart Tips:\n\n";

        result += GetHandFeedback(LEFT) + GetHandFeedback(RIGHT);

        if (result == "Smart Tips:\n\n")
          result += "-Great job! You completed this level nearly perfectly!\n\n";

        ResetVars();
        return result;
    }

    public static void TriggerEvent(int hand, int grip, bool face)
    {
        int nextCode = 0;

        if (grip == NEITHER) {
            if (handCodes[hand] == 0)
                return;
            else if (face)
                nextCode = FACE_HITS;
            else
                nextCode = FACE_LEAVES;
        } else if (grip == RELEASE) {
            if (handCodes[hand] == 0)
                return;
            if (face)
                nextCode = RELEASE_FACE;
            else
                nextCode = RELEASE_NO_FACE;
        } else {
            if (face)
                nextCode = SQUEEZE_FACE;
            else
                nextCode = SQUEEZE_NO_FACE;
        }

        ProcessTriggerEvent(hand, nextCode);
    }

    private static void ProcessTriggerEvent(int hand, int nextCode)
    {
        handCodes[hand] *= 100;
        handCodes[hand] += nextCode;

        int key = handCodes[hand];
        if (TIP_LOOKUP.ContainsKey(key)) {
            TIP_LOOKUP[key].Invoke(hand);
            handCodes[hand] = 0;
        } else if (nextCode == RELEASE_FACE || nextCode == RELEASE_NO_FACE) {
            grips[hand, MISS] += 1;
            handCodes[hand] = 0;
        }
    }

    private static string GetHandFeedback(int hand)
    {
        int[] types = { EARLY, LATE, WIDE, MISS };
        int totalColls = types.Select(x => grips[hand, x]).Sum();

        if (totalColls <= 4)
            return "";

        if (grips[hand, WIDE] > 0.5f * totalColls)
            return "-You tend to grip too early and release too late with your " + handStrings[hand] +
                " hand, try to wait until the faces touch the bar to squeeze and let go more" +
                " quickly with that hand.\n\n";
        else if (grips[hand, EARLY] > 0.5 * totalColls)
            return "-You tend to grip too early with your " + handStrings[hand] +
                " hand, wait until the faces hit the bar to start squeezing with that hand.\n\n";
        else if (grips[hand, LATE] > 0.5 * totalColls)
            return "-You tend to release too late with your " + handStrings[hand] +
                " hand, try to let go quicker once you squeeze with that hand.\n\n";
        else if (grips[hand, MISS] > 0.5 * totalColls)
            return "-You tend to grip and release sporadically with your " + handStrings[hand] +
                " hand, try to keep that hand relaxed until faces have reached the bar.\n\n";

        return "-You are generally inaccurate with your " + handStrings[hand] +
                " hand, try to concentrate on when the faces hit the bar, then grip and" +
                " release as fast as you can with that hand.\n\n";
    }

    public static void ResetVars()
    {
        handCodes = new int[2];
        grips = new int[2,4];
    }
}
