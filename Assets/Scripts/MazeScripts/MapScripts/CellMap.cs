using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CellMap
{
    /// <summary>
    /// these multipliers are used in the generation of their respective path widths
    /// when multiplied by the area of cells (in units of cells)
    /// </summary>
    public const float WIDE_PATH_MODIFIER = 0.16f, NARROW_PATH_MODIFIER = 0.12f;
    /// <summary>
    /// definitions to easier use the possible starts and ends, will also make function calls easier
    /// </summary>
    public const int BOTTOM_CORNER = 0, SIDE = 1, TOP_CORNER = 2, MIDDLE = 3;
    /// <summary>
    /// the smallest possible grid we will generate without throwing an exception, ideally, it would
    /// still be considerably larger than this
    /// </summary>
    private const int SMALLEST_POSSIBLE_GRID = 64;

    /// <summary>
    /// the width, and height, of any given cell (they're square)
    /// </summary>
    private float cellWidth;
    /// <summary>
    /// the width of the map's border, in cells
    /// </summary>
    private int mapBorderWidth;
    /// <summary>
    /// the number of cells that span the screen horizontally and vertically,
    /// respectively
    /// </summary>
    private int numHorizontalCells, numVerticalCells;
    /// <summary>
    /// the placement offset of the first cell on the map, used to calculate the position
    /// of all cells on the map
    /// </summary>
    private float horizontalPositionOffset, verticalPositionOffset;
    /// <summary>
    /// two dimensional array of cell codes, this is for easier map manipulation
    /// in comparison to changing physical GameObjects with every adjustment
    /// **cell codes are stored in CellCodeLookup dictionary
    /// </summary>
    private int[,] codeMap;
    /// <summary>
    /// lookup dictionary that maps cell codes to cell builder objects
    /// **mutable and extendable, but idiomatically these should hold:
    /// cell code 0 = path cell
    /// cell code 1 = path border cell
    /// cell code 2 = filler cell
    /// cell code 3 = trigger cell
    /// </summary>
    private Dictionary<int, ICellBuilder> cellCodeLookup;

    /// <summary>
    /// the width that the current path will be generated at (in units of cells)
    /// </summary>
    private int currentPathWidth;
    /// <summary>
    /// arrays of integer tuples representing locations in the code map array
    /// each array holds 4 tuples each corresponding to possible path start locations
    /// or end locations respectively
    /// 
    /// This is the ordering of the arrays:
    /// ------------------------
    /// | [S2]    [E3]    [E2] |
    /// |                      |
    /// | [S1]            [E1] |
    /// |                      |
    /// | [S0]    [S3]    [E0] |
    /// ------------------------
    /// </summary>
    private CustomTuple<int, int>[] possibleStarts, possibleEnds;
    /// <summary>
    /// once a path type has been selected, start and end points will be chosen from the
    /// possibleStart and possibleEnd arrays, by default these will be trigger cells
    /// </summary>
    private CustomTuple<int, int> start, end;
    /// <summary>
    /// the area encapsulated by these tuples defines all possible points that the bottom-left-most cell of
    /// any given path cell group can occupy. Ex:
    /// 
    /// [X][X]
    /// [O][X]
    /// 
    /// the [O] is the bottom-left-most cell of a single path placement with a path width of two
    /// this placement pattern limits us not only by the map size but the remaining cell group size as well
    /// </summary>
    private CustomTuple<int, int> usableAreaBotLeft, usableAreaTopRight;
    /// <summary>
    /// a list of all the cell origin points (defined for usable left and right area variables)
    /// this is used to make readjustments to a normally direct applied path in order to make it
    /// into a backtracking path
    /// </summary>
    private List<CustomTuple<int, int>> allCellOrigins;
    /// <summary>
    /// the number of turns in the current path
    /// </summary>
    private int numTurns;

    /// <summary>
    /// Constructor for CellMap, used to initialize variables needed for map and
    /// path generation.
    /// </summary>
    /// <param name="screenHalfSize">A vector2 which represents the viewport half width and half height</param>
    /// <param name="minGridSize">The minimum number of cells to be placed in the grid</param>
    /// <param name="borderSize">The width of the area around the map where paths cannot be generated</param>
    public CellMap(Vector2 screenHalfSize, int minGridSize, int borderSize)
    {
        if (borderSize < 0 || minGridSize < SMALLEST_POSSIBLE_GRID || screenHalfSize.x <= 0 || screenHalfSize.y <= 0)
            throw new ArgumentException("Cell map could not be initialized, invalid arguments were provided.");

        Debug.Log("In CellMap constructor");

        mapBorderWidth = borderSize;
        //do preliminary calculations
        DoCellSizeCalculations(screenHalfSize, minGridSize, borderSize);
        DoOffsetCalculations();
        ResetMap();

        //initialize the lookup dictionary to the specifications we expect
        cellCodeLookup = new Dictionary<int, ICellBuilder>
        {
            { 0, new NoCollideCellBuilder() },
            { 1, new CollideCellBuilder() },
            { 2, new NoCollideCellBuilder("DefaultFillerCell") },
            { 3, new TriggerCellBuilder() }
        };
    }

    /// <summary>
    /// Helper method for the constructor which calculates the number of cells to put across the screen, up
    /// and down the screen, and the size to make the cells.
    /// </summary>
    /// <param name="screenHalfSize">A vector2 representing the half width and height of the viewport</param>
    /// <param name="minGridSize">The minimum number of cells to be generated for this map</param>
    /// <param name="borderSize">The size of the area around the map where paths cannot be generated</param>
    private void DoCellSizeCalculations(Vector2 screenHalfSize, int minGridSize, int borderSize)
    {
        //aspect as y/x
        float aspect = screenHalfSize.y / screenHalfSize.x;
        //minGridSize is essentially area of viewport in cells;
        //A = xy
        //A * aspect ratio = A * y/x = xy * y/x = y^2
        //So: sqrt of minGridSize * aspect = sqrt(y^2) = y
        float rawYCells = Mathf.Sqrt(minGridSize * aspect);
        //A = xy
        //A / y = x
        float rawXCells = minGridSize / rawYCells;

        //we ceil the number of cells vertically and horizontally to maintain minimum
        //cell number requirements (we also add the borders into the calculation)
        //borderSize is multiplied by 2 because it appears on both sides of the map
        numVerticalCells = Mathf.CeilToInt(rawYCells) + (2 * borderSize);
        numHorizontalCells = Mathf.CeilToInt(rawXCells) + (2 * borderSize);

        //we calculate cellWidth as the larger of a perfect horizontal or vertical fit
        float vertFit = screenHalfSize.y * 2 / numVerticalCells;
        float horizFit = screenHalfSize.x * 2 / numHorizontalCells;
        cellWidth = Mathf.Max(vertFit, horizFit);
        //Debug.Log("CellWidth: " + cellWidth);
    }

    /// <summary>
    /// Used to calculate offsets that are required to center the map, this is needed because cell size
    /// is calculated to make morizontal and vertical directions slightly longer than needed
    /// </summary>
    private void DoOffsetCalculations()
    {
        //multiply by cellWidth to get the offset in the correct units at the correct
        //magnitude
        //formula is effectively (# - 1) / 2
        horizontalPositionOffset = cellWidth * ((numHorizontalCells - 1f) / 2f);
        verticalPositionOffset = cellWidth * ((numVerticalCells - 1f) / 2f);
    }

    /// <summary>
    /// Used to calculate the position of any given cell from the code map array
    /// using all appropriate offsets
    /// </summary>
    /// <param name="x">X position in the code map array</param>
    /// <param name="y">Y position in the code map array</param>
    /// <returns>Vector2 position in proper units</returns>
    private Vector2 GetCellPosition(int x, int y)
    {
        if (x >= codeMap.GetLength(0) || y >= codeMap.GetLength(1) || x < 0 || y < 0)
            throw new ArgumentException("The position of this cell should not be needed as it does not exist -- " + x + ", " + y);

        float xPos = -horizontalPositionOffset + (x * cellWidth);
        float yPos = verticalPositionOffset - (y * cellWidth);

        return new Vector2(xPos, yPos);
    }

    /// <summary>
    /// Compiles the map into a list of cell GameObjects. Useful for adding all map
    /// components as children in one fell swoop.
    /// </summary>
    /// <returns></returns>
    public List<GameObject> GetMapCells()
    {
        List<GameObject> cells = new List<GameObject>();

        for (int i = 0; i < numHorizontalCells; i++)
        {
            for (int j = 0; j < numVerticalCells; j++)
            {
                //We can expect that the cellCode maps to an appropriate cell builder
                cells.Add(cellCodeLookup[codeMap[i, j]].BuildCell(GetCellPosition(i, j), cellWidth));
            }
        }

        return cells;
    }

    /// <summary>
    /// Used to update the code lookup dictionary to a different non-default dictionary
    /// </summary>
    /// <param name="newDict">The new dictionary to take the place of cellCodeLookup</param>
    public void UpdateCellCodeLookup(Dictionary<int, ICellBuilder> newDict)
    {
        cellCodeLookup = newDict;
    }

    /// <summary>
    /// Used to generate a path pattern with the given specifications of a start and end point,
    /// path type, and path width.
    /// </summary>
    /// <param name="pathType">The type of path to be generated</param>
    /// <param name="widthPercentage">The perrcentage of the overall area that the path width will be</param>
    /// <param name="startPointIndex">The index of a point in the path start options array</param>
    /// <param name="endPointIndex">The index of a point in the path end options array</param>
    public void ApplyPath(string pathType, float widthPercentage, int startPointIndex, int endPointIndex)
    {
        CalculateTruePathWidth(widthPercentage);
        CalculateUsableAreaValues();

        //Debug.Log("StartIndex: " + startPointIndex + " EndIndex: " + endPointIndex);
        //Debug.Log(possibleStarts[startPointIndex].x + ", " + possibleStarts[startPointIndex].y);
        //Debug.Log(possibleEnds[endPointIndex].x + ", " + possibleEnds[endPointIndex].y);

        //if indices are out of range, use random start and end points
        //idiomatically, we should send a -1 in order to get this behavior
        if (startPointIndex < 0 || startPointIndex > 3)
            startPointIndex = UnityEngine.Random.Range(0, 4);
        if (endPointIndex < 0 || endPointIndex > 3)
            endPointIndex = UnityEngine.Random.Range(0, 4);

        start = possibleStarts[startPointIndex];
        end = possibleEnds[endPointIndex];

        switch (pathType)
        {
            case "Direct":
                GenerateDirectPath();
                break;
            case "Backtrack":
                GenerateBackTrack();
                break;
            default:
                throw new ArgumentException("Path type parameter unrecognized -- " + pathType);
        }

        //we set the last block to trigger for scene changes and end of game triggers
        SetEndPosToTrigger();
    }

    /// <summary>
    /// Used to convert the width percentage into proper cell units
    /// </summary>
    /// <param name="widthPercentage">The percentage of the cell area that will compose the path width</param>
    private void CalculateTruePathWidth(float widthPercentage)
    {
        if (widthPercentage <= 0f)
            throw new ArgumentException("Provided path width percentage is too low -- " + widthPercentage);

        currentPathWidth = Mathf.CeilToInt(widthPercentage * numVerticalCells);
        //Debug.Log("PathWidth: " + currentPathWidth);
    }

    /// <summary>
    /// Used to generate a direct path with no backtracking from the defined start and end locations
    /// </summary>
    private void GenerateDirectPath()
    {
        //setup list of directional transformations
        List<CustomTuple<int, int>> transformations = new List<CustomTuple<int, int>>();
        CalculateSingleDimensionTransforms(transformations, new CustomTuple<int, int>(1, 0), end.x - start.x);
        CalculateSingleDimensionTransforms(transformations, new CustomTuple<int, int>(0, 1), end.y - start.y);
        ScrambleList(transformations);

        //iterate through the scrambled transformations applying each cell group as we go
        CustomTuple<int, int> currentCellPos = new CustomTuple<int, int>(start.x, start.y);
        CustomTuple<int, int> lastTrans = new CustomTuple<int, int>(-1, -1);
        foreach (CustomTuple<int, int> t in transformations)
        {
            if (t.x != lastTrans.x || t.y != lastTrans.y)
                numTurns += 1;
            lastTrans = t;
            //Debug.Log("Apply Cell Group At: " + currentCellPos.x + ", " + currentCellPos.y);
            ApplyCellGroupToCodeMap(currentCellPos);
            currentCellPos.x += t.x;
            currentCellPos.y += t.y;
        }
        //For final block to be included
        ApplyCellGroupToCodeMap(currentCellPos);
    }

    /// <summary>
    /// Generates a direct path then applies a single adjustment to it
    /// turning it into a backtracking path
    /// </summary>
    private void GenerateBackTrack()
    {
        Debug.Log("-----In Generate Back Track-----");
        //the list of path transformations to make
        List<CustomTuple<int, int>> transformations = new List<CustomTuple<int, int>>();
        //whether or not the adjustment has been made yet
        bool adjustmentMade = false;
        //the needed space for the path adjustment we are making
        int neededRemovalSpace = 4 * currentPathWidth;
        //the min height for an adjustment to take place
        int minNecessaryHeight = mapBorderWidth + currentPathWidth;
        //the min width for an adjustment to take place
        int minNecessaryWidth = mapBorderWidth + currentPathWidth;
        //the max height for an adjustment to take place
        int maxNecessaryHeight = end.y - neededRemovalSpace / 2;
        //the max width for an adjustment to take place
        int maxNecessaryWidth = end.x - neededRemovalSpace;
        //the number of direct x transforms
        int normalXTransforms = end.x - start.x - neededRemovalSpace;
        //the number of direct y transforms
        int normalYTransforms = end.y - start.y;

        Debug.Log("Needed Removal Space: " + neededRemovalSpace);
        Debug.Log("Min Necessary Width: " + minNecessaryWidth);
        Debug.Log("Max Necessary Width: " + maxNecessaryWidth);
        Debug.Log("Min Necessary Height: " + minNecessaryHeight);
        Debug.Log("Max Necessary Height: " + maxNecessaryHeight);
        Debug.Log("NormalXTransforms: " + normalXTransforms);
        Debug.Log("NormalYTransforms: " + normalYTransforms);

        //check that the path can support backtracking
        if (normalXTransforms <= 0 || normalYTransforms <= neededRemovalSpace / 2)
        {
            Debug.Log("Too small of a grid for backtracking, generating direct");
            GenerateDirectPath();
            return;
        }

        CalculateSingleDimensionTransforms(transformations, new CustomTuple<int, int>(1, 0), normalXTransforms);
        CalculateSingleDimensionTransforms(transformations, new CustomTuple<int, int>(0, 1), normalYTransforms);
        ScrambleList(transformations);

        //iterate through the scrambled transformations applying each cell group as we go
        CustomTuple<int, int> currentCellPos = new CustomTuple<int, int>(start.x, start.y);
        CustomTuple<int, int> lastTrans = new CustomTuple<int, int>(-1, -1);
        for (int i = 0; i < transformations.Count; i++)
        {
            if (transformations[i].x != lastTrans.x || transformations[i].y != lastTrans.y)
                numTurns += 1;
            lastTrans = transformations[i];
            CustomTuple<int, int> t = transformations[i];
            //Debug.Log("Apply Cell Group At: " + currentCellPos.x + ", " + currentCellPos.y);
            ApplyCellGroupToCodeMap(currentCellPos);
            currentCellPos.x += t.x;
            currentCellPos.y += t.y;

            //if we need to make the adjustment still and we are in the designated area, draw the adjustment
            if (!adjustmentMade && i >= 2
                && currentCellPos.x >= minNecessaryWidth && currentCellPos.y >= minNecessaryHeight
                && currentCellPos.x < maxNecessaryWidth && currentCellPos.y < maxNecessaryHeight)
            {
                adjustmentMade = true;
                MakeTransitionalInsertions(i + 1, transformations);
                numTurns += 4;
            }
        }
        //apply final cell to code map
        ApplyCellGroupToCodeMap(currentCellPos);

        //if the adjustment was never made, regenerate the map
        if (!adjustmentMade)
        {
            Debug.Log("Backtrack map failure");
            ResetMap();
            GenerateBackTrack();
        }  
    }

    /// <summary>
    /// Draws a single map adjustment using transformation vectors on a given list
    /// </summary>
    /// <param name="i">index to insert adjustment at</param>
    /// <param name="t">the list of transformations to insert into</param>
    private void MakeTransitionalInsertions(int i, List<CustomTuple<int, int>> t)
    {
        i = AddDir(i, 2, t);    //up
        i = AddDir(i, 0, t);    //right
        i = AddDir(i, 0, t);    //right
        i = AddDir(i, 3, t);    //down
        i = AddDir(i, 0, t);    //right
        i = AddDir(i, 0, t);    //right
    }

    /// <summary>
    /// Generates a single standard transformation vector and adds it at the desired index
    /// of a given list of transformation vectors
    /// </summary>
    /// <param name="i">the index to add the transformation at</param>
    /// <param name="d">the direction code</param>
    /// <param name="t">the list of transformation vectors to add to</param>
    /// <returns>i incremented by 1 (the proceeding index of the given one)</returns>
    private int AddDir(int i, int d, List<CustomTuple<int, int>> t)
    {
        /* direction codes:
         * 0 = right
         * 1 = left
         * 2 = up
         * 3 = down
         */
        switch (d)
        {
            case 0:
                t.Insert(i, new CustomTuple<int, int>(currentPathWidth, 0));
                break;
            case 1:
                t.Insert(i, new CustomTuple<int, int>(-currentPathWidth, 0));
                break;
            case 2:
                t.Insert(i, new CustomTuple<int, int>(0, currentPathWidth));
                break;
            case 3:
                t.Insert(i, new CustomTuple<int, int>(0, -currentPathWidth));
                break;
            default:
                return i;
        }

        return i + 1;
    }

    /// <summary>
    /// Used to determine if a given cell is a path cell
    /// </summary>
    /// <param name="pos">The codeMap position of the cell</param>
    /// <returns>True if the cell is a path or trigger cell, false otherwise</returns>
    private bool CellIsPath(CustomTuple<int, int> pos)
    {
        if (pos == null)
            return false;

        return codeMap[pos.x, pos.y] == 0 || codeMap[pos.x, pos.y] == 3;
    }

    /// <summary>
    /// Applies a cell group pattern to the codemap as a method of path generation,
    /// or at least generating paths piece by piece in pathSize increments
    /// </summary>
    /// <param name="cellLoc">The bottom left corner of the cell group to be applied</param>
    private void ApplyCellGroupToCodeMap(CustomTuple<int, int> cellLoc)
    {
        ApplyCellGroupToCodeMap(cellLoc, -1);
    }

    /// <summary>
    /// Applies a cell group pattern to the codemap as a method of path generation,
    /// or at least generating paths piece by piece in pathSize increments
    /// </summary>
    /// <param name="cellLoc">The bottom left corner of the cell group to be applied</param>
    /// <param name="originIndex">The index at which to add the next cell, -1 means on the end</param>
    private void ApplyCellGroupToCodeMap(CustomTuple<int, int> cellLoc, int originIndex)
    {
        if (originIndex == -1)
            allCellOrigins.Add(cellLoc.Copy());
        else
            allCellOrigins.Insert(originIndex, cellLoc.Copy());

        //iterate through the entire cell group, border included
        for (int i = cellLoc.x - 1; i < cellLoc.x + currentPathWidth + 1; i++)
        {
            for (int j = cellLoc.y - 1; j < cellLoc.y + currentPathWidth + 1; j++)
            {
                //if the current cell is on the border of this group try to make it a border cell
                if (i == cellLoc.x - 1 || i == cellLoc.x + currentPathWidth ||
                    j == cellLoc.y - 1 || j == cellLoc.y + currentPathWidth)
                {
                    codeMap[i, j] = GetPrevailingCode(codeMap[i, j], 1);
                }
                else //if not, try to make it a path cell
                {
                    codeMap[i, j] = GetPrevailingCode(codeMap[i, j], 0);
                }
            }
        }
    }

    /// <summary>
    /// Simply determines which codes override which other codes
    /// </summary>
    /// <param name="code1">The first code for comparison</param>
    /// <param name="code2">The second code for comparison</param>
    /// <returns>The dominant of the two codes</returns>
    private int GetPrevailingCode(int code1, int code2)
    {
        if (code1 < code2)
            return code1;
        else
            return code2;
    }

    /// <summary>
    /// Used to add all necessary transformations for a single direction
    /// </summary>
    /// <param name="trans">The transformation list</param>
    /// <param name="multiplier">A multiplier which effectively determines the direction that is currently being calculated</param>
    /// <param name="remainingSpace">The distance across the usable area in the direction being currently calculated</param>
    private void CalculateSingleDimensionTransforms(List<CustomTuple<int, int>> trans, CustomTuple<int, int> multiplier, int remainingSpace)
    {
        while (remainingSpace > 0)
        {
            remainingSpace -= currentPathWidth;
            int currentTransform = currentPathWidth;

            if (remainingSpace < 0)
            {
                currentTransform += remainingSpace;
                remainingSpace -= remainingSpace;
            }

            trans.Add(new CustomTuple<int, int>(currentTransform * multiplier.x, currentTransform * multiplier.y));
        }
    }

    /// <summary>
    /// Used to randomly scramble a list of elements
    /// </summary>
    /// <param name="target">The list to be scrambled</param>
    private void ScrambleList(List<CustomTuple<int, int>> target)
    {
        for (int i = 0; i < target.Count; i++)
        {
            int randIndex = UnityEngine.Random.Range(0, target.Count);
            CustomTuple<int, int> temp = target[i];
            target[i] = target[randIndex];
            target[randIndex] = temp;
        }
    }

    /// <summary>
    /// Used to determine the usable area and fill in the possible start and end point arrays
    /// </summary>
    private void CalculateUsableAreaValues()
    {
        int maxX = numHorizontalCells - (currentPathWidth + mapBorderWidth + 1);
        int maxY = numVerticalCells - (currentPathWidth + mapBorderWidth + 1);

        usableAreaBotLeft = new CustomTuple<int, int>(mapBorderWidth + 1, mapBorderWidth + 1);
        usableAreaTopRight = new CustomTuple<int, int>(maxX, maxY);

        possibleStarts = new CustomTuple<int, int>[4];
        possibleEnds = new CustomTuple<int, int>[4];

        possibleStarts[BOTTOM_CORNER] = new CustomTuple<int, int>(usableAreaBotLeft.x, usableAreaBotLeft.y);
        possibleStarts[SIDE] = new CustomTuple<int, int>(usableAreaBotLeft.x, (usableAreaBotLeft.y + usableAreaTopRight.y) / 2);
        possibleStarts[TOP_CORNER] = new CustomTuple<int, int>(usableAreaBotLeft.x, usableAreaTopRight.y);
        possibleStarts[MIDDLE] = new CustomTuple<int, int>((usableAreaBotLeft.x + usableAreaTopRight.x) / 2, usableAreaBotLeft.y);

        possibleEnds[BOTTOM_CORNER] = new CustomTuple<int, int>(usableAreaTopRight.x, usableAreaBotLeft.y);
        possibleEnds[SIDE] = new CustomTuple<int, int>(usableAreaTopRight.x, (usableAreaBotLeft.y + usableAreaTopRight.y) / 2);
        possibleEnds[TOP_CORNER] = new CustomTuple<int, int>(usableAreaTopRight.x, usableAreaTopRight.y);
        possibleEnds[MIDDLE] = new CustomTuple<int, int>((usableAreaBotLeft.x + usableAreaTopRight.x) / 2, usableAreaTopRight.y);
    }

    /// <summary>
    /// Used to set the code map back to its default value
    /// </summary>
    public void ResetMap()
    {
        numTurns = 0;
        allCellOrigins = new List<CustomTuple<int, int>>();
        codeMap = new int[numHorizontalCells, numVerticalCells];
        for (int i = 0; i < codeMap.GetLength(0); i++)
        {
            for (int j = 0; j < codeMap.GetLength(1); j++)
            {
                codeMap[i, j] = 2;
            }
        }
    }

    /// <summary>
    /// Gets the starting point game coordinates, useful for a variety of things
    /// namely player start point
    /// </summary>
    /// <returns>The game coords of the center of the first cell</returns>
    public Vector2 GetPlayerStartPointPos()
    {
        Vector2 startPos = GetCellPosition(start.x, start.y);
        float centeringVar = (currentPathWidth - 1) / 2 * cellWidth;
        return new Vector2(startPos.x + centeringVar, startPos.y - centeringVar);
    }

    /// <summary>
    /// Used to set the end cells to a trigger so that the end of the game can be signaled
    /// </summary>
    public void SetEndPosToTrigger()
    {
        for (int i = end.x; i < end.x + currentPathWidth; i++)
        {
            for (int j = end.y; j < end.y + currentPathWidth; j++)
            {
                //map code 3 corresponds to a trigger cell
                codeMap[i, j] = 3;
            }
        }
    }

    public int GetNumTurns()
    {
        return numTurns;
    }
}
