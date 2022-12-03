using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//	EP08:16M

/*	IDEAS

	make crouch crouchier
	add chunk collumns

*/

public static class	WorldData
{

	public static readonly int	RenderDistance = 8;		//in chunk

	public static readonly int	WorldChunkSize = 32;	//in chunk
	public static int			WorldVoxelSize			//in voxel
	{
		get { return WorldChunkSize * ChunkSize; }
	}

	public static readonly int	WorldChunkHeight = 12;	//in chunk
	public static int			WorldVoxelHeight		//in voxel
	{
		get { return WorldChunkHeight * ChunkSize; }
	}

	public static readonly int	ChunkSize = 16;			//in voxel
	public static readonly int	TextureAtlasSize = 16;	//in face
	public static float			NormalizedTextureSize	//over 1
	{
		get {return 1f / (float)TextureAtlasSize;}
	}

	public static readonly int	RockLevel = 16;			//in voxel : max height where rock is generated
	public static int	SeaLevel 						//in voxel
	{
		get { return Mathf.FloorToInt(WorldVoxelHeight * 0.5f);}
	}
	public static int	SnowLevel 						//in voxel
	{
		get { return Mathf.FloorToInt(WorldVoxelHeight * 0.80f);}
	}

	public static float	noiseScale
	{
		get {return 16f / (float)ChunkSize;}
	}
}
