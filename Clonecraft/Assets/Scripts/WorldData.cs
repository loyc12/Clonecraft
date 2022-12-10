using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class	WorldData
{
	public static readonly int		RandomRange = 1024;

	public static readonly bool		Use3DGen = true;
	public static readonly bool		UseSimpleGen = false;
	public static readonly bool		UseCaveGen = true;
	public static readonly bool		PreGenSpawn = false;

	public static readonly int		RenderDistance = 12;		//in chunk
	public static readonly float	ChunkTooFarFactor = 1.2f;	//of renderDist

	public static readonly int		WorldChunkSize = 16;		//in chunk
	public static int				WorldBlockSize				//in Block
	{ get { return WorldChunkSize * ChunkSize; }}

	public static readonly int		WorldChunkHeight = 12;		//in chunk
	public static int				WorldBlockHeight			//in Block
	{ get { return WorldChunkHeight * ChunkSize; }}

	public static readonly int		ChunkSize = 16;				//in Block
	public static readonly int		TextureAtlasSize = 16;		//in face
	public static float				NormalizedTextureSize		//over 1
	{ get {return 1f / (float)TextureAtlasSize;}}

	public static readonly int		RockLevel = 16;				//in Block : max height where rock is generated
	public static int				SeaLevel 					//in Block
	{ get { return Mathf.FloorToInt(WorldBlockHeight * 0.30f);}}
	public static int				SnowLevel 					//in Block
	{ get { return Mathf.FloorToInt(WorldBlockHeight * 0.80f);}}

	public static float				noiseScale
	{ get {return 16f / (float)ChunkSize;}}
}
