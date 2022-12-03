using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
pos				generalized pos
vPos			Vector (relative to world, in float)
worldPos		blocks (relative to world, in voxel increments)
blockPos		blocks (relative to chunk, in voxel increments)
chunkPos		chunks (relative to world, in chunk increments)
chunkWorldPos	chunks (relative to world, in voxel increments)		
*/

//coordinate system to avoid using floats
public class Coords
{
	public int	x;
	public int	y;
	public int	z;

	public	Coords ()
	{
		x = 0;
		y = 0;
		z = 0;
	}
	public	Coords (int _x, int _y, int _z)
	{
		x = _x;
		y = _y;
		z = _z;
	}
	public	Coords (Coords pos)
	{
		x = pos.x;
		y = pos.y;
		z = pos.z;
	}
	public	Coords (float _x, float _y, float _z)
	{
		x = Mathf.FloorToInt(_x);
		y = Mathf.FloorToInt(_y);
		z = Mathf.FloorToInt(_z);
	}
	public	Coords (Vector3 vPos)
	{
		x = Mathf.FloorToInt(vPos.x);
		y = Mathf.FloorToInt(vPos.y);
		z = Mathf.FloorToInt(vPos.z);
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

	public float	CubeDistance(Coords other)
	{
		int	x = this.x - other.x;
		int	y = this.y - other.y;
		int	z = this.z - other.z;

		return (Mathf.Max(Mathf.Abs(x), Mathf.Max(Mathf.Abs(y), Mathf.Abs(z))));
	}

	public float	SphereDistance(Coords other)
	{
		int	x = this.x - other.x;
		int	y = this.y - other.y;
		int	z = this.z - other.z;

		int	dsquare = (x * x) + (y * y) + (z * z);

		return (Mathf.Sqrt(dsquare));
	}

	public Coords	AddPos(Coords other)
	{
		int	x = this.x + other.x;
		int	y = this.y + other.y;
		int	z = this.z + other.z;

		return (new Coords(x, y, z));
	}

	public Coords	SubPos(Coords other)
	{
		int	x = this.x - other.x;
		int	y = this.y - other.y;
		int	z = this.z - other.z;

		return (new Coords(x, y, z));
	}

	public Coords	MulPos(float factor)
	{
		int	x = Mathf.FloorToInt(this.x * factor);
		int	y = Mathf.FloorToInt(this.y * factor);
		int	z = Mathf.FloorToInt(this.z * factor);

		return (new Coords(x, y, z));
	}

	public Coords	DivPos(float factor)
	{
		int	x = Mathf.FloorToInt(this.x / factor);
		int	y = Mathf.FloorToInt(this.y / factor);
		int	z = Mathf.FloorToInt(this.z / factor);

		return (new Coords(x, y, z));
	}

	public bool	SamePos(Coords other)
	{
		//if (this == null && pos == null )
			//return (true);

		if (this == null  || other == null )
			return (false);

		else if (this.x == other.x && this.y == other.y && this.z == other.z)
			return (true);

		return (false);
	}

	public Vector3	ToVector3()
	{
		return (new Vector3(this.x, this.y, this.z));
	}
	/*
	public Vector2	ToVector2()
	{
		return (new Vector2(this.x, this.z));
	}
	*/
}

