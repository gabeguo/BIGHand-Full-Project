using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Experimental.UIElements;

public class GraphDisplay : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public RectTransform graph;
    public GameObject dot, line;
    public float dotGraphPercent, lineGraphPercent;

    private List<GraphingData> dataList = new List<GraphingData>();
    private int index;

    private const float GRAPH_SCALE = 0.9f;
    private const float GRAPH_OFFSET = 0.05f;
    private const int POINT_Z_VALUE = -3;

    public bool ShowDisplay()
    {
        if (dataList.Count == 0)
            return false;

        gameObject.SetActive(true);
        index = 0;
        return DisplayGraphingData(dataList[index]);
    }

    public bool NextData()
    {
        if (!gameObject.activeSelf || index >= dataList.Count - 1)
            return false;

        return DisplayGraphingData(dataList[++index]);
    }

    private bool DisplayGraphingData(GraphingData data)
    {
        if (data.Points == null)
            return false;

        titleText.text = data.Title;

        GraphData(data.Points);

        return true;
    }

    private void GraphData(Vector3[] points)
    {
        if (points.Length == 0)
            return;

        //clear tha graph
        for (int i = graph.transform.childCount - 1; i >= 0; i--) {
            Transform child = graph.GetChild(i);
            //Debug.Log(child);
            Destroy(child.gameObject);
        }
        //Debug.Log("Points.length: " + points.Length);
        SpawnDot(points[0]);

        for (int i = 1; i < points.Length; i++)
        {
            SpawnDot(points[i]);
            SpawnLine(points[i - 1], points[i]);
        }

    }

    private void SpawnDot(Vector3 pointP)
    {
        Vector3 point = new Vector3(pointP.x, pointP.y, -1);
        point = point * GRAPH_SCALE;
        point = new Vector3(point.x + GRAPH_OFFSET, point.y + GRAPH_OFFSET, -1);

        GameObject singleDot = Instantiate(dot, new Vector3(), Quaternion.identity, graph);
        RectTransform singleDotRect = singleDot.GetComponent<RectTransform>();

        float halfDotPercent = dotGraphPercent / 2f;
        //Debug.Log("Dot, POINT: " + point);

        float minX = point.x - halfDotPercent;
        float minY = point.y - halfDotPercent;
        Vector2 min = new Vector3(minX, minY, point.z);

        float maxX = point.x + halfDotPercent;
        float maxY = point.y + halfDotPercent;
        Vector2 max = new Vector3(maxX, maxY, point.z);

        singleDotRect.anchorMin = min;
        singleDotRect.anchorMax = max;
        singleDotRect.offsetMin = new Vector2();
        singleDotRect.offsetMax = new Vector2();
    }

    private void SpawnLine(Vector3 startP, Vector3 endP)
    {
        Vector3 start = new Vector3(startP.x, startP.y, startP.z);
        Vector3 end = new Vector3(endP.x, endP.y, endP.z);

        start = start * GRAPH_SCALE;
        start = new Vector3(start.x + GRAPH_OFFSET, start.y + GRAPH_OFFSET, 0);
        end = end * GRAPH_SCALE;
        end = new Vector3(end.x + GRAPH_OFFSET, end.y + GRAPH_OFFSET, 0);

        GameObject lineSeg = Instantiate(line, new Vector3(), Quaternion.identity, graph);
        RectTransform lineSegRect = lineSeg.GetComponent<RectTransform>();

        float yToXAspect = graph.rect.size.y / graph.rect.size.x;
        float hypotenuse = Mathf.Sqrt(Mathf.Pow(end.x - start.x, 2) + Mathf.Pow((end.y - start.y) * yToXAspect, 2));
        float yDist = end.y - start.y;
        float xDist = end.x - start.x;

        if (hypotenuse <= float.Epsilon)
            return;

        float halfLineWidth = lineGraphPercent / 2f;
        //float heightLengthRatio = yDist / hypotenuse;
        float oppOverAdj = yDist * yToXAspect / xDist;
        //Debug.Log(heightLengthRatio);
        //float degrees = Mathf.Rad2Deg * Mathf.Asin(heightLengthRatio);
        float degrees = Mathf.Rad2Deg * Mathf.Atan(oppOverAdj);
        Quaternion newRotation = Quaternion.Euler(0, 0, degrees);
        //Debug.Log("Line Rotation: " + degrees);

        float oneSideHorizontalOverflow = (hypotenuse - xDist) / 2f;
        float avgY = (start.y + end.y) / 2f;

        float minX = start.x - oneSideHorizontalOverflow;
        float minY = avgY - halfLineWidth;
        Vector3 min = new Vector3(minX, minY, start.z);

        float maxX = end.x + oneSideHorizontalOverflow;
        float maxY = avgY + halfLineWidth;
        Vector3 max = new Vector3(maxX, maxY, start.z);

        lineSegRect.anchorMin = min;
        lineSegRect.anchorMax = max;
        lineSegRect.offsetMin = new Vector2();
        lineSegRect.offsetMax = new Vector2();
        lineSegRect.rotation = newRotation;
    }

    public void AddGraphData(List<DateTime> dateData, List<float> data, string title)
    {
        GraphingData newData = new GraphingData {
            Title = title
        };
        SetNewDataExtremes(newData, dateData, data);
        dataList.Add(newData);
    }

    private void SetNewDataExtremes(GraphingData dataHolder, List<DateTime> dateTimeData, List<float> rawData)
    {
        if (dateTimeData.Count == 0 || rawData.Count == 0)
            throw new ArgumentException();

        DateTime dateMin = dateTimeData.Min();
        DateTime dateMax = dateTimeData.Max();
        float dataMin = 0;
        float dataMax = RoundUp(rawData.Max());

        dataHolder.DateMin = dateMin;
        dataHolder.DateMax = dateMax;
        dataHolder.DataMin = dataMin;
        dataHolder.DataMax = dataMax;

        dateMin = dateMin.AddYears(-1);
        dateMax = dateMax.AddYears(-1);

        dataHolder.XRange = dateMax.ToUnixEpochTime() - dateMin.ToUnixEpochTime();
        dataHolder.YRange = dataMax - dataMin;

        CalculatePoints(dataHolder, dateTimeData, rawData);
    }

    private void CalculatePoints(GraphingData dataHolder, List<DateTime> dateData, List<float> data)
    {
        if (dateData.Count != data.Count)
            throw new ArgumentException();

        Vector3[] result = new Vector3[dateData.Count];

        for (int i = 0; i < dateData.Count; i++)
        {
            float xVal = CalculateDateProportion(dateData[i], dataHolder.DateMin.ToUnixEpochTime(), dataHolder.XRange);
            float yVal = CalculateRawDataProportion(data[i], dataHolder.DataMin, dataHolder.YRange);
            result[i] = new Vector3(xVal, yVal, POINT_Z_VALUE);
        }

        Vector3[] averages = ComputeSessionAverages(result);
        Vector3[] r = averages.Length >= 3 ? averages : result;
        ChangeOverToEvenDist(r);
        if (r.Length <= 1)
            dataHolder.Points = null;
        else
            dataHolder.Points = r;
    }

    private Vector3[] ComputeSessionAverages(Vector3[] data)
    {
        List<Vector3> result = new List<Vector3>();
        Vector3 currentSum = data[0];
        float currentDate = data[0].x;
        int numStages = 1;

        for (int i = 1; i < data.Length; i++)
        {
            Debug.Log("Data[" + i + "].x = " + data[i].x);
            Debug.Log("Current Date = " + currentDate);
            if (Mathf.Abs(data[i].x - currentDate) <= float.Epsilon)
            {
                currentSum += data[i];
                numStages += 1;
            }
            else
            {
                result.Add(currentSum / numStages);
                currentSum = data[i];
                currentDate = data[i].x;
                numStages = 1;
            }
        }

        return result.ToArray();
    }

    private void ChangeOverToEvenDist(Vector3[] data)
    {
        if (data.Length <= 1)
            return;

        for (int i = 0; i < data.Length; i++)
        {
            data[i] = new Vector3((float)i / (data.Length - 1), data[i].y, data[i].z);
        }
    }

    private float CalculateDateProportion(DateTime date, double offset, double range)
    {
        range = range <= float.Epsilon ? 0.1f : range;

        return (float)((date.ToUnixEpochTime() - offset) / range);
    }

    private float CalculateRawDataProportion(float data, double offset, double range)
    {
        range = range <= float.Epsilon ? 0.1f : range;

        return (float)((data - offset) / range);
    }

    private float RoundUp(float data)
    {
        int highestPlace = 1;
        while (data / (highestPlace * 10f) >= 1)
            highestPlace *= 10;

        int frontVal = (int)(data / highestPlace) + 1;
        return highestPlace * frontVal;
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

/*
Debug.Log("Dot anchored position: " + singleDotRect.anchoredPosition);
Debug.Log("Dot anchor max: " + singleDotRect.anchorMax);
Debug.Log("Dot anchor min: " + singleDotRect.anchorMin);
Debug.Log("Dot local position: " + singleDotRect.localPosition);
Debug.Log("Dot local scale: " + singleDotRect.localScale);
Debug.Log("Dot offset max: " + singleDotRect.offsetMax);
Debug.Log("Dot offset min: " + singleDotRect.offsetMin);
Debug.Log("Dot position: " + singleDotRect.position);
Debug.Log("Dot pivot: " + singleDotRect.pivot);
Debug.Log("Dot size delta: " + singleDotRect.sizeDelta);
*/
