using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HexDirection
{
	N, NE, SE, S, SW, NW
}

public static class HexDirectionExtensions
{
	public static HexDirection Opposite (this HexDirection direction)
    {
        if ((int)direction < 3)
            return (direction + 3);
        else
            return (direction - 3);
	}

	public static HexDirection Previous (this HexDirection direction)
    {
        if (direction == HexDirection.N)
            return (HexDirection.NW);
        else
		    return (direction - 1);
	}

	public static HexDirection Next (this HexDirection direction)
    {
		if (direction == HexDirection.NW)
            return (HexDirection.N);
        else
		    return (direction + 1);
	}
}