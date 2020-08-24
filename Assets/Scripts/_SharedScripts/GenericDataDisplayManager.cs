using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class GenericDataDisplayManager : MonoBehaviour
{
    public StringDisplay stringViewer;
    public GraphDisplay graphViewer;

    private int viewerStage;

    private const int PRE_DISPLAY = 0, STRING_DATA_STAGE = 1, GRAPH_STAGE = 2;
    
    void Start()
    {
        //stringViewer.gameObject.SetActive(false);
        graphViewer.gameObject.SetActive(false);
    }

    public void InitializeDisplays()
    {
        if (!NextScreen())
        {
            stringViewer.textView.text = "No Data To Display";
            stringViewer.gameObject.SetActive(true);
        }
    }

    public bool NextScreen()
    {
        Debug.Log("Viewer stage: " + viewerStage);
        switch (viewerStage)
        {
            case PRE_DISPLAY:
                viewerStage += 1;
                if (!stringViewer.ShowDisplay())
                {
                    Debug.Log("No String Displays");
                    viewerStage += 1;
                    if (!graphViewer.ShowDisplay())
                    {
                        Debug.Log("No Graph Displays");
                        return false;
                    }
                }
                Debug.Log("Showing String Display Pre-Display");
                return true;
            case STRING_DATA_STAGE:
                if (!stringViewer.NextData())
                {
                    Debug.Log("No String Displays Left");
                    stringViewer.gameObject.SetActive(false);
                    if (!graphViewer.ShowDisplay())
                    {
                        Debug.Log("No Graph Displays");
                        return false;
                    }
                    viewerStage += 1;
                }
                Debug.Log("Showing String Display");
                return true;
            case GRAPH_STAGE:
                if (!graphViewer.NextData())
                {
                    Debug.Log("No Graph Displays Left");
                    graphViewer.gameObject.SetActive(false);
                    return false;
                }
                Debug.Log("Showing Graph Display");
                return true;
            default:
                return false;
        }
    }

    public void AddTextView(string[] text)
    {
        foreach (string t in text)
            AddTextView(t);
    }

    public void AddTextView(string text)
    {
        stringViewer.SaveText(text);
    }

    public void AddTableView(StringData[] data)
    {
        foreach (StringData d in data)
            AddTableView(d);
    }

    public void AddTableView(StringData data)
    {
        stringViewer.SaveText(data.ToString());
    }

    //Assuming that passed values for columns only correspond to numerical data
    public void AddGraph(IGameSessionData[] data, string[] columns, int[] discludeStages = null, string[] titles = null, int difficulty = 0)
    {
        if (titles == null)
            titles = columns;

        if (titles.Length != columns.Length)
            throw new ArgumentException("Must have equal amount of titles and columns for graphs");

        for (int i = 0; i < columns.Length; i++)
        {
            string col = columns[i];
            string colTitle = titles[i];
            AddGraph(data, col, discludeStages, colTitle, difficulty);
        }
    }

    public void AddGraph(IGameSessionData[] data, string col, int[] discludeStages = null, string title = "Graph", int difficulty = 0)
    {
        List<float> colData = new List<float>();
        List<DateTime> dateData = new List<DateTime>();
        if (discludeStages != null)
            data = data.Where((d) => !discludeStages.Contains(int.Parse(d.GetStringRepresentation("stage")))).ToArray();
        data = data.Where((d) => int.Parse(d.GetStringRepresentation("level")) == difficulty).ToArray();

        for (int i = data.Length - 1; i >= 0; i--)
        {
            IGameSessionData gameData = data[i];
            colData.Add(float.Parse(gameData.GetStringRepresentation(col)));
            dateData.Add((DateTime)gameData.GetSessionDate());
        }

        graphViewer.AddGraphData(dateData, colData, title);
    }
}
