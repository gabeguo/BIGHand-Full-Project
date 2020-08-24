using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using UnityEngine;
using System.Runtime.Serialization;
using UnityEditor;

public static class WhenPigsFlyLevelInitializer
{
    /// <summary>
    /// Maximum percentage of width or height that pig can go either way
    /// </summary>
    const float scaleFactor = 0.9f;
    /// <summary>
    /// Exposed to other scripts for generating JSON files containing main points as percentages for each stage of each level
    /// Writes to files named in format "pigs_diff_lx_sy.txt", where x is the level, and y is the stage
    /// </summary>
    public static void WriteLevelsToJSON()
    {
        Queue<Vector2> pl0s0 = new Queue<Vector2>(new[] { new Vector2(-scaleFactor, -scaleFactor), new Vector2(-scaleFactor, +scaleFactor) });
        writeFile(0, 0, pl0s0);

        Queue<Vector2> pl0s1 = new Queue<Vector2>(new[] { new Vector2(-scaleFactor, -scaleFactor), new Vector2(+scaleFactor, -scaleFactor) });
        writeFile(0, 1, pl0s1);

        Queue<Vector2> pl0s2 = new Queue<Vector2>(new[] { new Vector2(-scaleFactor, -scaleFactor), new Vector2(+scaleFactor, -scaleFactor),
            new Vector2(+scaleFactor, +scaleFactor)});
        writeFile(0, 2, pl0s2);

        Queue<Vector2> pl0s3 = new Queue<Vector2>(new[] { new Vector2(-scaleFactor, -scaleFactor), new Vector2(-scaleFactor, +scaleFactor),
            new Vector2(+scaleFactor, +scaleFactor)});
        writeFile(0, 3, pl0s3);

        Queue<Vector2> pl1s0 = new Queue<Vector2>(new[] { new Vector2(-scaleFactor, -scaleFactor), new Vector2(-scaleFactor, +scaleFactor),
            new Vector2(-scaleFactor, -scaleFactor)});
        writeFile(1, 0, pl1s0);

        Queue<Vector2> pl1s1 = new Queue<Vector2>(new[] { new Vector2(-scaleFactor, -scaleFactor), new Vector2(+scaleFactor, -scaleFactor),
            new Vector2(-scaleFactor, -scaleFactor)});
        writeFile(1, 1, pl1s1);

        Queue<Vector2> pl1s2 = new Queue<Vector2>(new[] { new Vector2(-scaleFactor, -scaleFactor), new Vector2(0, -scaleFactor),
            new Vector2(0, +scaleFactor), new Vector2(0, -scaleFactor), new Vector2(-scaleFactor, -scaleFactor)});
        writeFile(1, 2, pl1s2);

        Queue<Vector2> pl1s3 = new Queue<Vector2>(new[] { new Vector2(-scaleFactor, -scaleFactor), new Vector2(-scaleFactor, 0),
            new Vector2(+scaleFactor, 0), new Vector2(-scaleFactor, 0), new Vector2(-scaleFactor, -scaleFactor)});
        writeFile(1, 3, pl1s3);

        Queue<Vector2> pl2s0 = new Queue<Vector2>(new[] { new Vector2(-scaleFactor, -scaleFactor), new Vector2(+scaleFactor, +scaleFactor) });
        writeFile(2, 0, pl2s0);

        Queue<Vector2> pl2s1 = new Queue<Vector2>(new[] { new Vector2(-scaleFactor, -scaleFactor), new Vector2(+scaleFactor, +scaleFactor)
            , new Vector2(-scaleFactor, -scaleFactor)});
        writeFile(2, 1, pl2s1);

        Queue<Vector2> pl2s2 = new Queue<Vector2>(new[] { new Vector2(-scaleFactor, -scaleFactor), new Vector2(0, +scaleFactor * 0.5f)
            , new Vector2(+scaleFactor, -scaleFactor)});
        writeFile(2, 2, pl2s2);

        Queue<Vector2> pl2s3 = new Queue<Vector2>(new[] { new Vector2(-scaleFactor, -scaleFactor), new Vector2(0, +scaleFactor * 0.5f)
            , new Vector2(+scaleFactor, -scaleFactor), new Vector2(0, +scaleFactor * 0.5f), new Vector2(-scaleFactor, -scaleFactor)});
        writeFile(2, 3, pl2s3);
    }
    /// <summary>
    /// Writes <c>points</c> in JSON format
    /// to a file "pigs_diff_l<c>level</c>_s<c>stage</c>.txt"
    /// </summary>
    /// <param name="level">Level.</param>
    /// <param name="stage">Stage.</param>
    /// <param name="points">The main points of the pig's path</param>
    private static void writeFile(int level, int stage, Queue<Vector2> points)
    {
        using (StreamWriter file = File.CreateText(GetFilePath(level, stage)))
        {
            file.Write(CreateWritableString(points));
            Debug.Log("File created: " + level + ", " + stage + ", " + points);
            Debug.Log(GetFilePath(level, stage));
        }
    }

    /// <summary>
    /// Creates string in format:
    /// "
    /// x0 y0
    /// x1 y1
    /// etc
    /// "
    /// </summary>
    /// <returns>The writable string.</returns>
    /// <param name="points">Points.</param>
    private static string CreateWritableString(Queue<Vector2> points)
    {
        string retVal = "";
        foreach (Vector2 v in points)
        {
            float x = v.x;
            float y = v.y;
            retVal += (x + " " + y + "\n");
        }
        return retVal;
    }

    /// <summary>
    /// Gets the full file path for level and stage
    /// </summary>
    /// <returns>The file path.</returns>
    /// <param name="level">Level.</param>
    /// <param name="stage">Stage.</param>
    private static string GetFilePath(int level, int stage)
    {
        string retVal = Application.persistentDataPath + "/TextFiles/Pigs/" + GetFileName(level, stage);
        Debug.Log(retVal);
        return retVal;
    }

    /// <summary>
    /// Writes to files named in format "pigs_diff_l<c>l</c>_s<c>s</c>.txt", where l is the level, and s is the stage
    /// </summary>
    /// <returns>The file name.</returns>
    /// <param name="l">L.</param>
    /// <param name="s">S.</param>
    private static string GetFileName(int l, int s)
    {
        return "pigs_diff_l" + l + "_s" + s + ".txt";
    }

    /// <summary>
    /// Throws exception if level and stage do not exist
    /// </summary>
    /// <returns>the queue representing the main points in <c>level</c> and <c>stage</c></returns>
    /// <param name="level">Level.</param>
    /// <param name="stage">Stage.</param>
    public static Queue<Vector2> GetLevelPointsQueue(int level, int stage)
    {
        string filePath = GetFilePath(level, stage);
        if (!File.Exists(filePath)) {
            Debug.Log("Invalid file");
            return null;
        }
        StreamReader sr = File.OpenText(filePath);
        Queue<Vector2> points = new Queue<Vector2>();
        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine();
            if (line.Equals("\n") || line.Equals(""))
            {
                break;
            }
            string[] parts = line.Split(null);
            float x = float.Parse(parts[0]);
            float y = float.Parse(parts[1]);
            Vector2 pair = new Vector2(x, y);
            points.Enqueue(pair);
        }
        sr.Close();
        return points;
    }
}
