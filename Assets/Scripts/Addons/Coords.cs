using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
pos				generalized pos
rPos			(relative to something unspecified)
vPos			Vector (relative to world, in float)
worldPos		blocks (relative to world, in Block increments)
blockPos		blocks (relative to chunk, in Block increments)
chunkPos		chunks (relative to world, in chunk increments)
chunkWorldPos	chunks (relative to world, in Block increments)
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

	//Random Methods

	//Returns a random float value between 0 and 1 (height dependent)
	public float	GetRandomValue(World world)
	{
		float	scale = WorldData.minScale;
		return (Noise.Get3DNoise(world, this.ToVector3(), world.randomOffset, scale, scale));
	}
	//Returns a random float value between 0 and 1 (height independent)
	public float	GetRandomHeight(World world)
	{
		return (Noise.Get2DNoise(world, this.ToVector2(), world.randomOffset, WorldData.minScale));
	}

	//Distance Methods

	public float	VerticalDistance(Coords other)
	{
		int	y = Mathf.Abs(this.y - other.y);

		return (y);
	}

	public float	SquarelDistance(Coords other)
	{
		int	x = Mathf.Abs(this.x - other.x);
		int	z = Mathf.Abs(this.z - other.z);

		return (Mathf.Max(x, z));
	}
	public float	CubeDistance(Coords other)
	{
		int	y = Mathf.Abs(this.y - other.y);
		int	x = Mathf.Abs(this.x - other.x);
		int	z = Mathf.Abs(this.z - other.z);

		return (Mathf.Max(y, Mathf.Max(x, z)));
	}

	public float	CircleDistance(Coords other)
	{
		int	x = this.x - other.x;
		int	z = this.z - other.z;

		int	dsquare = (x * x) + (z * z);

		return (Mathf.Sqrt(dsquare));
	}
	public float	SphereDistance(Coords other)
	{
		int	y = this.y - other.y;
		int	x = this.x - other.x;
		int	z = this.z - other.z;

		int	dsquare = (y * y) + (x * x) + (z * z);

		return (Mathf.Sqrt(dsquare));
	}

	//Math Methods

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

	//Boolean Methods

	public bool 	IsBlockInWorld()	//for worldPos
	{
		if (this.x < 0 || WorldData.WorldBlockSize <= this.x)
			return (false);
		if (this.y < 0 || WorldData.WorldBlockHeight <= this.y)
			return (false);
		if (this.z < 0 || WorldData.WorldBlockSize <= this.z)
			return (false);
		return (true);
	}

	public bool 	IsChunkInWorld()	//for chunkPos
	{
		if (this.x < 0 || WorldData.WorldChunkSize <= this.x)
			return (false);
		if (this.y < 0 || WorldData.WorldChunkHeight <= this.y)
			return (false);
		if (this.z < 0 || WorldData.WorldChunkSize <= this.z)
			return (false);
		return (true);
	}

	public bool 	IsBlockInChunk()	//for blockPos
	{
		if (this.x < 0 || WorldData.ChunkSize <= this.x)
			return (false);
		if (this.y < 0 || WorldData.ChunkSize <= this.y)
			return (false);
		if (this.z < 0 || WorldData.ChunkSize <= this.z)
			return (false);
		return (true);
	}

	public bool		IsInVolume(Coords pos1, Coords pos2)
	{
		int	minX;
		int	maxX;
		int	minY;
		int	maxY;
		int	minZ;
		int	maxZ;

		if (pos1.y <= pos2.y)
			{minY = pos1.y;	maxY = pos2.y;}
		else
			{minY = pos2.y;	maxY = pos1.y;}

		if (pos1.x <= pos2.x)
			{minX = pos1.x;	maxX = pos2.x;}
		else
			{minX = pos2.x;	maxX = pos1.x;}

		if (pos1.z <= pos2.z)
			{minZ = pos1.z;	maxZ = pos2.z;}
		else
			{minZ = pos2.z;	maxZ = pos1.z;}

		if (minY <= this.y && this.y <= maxY)
			if (minX <= this.x && this.x <= maxX)
				if (minZ <= this.z && this.z <= maxZ)
					return (true);
		return (false);
	}

	public bool		IsEqual(Coords other)
	{
		//if (this == null && pos == null )
			//return (true);

		if (this == null  || other == null )
			return (false);

		else if (this.x == other.x && this.y == other.y && this.z == other.z)
			return (true);

		return (false);
	}

	//Optention Methods

	public Coords	GetNeighbor(int faceIndex)
	{
		Coords pos = new Coords(this.x, this.y, this.z);

		if (faceIndex == 0)
			pos.y += 1;			//top
		else if (faceIndex == 1)
			pos.y -= 1;			//bottom
		else if (faceIndex == 2)
			pos.z -= 1;			//front
		else if (faceIndex == 3)
			pos.z += 1;			//back
		else if (faceIndex == 4)
			pos.x -= 1;			//left
		else if (faceIndex == 5)
			pos.x += 1;			//right
		else
		{
			Debug.Log("Error in Coords.GetNeighbor() : invalid face index given");
			return (null);
		}
		return (pos);
	}

	public Coords[]	ListCoordsInVolume(Coords that)
	{
		int	minX;
		int	maxX;
		int	minY;
		int	maxY;
		int	minZ;
		int	maxZ;

		if (this.y <= that.y)
			{minY = this.y;	maxY = that.y;}
		else
			{minY = that.y;	maxY = this.y;}

		if (this.x <= that.x)
			{minX = this.x;	maxX = that.x;}
		else
			{minX = that.x;	maxX = this.x;}

		if (this.z <= that.z)
			{minZ = this.z;	maxZ = that.z;}
		else
			{minZ = that.z;	maxZ = this.z;}

		int			size = (1 + maxX - minX) * (1 + maxY - minY) * (1 + maxZ - minZ);
		Coords[]	array = new Coords[size];

		int	i = 0;
		for (int y = minY; y <= maxY; y++)
			for (int x = minX; x <= maxX; x++)
				for (int z = minZ; z <= maxZ; z++)
					array[i++] = new Coords(x, y, z);

		return (array);
	}

	//Convertion Methods

	public Coords	WorldToChunkPos()
	{
		return (this.DivPos(WorldData.ChunkSize));
	}

	public Coords	ChunkToWorldPos()
	{
		return (this.MulPos(WorldData.ChunkSize));
	}

	public Coords	WorldToBlockPos()
	{
		return (this.SubPos(this.WorldToChunkPos().MulPos(WorldData.ChunkSize)));
	}

	public Coords	BlockToWorldPos(Coords chunkPos)
	{
		return (this.AddPos(chunkPos.ChunkToWorldPos()));
	}

	public Coords	Copy()
	{
		return (new Coords(this));
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

