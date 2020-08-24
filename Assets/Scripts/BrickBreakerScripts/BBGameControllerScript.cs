using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BBGameControllerScript : MonoBehaviour
{
    /// <summary>
    /// GameObject which bricks should be set as children of.
    /// </summary>
    public GameObject brickMap;
    /// <summary>
    /// GameObject which represents the top boundary of the viewable area.
    /// </summary>
    public GameObject topBound;
    /// <summary>
    /// GameObject which represents the bottom boundary of the viewable area;
    /// </summary>
    public GameObject bottomBound;
    /// <summary>
    /// Represents the difficulty at which to generate the brick map. Should only be between 1 and 3.
    /// </summary>
    public int difficultyLevel;
    /// <summary>
    /// Multiply this variable by the difficulty to determine the number of bricks to be generated.
    /// </summary>
    public int bricksPerDifficulty;
    /// <summary>
    /// Defines number of columns (and rows) of bricks.
    /// </summary>
    public int brickColumns;

    /// <summary>
    /// Defines the half height and width of the brick zone.
    /// </summary>
    private Vector2 brickMapDimensions;
    /// <summary>
    /// The half height and width of the brick map
    /// </summary>
    private Vector2 brickSizeHalfDimensions;
    /// <summary>
    /// The height and width of a single brick.
    /// </summary>
    private float brickSize;
    /// <summary>
    /// Number of bricks to be generated.
    /// </summary>
    private int numSolidBricks;
    /// <summary>
    /// The number of bricks that still need to be destroyed.
    /// </summary>
    private int currBrickCount;

    // Start is called before the first frame update
    void Start()
    {
        int totalBrickSpaces = brickColumns * brickColumns;
        numSolidBricks = (difficultyLevel + 1) * bricksPerDifficulty;

        if (numSolidBricks > totalBrickSpaces)
            throw new System.InvalidOperationException("Too few brick spaces for this difficulty.");

        float camHeight = Camera.main.orthographicSize;
        brickMapDimensions = new Vector2(camHeight, camHeight);

        brickSize = camHeight / brickColumns;
        brickSizeHalfDimensions = new Vector2(brickSize / 2, brickSize / 2);

        GenerateBricks();
        GenerateBounds(camHeight, Camera.main.aspect * camHeight * 2);
    }

    private void GenerateBricks()
    {
        List<Vector2> allPossibleLocations = new List<Vector2>();
        for (int i = 0; i < brickColumns; i++)
            for (int j = 0; j < brickColumns; j++)
                allPossibleLocations.Add(new Vector2(i, j));

        ShuffleList(allPossibleLocations);

        for (int k = 0; k < numSolidBricks; k++)
        {
            Vector2 gamePosition = ConvertSimpleCoordsToGameCoords(allPossibleLocations[k]);
            GenerateSingleBrick(gamePosition);
        }

        currBrickCount = numSolidBricks;
    }

    private void GenerateBounds(float halfHeight, float width)
    {
        topBound.transform.position = new Vector2(0, halfHeight);
        BoxCollider2D topColl = topBound.AddComponent<BoxCollider2D>();
        topColl.size = new Vector2(width, 0.1f);

        bottomBound.transform.position = new Vector2(0, -1 * halfHeight);
        BoxCollider2D botColl = bottomBound.AddComponent<BoxCollider2D>();
        botColl.size = new Vector2(width, 0.1f);
    }

    private void GenerateSingleBrick(Vector2 location)
    {
        //Create rectangle and use to get cell corners
        List<Vector2> corners = GetBrickCorners(location);

        //Generate cell with only mesh and add collider
        GameObject brick = GenerateSquareBrick(location, corners);
        AddColliderToBrick(brick, corners);

        //Make cell a child of the brick map GameObject
        brick.transform.parent = brickMap.transform;
    }

    public static GameObject GenerateSquareBrick(Vector2 position, List<Vector2> points)
    {
        //Creates cell GameObject and adds empty mesh to it
        GameObject brick = new GameObject { name = "Brick" };
        brick.AddComponent<MeshFilter>();
        MeshRenderer rend = brick.AddComponent<MeshRenderer>();
        Mesh boundMesh = new Mesh();
        brick.GetComponent<MeshFilter>().mesh = boundMesh;

        //Creates point list and point indices list for triangulation
        List<int> pointIndices = new List<int>();
        for (int i = 0; i < points.Count; i++)
            pointIndices.Add(i);

        //Calculates and applies new mesh attributes
        Vector3[] vertices = System.Array.ConvertAll(points.ToArray(), VectorUpConverter);
        int[] triangles = { 0, 1, 2, 2, 3, 0 };
        boundMesh.vertices = vertices;
        boundMesh.triangles = triangles;

        brick.transform.position = position;
        int matType = Random.Range(0, 3);
        switch (matType)
        {
            case 0:
                rend.material = Resources.Load("Materials\\BrickMat01") as Material;
                break;
            case 1:
                rend.material = Resources.Load("Materials\\BrickMat02") as Material;
                break;
            case 2:
                rend.material = Resources.Load("Materials\\BrickMat03") as Material;
                break;
        }

        return brick;
    }

    public static void AddColliderToBrick(GameObject cell, List<Vector2> points)
    {
        //Add polygon collider
        PolygonCollider2D collider = cell.AddComponent<PolygonCollider2D>();
        //Set polygon collider path
        collider.pathCount = 1;
        collider.SetPath(0, points.ToArray());
        //Add bound script for collision detection
        cell.AddComponent<BrickScript>();
    }

    private List<Vector2> GetBrickCorners(Vector2 pos)
    {
        return new List<Vector2>()
        {
            //Upper left
            new Vector2(pos.x - brickSize, pos.y + brickSize),
            //Upper right
            new Vector2(pos.x + brickSize, pos.y + brickSize),
            //Lower right
            new Vector2(pos.x + brickSize, pos.y - brickSize),
            //Lower left
            new Vector2(pos.x - brickSize, pos.y - brickSize)
        };
    }

    private Vector2 ConvertSimpleCoordsToGameCoords(Vector2 simplePos)
    {
        return (simplePos * brickSize) - (brickMapDimensions / 2) + brickSizeHalfDimensions;
    }

    public static Vector3 VectorUpConverter(Vector2 v)
    {
        return new Vector3(v.x, v.y);
    }

    /// <summary>
    /// Generic list shuffle method.
    /// </summary>
    /// <typeparam name="E">Type of list in question.</typeparam>
    /// <param name="l">List to be shuffled.</param>
    private void ShuffleList<E>(IList<E> l)
    {
        for (int i = 0; i < l.Count; i++)
        {
            int randIndex = Random.Range(0, l.Count);
            E temp = l[i];
            l[i] = l[randIndex];
            l[randIndex] = temp;
        }
    }

    public void DecrementBrickCount()
    {
        currBrickCount -= 1;
        if (currBrickCount <= 0)
        {
            //Game over, display data or return to home screen etc.
        }
    }
}
