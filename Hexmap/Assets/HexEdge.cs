using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HexEdge
{
	Flat, Slope, Hill, Cliff
}

public static class HexEdgeExtensions
{
	public static HexEdge GetEdgeType (int elevation1, int elevation2)
	{
		if (elevation1 == elevation2) {
			return HexEdge.Flat;
		}
		int delta = elevation2 - elevation1;
		if (delta == 1 || delta == -1) {
			return HexEdge.Slope;
		}
		if (delta == 2 || delta == -2) {
			return HexEdge.Hill;
		}
		return HexEdge.Cliff;
	}

	static string GetEdgeLetter (int elevation1, int elevation2)
	{
		if (elevation1 == elevation2)
		{
			return "F";
		}
		int delta = elevation2 - elevation1;
		if (delta == 1 || delta == -1)
		{
			return "S";
		}
		if (delta == 2 || delta == -2)
		{
			return "H";
		}
		return "C";
	}

	public static string GetCornerType(int e1, int e2, int e3)
	{								// From lowest to highest
		return GetEdgeLetter(e1, e2) + GetEdgeLetter(e2, e3) + GetEdgeLetter(e1, e3);
	}

	public static void ProcessCorner(HexCell c1, HexCell c2, HexCell c3)
	{
		String CornerType = GetCornerType(c1.elevation, c2.Elevation, c3.Elevation);
		if (CornerType == "")
		{

		}
		else
		{
			
		}
	}

}