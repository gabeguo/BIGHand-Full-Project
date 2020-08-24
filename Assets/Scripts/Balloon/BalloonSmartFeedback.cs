using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public static class BalloonSmartFeedback
{
    public const int LEFT_HAND = 0, RIGHT_HAND = 1;
    public const int LEFT_COLLISION = 0, RIGHT_COLLISION = 1, CLOUD_COLLISION = 2;
    private static int[,] collisions = new int[2, 3];

    public static void Collision(int hand, int type)
    {
        collisions[hand, type] += 1;
    }

    public static string LevelFinish()
    {
        string result = "Smart Tips:\n\n";

        result += GetCollisionTypeTips();

        if (result == "Smart Tips:\n\n")
            result += "-Great job! You completed this level nearly perfectly!\n\n";

        ResetVars();
        return result;
    }

    private static string GetCollisionTypeTips()
    {
        string result = "";
        int[] hands = { LEFT_HAND, RIGHT_HAND };

        int cloudColls = hands.Select((x) => collisions[x, CLOUD_COLLISION]).Sum((x) => x);
        int leftColls = hands.Select((x) => collisions[x, LEFT_COLLISION]).Sum((x) => x);
        int rightColls = hands.Select((x) => collisions[x, RIGHT_COLLISION]).Sum((x) => x);
        int totalColls = cloudColls + leftColls + rightColls;

        if (totalColls > 9)
        {
            if (cloudColls > leftColls + rightColls)
            {
                string[] cloudTips = {
                    "-You collide with the clouds more often than the buildings when" +
                    " using your left hand, try not to squeeze as hard with that hand.\n\n",
                    "-You collide with the clouds more often than the buildings when" +
                    " using your right hand, try not to squeeze as hard with that hand.\n\n",
                    "-You collide with the clouds more often than the buildings," +
                    " try not to squeeze as hard.\n\n"
                };

                result += GetAreaTips(CLOUD_COLLISION, cloudTips);
            }

            if (leftColls > 2 * rightColls)
            {
                string[] leftTips = {
                    "-When using your left hand, you tend to hit the buildings on" +
                    " the way up, try squeezing earlier or harder with that hand.\n\n",
                    "-When using your right hand, you tend to hit the buildings on" +
                    " the way up, try squeezing earlier or harder with that hand.\n\n",
                    "-You tend to hit the buildings on the way up, try squeezing" +
                    " earlier or harder when obstacles are approaching.\n\n"
                };

                result += GetAreaTips(LEFT_COLLISION, leftTips);
            }
            else if (rightColls > 2 * leftColls)
            {
                string[] rightTips = {
                    "-When using your left hand, you tend to hit the buildings on the way down, try" +
                    " conserving energy before obstacles or holding your grip longer with that hand.\n\n",
                    "-When using your right hand, you tend to hit the buildings on the way down, try" +
                    " conserving energy before obstacles or holding your grip longer with that hand.\n\n",
                    "-You tend to hit the buildings on the way down, try conserving energy" +
                    " before obstacles or holding your grip for longer.\n\n",
                };

                result += GetAreaTips(RIGHT_COLLISION, rightTips);
            }
        }

        return result;
    }

    private static string GetAreaTips(int area, string[] tips)
    {
        int lefts = collisions[LEFT_HAND, area];
        int rights = collisions[RIGHT_HAND, area];

        return TipFetcher(lefts, rights, tips);
    }

    private static string TipFetcher(int leftColls, int rightColls, string[] tips)
    {
        if (leftColls > 2 * rightColls)
            return tips[0];
        else if (rightColls > 2 * leftColls)
            return tips[1];
        else
            return tips[2];
    }

    public static void ResetVars()
    {
        collisions = new int[2, 3];
    }
}
