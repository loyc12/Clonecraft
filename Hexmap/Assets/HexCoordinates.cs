using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct HexCoordinates
{

	[SerializeField]
	private int x, z;

	public int X
	{
		get {
			return x;
		}
	}

	public int Z
	{
		get {
			return z;
		}
	}

	public int Y
	{
		get {
			return X + Z;
		}
	}

	public HexCoordinates (int x, int z)
	{
		this.x = x;
		this.z = z;
	}

	public static HexCoordinates FromOffsetCoordinates (int x, int z)
	{
		return new HexCoordinates(x, z - x / 2);
	}

	public static HexCoordinates FromPosition (Vector3 position)
	{
		float x = position.x / (HexMetrics.outerRadius * 1.5f);
		float offset =  (x * 0.5f) - (x / 2);
		float z = (position.z / (HexMetrics.innerRadius * 2f)) - offset;
		z -= position.x / (HexMetrics.outerRadius * 3f);
		float y = x + z;

		int iX = Mathf.RoundToInt(x);
		int iY = Mathf.RoundToInt(y);
		int iZ = Mathf.RoundToInt(z);

		if (iX + iZ - iY != 0)
		{
			float dX = Mathf.Abs(x - iX);
			float dY = Mathf.Abs(y - iY);
			float dZ = Mathf.Abs(z - iZ);
			if (dZ > dY && dZ > dX) {
				iZ = iY - iX;
			}
			else if (dX > dY) {
				iX = iY - iZ;
			}
		}

		return new HexCoordinates(iX, iZ);
	}

	public override string ToString ()
	{
		int W = Z + (X / 2);
		return "(" +
			X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() +
			")     [" + X.ToString() + ", " + W.ToString() + "]";
	}

	public string ToStringOnSeparateLines ()
	{
		return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
	}
}
