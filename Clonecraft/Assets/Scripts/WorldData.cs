using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class	WorldData
{
	public static readonly int		RandomRange = 1024;

	public static readonly bool		Use3DGen = true;
	public static readonly bool		UseSimpleGen = false;
	public static readonly bool		UseCaveGen = false;
	public static readonly bool		PreGenSpawn = false;

	public static readonly int		RenderDistance = 10;		//in chunks
	public static readonly float	RenderLimit = 12;			//in chunks

	public static readonly int		WorldChunkSize = 64;		//in chunks
	public static int				WorldBlockSize				//in blocks
	{ get { return WorldChunkSize * ChunkSize; }}

	public static readonly int		WorldChunkHeight = 12;		//in chunks
	public static int				WorldBlockHeight			//in blocks
	{ get { return WorldChunkHeight * ChunkSize; }}

	public static readonly int		ChunkSize = 16;				//in blocks
	public static readonly int		TextureAtlasSize = 16;		//in textures
	public static float				NormalizedTextureSize		//over 1
	{ get {return 1f / (float)TextureAtlasSize;}}

	public static int				RockLevel 					//in blocks
	{ get { return Mathf.FloorToInt(WorldBlockHeight * 0.16f);}}
	public static int				SeaLevel 					//in blocks
	{ get { return Mathf.FloorToInt(WorldBlockHeight * 0.36f);}}
	public static int				SnowLevel 					//in blocks
	{ get { return Mathf.FloorToInt(WorldBlockHeight * 0.76f);}}

	public static float				noiseScale
	{ get {return 16f / (float)ChunkSize;}}
}
