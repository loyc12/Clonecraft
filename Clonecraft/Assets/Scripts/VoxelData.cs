using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class	VoxelData
{

	public static readonly int	ChunkSize = 16;
	public static readonly int	TextureAtlasSize = 16;
	//public static readonly int	textureResolution = 16;
	public static float			NormalizedTextureSize
	{
		get {return 1f / (float) TextureAtlasSize;}
	}

	public static readonly Vector3[]	voxelVerts = new Vector3[8]
	{
		new Vector3(-0.5f, -0.5f, -0.5f),	//0
		new Vector3( 0.5f, -0.5f, -0.5f),	//1
		new Vector3( 0.5f,  0.5f, -0.5f),	//2
		new Vector3(-0.5f,  0.5f, -0.5f),	//3
		new Vector3(-0.5f, -0.5f,  0.5f),	//4
		new Vector3( 0.5f, -0.5f,  0.5f),	//5
		new Vector3( 0.5f,  0.5f,  0.5f),	//6
		new Vector3(-0.5f,  0.5f,  0.5f)	//7
	};

	public static readonly Vector3[]	neighbors = new Vector3[6]
	{
		new Vector3(0.0f, 0.0f, -1.0f),	//0 : front neighbor
		new Vector3(0.0f, 0.0f,  1.0f),	//1 : back neighbor
		new Vector3(0.0f,  1.0f, 0.0f),	//2 : top neighbor
		new Vector3(0.0f, -1.0f, 0.0f),	//3 : bottom neighbor
		new Vector3(-1.0f, 0.0f, 0.0f),	//4 : left neighbor
		new Vector3( 1.0f, 0.0f, 0.0f)	//5 : right neighbor
	};

	public static readonly int[,]	voxelQuads = new int[6,4]	//TODO all faces
	{
		{0, 3, 2, 1},	//0 : front face
		{5, 6, 7, 4},	//1 : back face
		{3, 7, 6, 2},	//2 : top face
		{1, 5, 4, 0},	//3 : bottom face
		{4, 7, 3, 0},	//4 : left face
		{1, 2, 6, 5}	//5 : right face
	};

	public static readonly Vector2[]	voxelUvs = new Vector2[4]	//TODO all faces
	{
		new Vector2(0.0f, 0.0f),	//0 : bottom left
		new Vector2(0.0f, 1.0f),	//1 : top left
		new Vector2(1.0f, 1.0f),	//2 : top right
		new Vector2(1.0f, 0.0f)		//3	: bottom right
	};

}
