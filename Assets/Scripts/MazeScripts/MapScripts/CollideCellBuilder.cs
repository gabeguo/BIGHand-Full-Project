using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollideCellBuilder : ICellBuilder
{
    private Func<Collision2D, bool> CollisionAction = null;

    public CollideCellBuilder()
    {
        SetNewTexture("DefaultCollideCell");
    }

    public CollideCellBuilder(string textureLocation)
    {
        SetNewTexture(textureLocation);
    }

    public CollideCellBuilder(string textureLocation, Func<Collision2D, bool> del)
    {
        SetNewTexture(textureLocation);
        CollisionAction = del;
    }

    public override GameObject BuildCell(Vector2 position, float cellWidth)
    {
        return BuildCell(position, cellWidth, 0f);
    }

    public override GameObject BuildCell(Vector2 position, float cellWidth, float rotation)
    {
        cellSize = new Vector2(cellWidth, cellWidth);
        GameObject resultant = BuildBasicCell(position, rotation);

        BoxCollider2D cellColl = resultant.AddComponent<BoxCollider2D>();
        cellColl.size = cellSize;
        CellColliderController cellCollCont = resultant.AddComponent<CellColliderController>();
        if (CollisionAction != null)
            cellCollCont.SetDelegateAction(CollisionAction);

        return resultant;
    }
}
