using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ICellBuilder
{
    public Sprite cellTexture;
    public Vector2 cellSize;

    public abstract GameObject BuildCell(Vector2 position, float cellWidth);
    public abstract GameObject BuildCell(Vector2 position, float cellWidth, float rotation);

    protected void SetNewTexture(string textureLocation)
    {
        cellTexture = Resources.Load<Sprite>("MapRes/" + textureLocation);
    }

    protected GameObject BuildBasicCell(Vector2 cellPos, float rotation)
    {
        GameObject basicCell = new GameObject();
        SpriteRenderer cellRend = basicCell.AddComponent<SpriteRenderer>();
        cellRend.sprite = cellTexture;
        cellRend.drawMode = SpriteDrawMode.Sliced;
        cellRend.size = cellSize;

        basicCell.transform.position = cellPos;
        basicCell.transform.rotation = Quaternion.Euler(0, 0, rotation);

        return basicCell;
    }
}
