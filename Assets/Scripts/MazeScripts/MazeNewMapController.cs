using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MazeNewMapController : MonoBehaviour
{
    /// <summary>
    /// Represents the area in cells of the grid the map will be drawn on
    /// </summary>
    public int gridSize;
    /// <summary>
    /// Represents the cross-sectional width that the border of the map will have in cells
    /// </summary>
    public int mapBorderSize;
    /// <summary>
    /// The game object which functions as the game controller for maze
    /// </summary>
    public GameObject gameContGO;

    /// <summary>
    /// The actual game controller script that is attached to the game controller object
    /// </summary>
    private MazeNewGameController gameController;
    /// <summary>
    /// The cell map that is used to generate the cell game objects for the current session
    /// </summary>
    private CellMap cellMaze;

    // Start is called before the first frame update
    public void Start()
    {
        //Calculate the camera dimensions for use by the map generator
        float camHeight = Camera.main.orthographicSize;
        float camWidth = Camera.main.aspect * camHeight;
        Vector2 camSize = new Vector2(camWidth, camHeight);

        //Initialize game controller and cell map
        gameController = gameContGO.GetComponent<MazeNewGameController>();
        Debug.Log("In MapController.Start - Creating cell map");
        cellMaze = new CellMap(camSize, gridSize, mapBorderSize);

        //Provide new custom cell lookup so that map behaves properly with maze
        //and has the proper textures
        cellMaze.UpdateCellCodeLookup(GetSpecializedLookup());
    }

    /// <summary>
    /// Used to build a new cell code lookup dictionary that meets the
    /// specifications of maze
    /// </summary>
    /// <returns>The new cell code dictionary</returns>
    public Dictionary<int, ICellBuilder> GetSpecializedLookup()
    {
        Dictionary<int, ICellBuilder> cellCodeLookup = new Dictionary<int, ICellBuilder>
        {
            //Note: the secondary parameters for codes 1 and 3 and function delegates, similar to function pointers
            { 0, new NoCollideCellBuilder("Dirt") },
            { 1, new CollideCellBuilder("Corn", gameController.AddTime) },
            { 2, new NoCollideCellBuilder("Corn") },
            { 3, new TriggerCellBuilder("FinishCellLarge", gameController.NextStage) }
        };

        return cellCodeLookup;
    }

    /// <summary>
    /// Uses the cellMap to create a brand new map for the specified difficulty
    /// and its accompanying stage
    /// </summary>
    /// <param name="difficulty">The difficulty to generate the map at</param>
    /// <param name="stage">The stage to generate the map at</param>
    /// <returns>The new start position of the player</returns>
    public Vector2 GenerateMap(int difficulty, int stage)
    {
        Debug.Log("In MapController.GenerateMap");
        //First clear the map to ensure that we don't generate maps on top of one and another
        ClearMap();
        //Get true map parameters from helper functions
        string t = GetTypeFromDifficulty(difficulty);
        int s = GetStartIndexFromDifficulty(difficulty);
        int e = GetEndIndexFromDifficulty(difficulty); 
        float w = GetWidthPercentageFromStage(stage);

        //Apply these proper parameters to the currently empty map to generate a path
        cellMaze.ApplyPath(t, w, s, e);

        //Convert the map into a list of game objects and make them children of the map parent component
        List<GameObject> cells = cellMaze.GetMapCells();
        foreach(GameObject cell in cells)
        {
            cell.transform.parent = gameObject.transform;
        }

        //if (stage < 2)
            //MazeSmartFeedbackProcessor.PathGen(cellMaze.GetNumTurns());
        //else
            //MazeSmartFeedbackProcessor.SwitchedPathGen(cellMaze.GetNumTurns());
        //Return the location of the first cell in viewport units
        return cellMaze.GetPlayerStartPointPos(); 
    }

    /// <summary>
    /// Used to destroy any child game objects and purge the code map
    /// </summary>
    private void ClearMap()
    {
        //Iterate through each of the children (cells) and destroy them
        int numToRemove = transform.childCount;
        for (int i = numToRemove; i > 0; i--)
            Destroy(transform.GetChild(i - 1).gameObject);

        //purge the code map
        cellMaze.ResetMap();
    }

    /// <summary>
    /// Helper method for choosing start position based on difficulty
    /// </summary>
    /// <param name="difficulty">The difficulty in question</param>
    /// <returns>An integer code representing an area on the code map</returns>
    private int GetStartIndexFromDifficulty(int difficulty)
    {
        switch (difficulty)
        {
            case 0:
                return CellMap.SIDE;
            case 1:
                return CellMap.BOTTOM_CORNER;
            case 2:
                return CellMap.BOTTOM_CORNER;
            default: //returning -1 will cause the map to randomly generate a start point
                return -1;
        }
    }

    /// <summary>
    /// Helper method used to determine the path end position based on the difficulty
    /// </summary>
    /// <param name="difficulty">The difficulty in question</param>
    /// <returns>An integer code representing an area on the code map</returns>
    private int GetEndIndexFromDifficulty(int difficulty)
    {
        switch (difficulty)
        {
            case 0:
                return CellMap.SIDE;
            case 1:
                return CellMap.TOP_CORNER;
            case 2:
                return CellMap.TOP_CORNER;
            default: //Just like with start, returning -1 results in a random end point
                return -1;
        }
    }

    /// <summary>
    /// Determines whether the path will be a direct or backtrtacking path based on the
    /// difficulty level
    /// </summary>
    /// <param name="difficulty">The difficulty in question</param>
    /// <returns>A string that signals what kind of path to generate</returns>
    private string GetTypeFromDifficulty(int difficulty)
    {
        switch (difficulty)
        {
            case 0:
                return "Direct";
            case 1:
                return "Direct";
            case 2:
                return "Backtrack";
            default:
                throw new ArgumentException("Difficulty not in usable range -- " + difficulty);
        }
    }

    /// <summary>
    /// Used to determine the width of the path from the stage that the user is on
    /// </summary>
    /// <param name="stage"></param>
    /// <returns>A float that represents a percentage of total map height, to be used as path with</returns>
    private float GetWidthPercentageFromStage(int stage)
    {
        //Even stages are wide and odd are narrow
        //This is because the first and third stage should be the same difficulty only with controls switched
        //It's the same with the second and fourth stages
        if (stage % 2 == 0)
            return CellMap.WIDE_PATH_MODIFIER;
        else
            return CellMap.NARROW_PATH_MODIFIER;
    }
}
