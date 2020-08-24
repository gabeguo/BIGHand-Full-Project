using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellGroup
{
    //bottom left cell of group
    private CustomTuple<int, int> position;
    //width and height of group [must both be greater than 0]
    private CustomTuple<int, int> size;
    //the top right cell of the group
    private CustomTuple<int, int> final;

    public CellGroup()
    {
        SetPos(new CustomTuple<int, int>(0, 0));
        SetSize(new CustomTuple<int, int>(1, 1));
    }

    public CellGroup(CustomTuple<int, int> pos)
    {
        SetPos(pos);
        SetSize(new CustomTuple<int, int>(1, 1));
    }

    public CellGroup(CustomTuple<int, int> pos, CustomTuple<int, int> s)
    {
        SetPos(pos);
        SetSize(s);
    }

    public bool SetSize(CustomTuple<int, int> s)
    {
        if (s.x > 0 && s.y > 0)
        {
            size = new CustomTuple<int, int>(s.x, s.y);
            CalculateFinal();
            return true;
        }

        return false;
    }

    public bool SetPos(CustomTuple<int, int> pos)
    {
        position = pos.Copy();
        CalculateFinal();
        return true;
    }

    public bool CellIsInGroup(CustomTuple<int, int> pos)
    {
        bool xInRange = pos.x >= position.x && pos.x <= final.x;
        bool yInRange = pos.y >= position.y && pos.y <= final.y;

        if (xInRange && yInRange)
            return true;
        return false;
    }

    public bool GroupIsInRange(CellGroup group)
    {
        if (CellIsInGroup(group.position) && CellIsInGroup(group.final))
            return true;

        return false;
    }

    private void CalculateFinal()
    {
        if (final == null)
            final = new CustomTuple<int, int>(0, 0);
        if (size == null)
            size = new CustomTuple<int, int>(1, 1);
        if (position == null)
            position = new CustomTuple<int, int>(0, 0);

        final.x = position.x + size.x - 1;
        final.y = position.y + size.y - 1;
    }

    public int GetWidth()
    {
        return size.x;
    }

    public int GetHeight()
    {
        return size.y;
    }

    public CustomTuple<int, int> GetStart()
    {
        return position.Copy();
    }

    public CustomTuple<int, int> GetEnd()
    {
        return final.Copy();
    }

    public CustomTuple<int, int> GetCell(CustomTuple<int, int> offSet)
    {
        return GetCell(offSet.x, offSet.y);
    }

    public CustomTuple<int, int> GetCell(int xOff, int yOff)
    {
        CustomTuple<int, int> result = new CustomTuple<int, int>(position.x + xOff, position.y + yOff);
        if (CellIsInGroup(result))
            return result;
        return null;
    }

    public List<CustomTuple<int, int>> Intersect(CellGroup otherGroup)
    {
        List<CustomTuple<int, int>> result = new List<CustomTuple<int, int>>();

        int xStartOffset = position.x - otherGroup.position.x;
        int yStartOffset = position.y - otherGroup.position.y;

        int xOverlap, yOverlap;
        int xStart, yStart;

        if (xStartOffset >= 0)  //current is to the right of or at other
        {
            xOverlap = CalculatePreOverlap(otherGroup.size.x, position.x, otherGroup.position.x);
            xStart = position.x;
        }
        else                    //current is to the left of other
        {
            xOverlap = CalculatePostOverlap(size.x, position.x, otherGroup.position.x);
            xStart = position.x - (xOverlap - size.x);
        }

        if (yStartOffset >= 0)  //current is above or at other
        {
            yOverlap = CalculatePreOverlap(otherGroup.size.y, position.y, otherGroup.position.y);
            yStart = position.y;
        }
        else                    //current is below other
        {
            yOverlap = CalculatePostOverlap(size.y, position.y, otherGroup.position.y);
            yStart = position.y - (yOverlap - size.y);
        }

        if (xOverlap <= 0 | yOverlap <= 0)
            return null;

        for (int i = 0; i < xOverlap; i++)
        {
            for (int j = 0; j < yOverlap; j++)
            {
                result.Add(new CustomTuple<int, int>(xStart + i, yStart + j));
            }
        }

        return result;
    }

    private int CalculatePreOverlap(int otherSize, int thisPos, int otherPos)
    {
        return otherSize - (thisPos - otherPos);
    }

    private int CalculatePostOverlap(int thisSize, int thisPos, int otherPos)
    {
        return thisSize - (thisPos + thisSize - otherPos);
    }
}
