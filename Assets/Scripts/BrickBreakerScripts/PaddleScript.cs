using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleScript : MonoBehaviour
{
    public bool isLeft;
    public float offSetPercent;
    public float paddlePercent;
    public float paddleHalfWidth;
    public float maxForce;
    public float keyRate;

    private float horizontalPos;
    private float minY;
    private float maxDist;
    private float paddleSize;
    private float force;

    void Start()
    {
        float camHeight = Camera.main.orthographicSize;
        float camWidth = Camera.main.aspect * camHeight;

        horizontalPos = camWidth * (1 - offSetPercent);

        if (isLeft)
            horizontalPos = horizontalPos * -1;

        paddleSize = camHeight * 2 * paddlePercent;
        minY = (paddleSize / 4) - (camHeight / 2);
        maxDist = camHeight * 2 - paddleSize;

        force = 0;
        transform.position = new Vector2(horizontalPos, minY);

        GeneratePaddle();
    }

    void Update()
    {
        if (isLeft)
        {
            if (Input.GetKey("w"))
                force = Mathf.Min(maxForce, force + keyRate);
            if (Input.GetKey("d"))
                force = Mathf.Max(0, force - keyRate);
        }
        else
        {
            if (Input.GetKey("o"))  
                force = Mathf.Min(maxForce, force + keyRate);
            if (Input.GetKey("k"))  
                force = Mathf.Max(0, force - keyRate);
        }

        float forcePercentage = force / maxForce;
        gameObject.transform.position = new Vector2(horizontalPos, minY + maxDist * forcePercentage);
    }

    private void GeneratePaddle()
    {
        List<Vector2> corners = GetCorners();

        GeneratePaddleRect(corners);
        AddCollider(corners);
    }

    private void GeneratePaddleRect(List<Vector2> corners)
    {
        gameObject.AddComponent<MeshFilter>();
        MeshRenderer rend = gameObject.AddComponent<MeshRenderer>();
        Mesh boundMesh = new Mesh();
        gameObject.GetComponent<MeshFilter>().mesh = boundMesh;

        List<int> pointIndices = new List<int>();
        for (int i = 0; i < corners.Count; i++)
            pointIndices.Add(i);

        Vector3[] vertices = Array.ConvertAll(corners.ToArray(), VectorUpConverter);
        int[] triangles = { 0, 1, 2, 2, 3, 0 };
        boundMesh.vertices = vertices;
        boundMesh.triangles = triangles;

        rend.material = Resources.Load("Materials\\CellMat") as Material;
    }

    private void AddCollider(List<Vector2> corners)
    {
        PolygonCollider2D collider = gameObject.AddComponent<PolygonCollider2D>();
        collider.pathCount = 1;
        collider.SetPath(0, corners.ToArray());
    }

    private List<Vector2> GetCorners()
    {
        float halfHeight = paddleSize / 2;
        float xPos = horizontalPos;
        float yPos = minY;

        List<Vector2> result = new List<Vector2>();
        result.Add(new Vector2(xPos - paddleHalfWidth, yPos + halfHeight));
        result.Add(new Vector2(xPos + paddleHalfWidth, yPos + halfHeight));
        result.Add(new Vector2(xPos + paddleHalfWidth, yPos - halfHeight));
        result.Add(new Vector2(xPos - paddleHalfWidth, yPos - halfHeight));

        return result;
    }

    public static Vector3 VectorUpConverter(Vector2 v)
    {
        return new Vector3(v.x, v.y);
    }
}
