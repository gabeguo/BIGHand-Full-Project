using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cell builder for making trigger cells, these are useful for event handling mid game
/// </summary>
public class TriggerCellBuilder : ICellBuilder
{
    /// <summary>
    /// function delegate for the action to be taken when the cell is triggered
    /// </summary>
    private Func<Collider2D, bool> TriggerAction = null;

    /// <summary>
    /// constructor, sets default texture
    /// </summary>
    public TriggerCellBuilder()
    {
        SetNewTexture("DefaultTriggerCell");
    }

    /// <summary>
    /// overload constructor, specifies a custom trigger cell texture
    /// </summary>
    /// <param name="textureLocation">the custom trigger texture's location
    /// in the file path</param>
    public TriggerCellBuilder(string textureLocation)
    {
        SetNewTexture(textureLocation);
    }

    /// <summary>
    /// overload constructor, sets custom trigger texture and trigger action
    /// </summary>
    /// <param name="textureLocation">the custom texture's location in the file path</param>
    /// <param name="del">the new function delegate for trigger actions to use</param>
    public TriggerCellBuilder(string textureLocation, Func<Collider2D, bool> del)
    {
        SetNewTexture(textureLocation);
        TriggerAction = del;
    }

    /// <summary>
    /// used to construct a usable game object fitting the builder's specifications
    /// </summary>
    /// <param name="position">the vector2 position that the cell is built at</param>
    /// <param name="cellWidth">the size of the cell to build</param>
    /// <returns></returns>
    public override GameObject BuildCell(Vector2 position, float cellWidth)
    {
        return BuildCell(position, cellWidth, 0f);
    }

    /// <summary>
    /// overload cell build method, specifies a rotation in the case of a non standard cell layout
    /// </summary>
    /// <param name="position">the position to place the cell at</param>
    /// <param name="cellWidth">the size of the cell</param>
    /// <param name="rotation">the rotation to be applied to the cell in degrees</param>
    /// <returns></returns>
    public override GameObject BuildCell(Vector2 position, float cellWidth, float rotation)
    {
        //Debug.Log("Trigger Cell Spawned: " + position.ToString());

        cellSize = new Vector2(cellWidth, cellWidth);
        GameObject resultant = BuildBasicCell(position, rotation);

        BoxCollider2D cellColl = resultant.AddComponent<BoxCollider2D>();
        cellColl.size = cellSize;
        CellTriggerController cellTrigCont = resultant.AddComponent<CellTriggerController>();
        cellColl.isTrigger = true;
        if (TriggerAction != null)
            cellTrigCont.SetDelegateAction(TriggerAction);

        //Debug.Log("Building trigger cell at: " + position.x + ", " + position.y);
        return resultant;
    }
}
