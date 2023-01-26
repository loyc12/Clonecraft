using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FaceDir : byte
{
	TOP		= 0,
	BOTTOM	= 1,
	FRONT	= 2,
	BACK	= 3,
	LEFT 	= 4,
	RIGHT	= 5
}

public static class	VoxelData
{
	public static readonly Vector3[]	voxelVerts = new Vector3[8]
	{
		new Vector3(0f, 0f, 0f),	//0
		new Vector3(1f, 0f, 0f),	//1
		new Vector3(1f, 1f, 0f),	//2
		new Vector3(0f, 1f, 0f),	//3
		new Vector3(0f, 0f, 1f),	//4
		new Vector3(1f, 0f, 1f),	//5
		new Vector3(1f, 1f, 1f),	//6
		new Vector3(0f, 1f, 1f)		//7
	};

	public static readonly int[,]		voxelQuads = new int[6,4]
	{
		{3, 7, 6, 2},	//0 : top face
		{1, 5, 4, 0},	//1 : bottom face
		{0, 3, 2, 1},	//2 : front face
		{5, 6, 7, 4},	//3 : back face
		{4, 7, 3, 0},	//4 : left face
		{1, 2, 6, 5}	//5 : right face
	};

	public static readonly Vector2[]	voxelUvs = new Vector2[4]
	{
		new Vector2(0.0f, 0.0f),	//0 : bottom left
		new Vector2(0.0f, 1.0f),	//1 : top left
		new Vector2(1.0f, 1.0f),	//2 : top right
		new Vector2(1.0f, 0.0f)		//3	: bottom right
	};

	public static readonly float[,]		faceTint = new float[1,4]		//TO IMPLEMENT!!!
	{
		{1f, 0.9f, 0.8f, 0.7f}	//from lightest (top) to darkest (bottom)
	};
}
