using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class	WorldData
{
	public static readonly int		RandomRange = 2048;			//in blocks

	public static readonly int		RenderDistance = 8;			//in chunks
	public static readonly int		RenderLimit = 8;			//in chunks
	public static readonly int		GenerationDistance = 6;		//in chunks

	public static readonly int		WorldChunkSize = 256;		//in chunks
	public static int				WorldBlockSize				//in blocks
	{ get { return WorldChunkSize * ChunkSize; }}

	public static readonly int		WorldChunkHeight = 16;		//in chunks
	public static int				WorldBlockHeight			//in blocks
	{ get { return WorldChunkHeight * ChunkSize; }}

	public static readonly int		ChunkSize = 16;				//in blocks
	public static readonly int		TextureAtlasSize = 21;		//in textures			//make block atlas a POT to fix missing pixels?
	public static float				NormalizedTextureSize		//over 1
	{ get {return 1f / (float)TextureAtlasSize;}}

	public static int				MagmaLevel 					//in blocks
	{ get { return Mathf.FloorToInt(WorldBlockHeight * 0.06f);}}
	public static int				GraniteLevel 					//in blocks
	{ get { return Mathf.FloorToInt(WorldBlockHeight * 0.16f);}}
	public static int				RockLevel 					//in blocks
	{ get { return Mathf.FloorToInt(WorldBlockHeight * 0.32f);}}
	public static int				SeaLevel 					//in blocks
	{ get { return Mathf.FloorToInt(WorldBlockHeight * 0.4f);}}
	public static int				SnowLevel 					//in blocks
	{ get { return Mathf.FloorToInt(WorldBlockHeight * 0.80f);}}

	public static float				noiseScale
	{ get {return 16f / (float)ChunkSize;}}
	public static float				minScale = 0.01f;			//scale for random noise

	public static readonly int		BeachHeight = 3;			//in blocks
}
