using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomTuple<T1, T2>
{
    public T1 x;
    public T2 y;

    public CustomTuple(T1 val1, T2 val2)
    {
        x = val1;
        y = val2;
    }

    public CustomTuple<T1, T2> Copy()
    {
        return new CustomTuple<T1, T2>(x, y);
    }

    public override string ToString()
    {
        return "(" + x + ", " + y + ")";
    }
}
