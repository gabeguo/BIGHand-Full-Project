using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoCollideCellBuilder : ICellBuilder
{
    public NoCollideCellBuilder()
    {
        SetNewTexture("DefaultNoCollideCell");
    }

    public NoCollideCellBuilder(string textureLocation)
    {
        SetNewTexture(textureLocation);
    }

    public override GameObject BuildCell(Vector2 position, float cellWidth)
    {
        return BuildCell(position, cellWidth, 0f);
    }

    public override GameObject BuildCell(Vector2 position, float cellWidth, float rotation)
    {
        cellSize = new Vector2(cellWidth, cellWidth);
        GameObject resultant = BuildBasicCell(position, rotation);

        return resultant;
    }
}
