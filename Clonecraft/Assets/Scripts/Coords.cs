using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
pos			generalized
worldPos	blocks (relative to world)
blockPos	blocks (relative to chunk)
chunkPos	chunks (relative to world)
*/

//coordinate system to avoid using floats
public class Coords
{
	public int	x;
	public int	y;
	public int	z;

	public	Coords (int _x, int _y, int _z)
	{
		x = _x;
		y = _y;
		z = _z;
	}

	public Coords	GetNeighbor(int faceIndex)
	{
		Coords pos = new Coords(this.x, this.y, this.z);

		if (faceIndex == 0)
			pos.z -= 1;
		else if (faceIndex == 1)
			pos.z += 1;
		else if (faceIndex == 2)
			pos.y += 1;
		else if (faceIndex == 3)
			pos.y -= 1;
		else if (faceIndex == 4)
			pos.x -= 1;
		else if (faceIndex == 5)
			pos.x += 1;
		else
		{
			Debug.Log("Error in GetNeighbor : invalid face index given");
			return (null);
		}
		return (pos);
	}

	public float	SphereDistance(Coords pos)
	{
		int	x = pos.x - this.x;
		int	y = pos.y - this.y;
		int	z = pos.z - this.z;

		int	dsquare = (x * x) + (y * y) + (z * z);

		return (Mathf.Sqrt(dsquare));
	}

	public Coords	AddPos(Coords pos)
	{
		int	x = pos.x + this.x;
		int	y = pos.y + this.y;
		int	z = pos.z + this.z;

		return (new Coords(x, y, z));
	}

	public Vector3	ToVector3()
	{
		return (new Vector3(this.x, this.y, this.z));
	}

	public Vector2	ToVector2()
	{
		return (new Vector2(this.x, this.z));
	}
}

