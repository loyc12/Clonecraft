using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "CloneCraft/Biome")]
public class BiomeAttributes : ScriptableObject
{
	public string	biomeName;

	public int		baseElevation;		//elevation above world's rockLevel			(only in 2D Gen)
	public int		maxElevation;		//elevation above biome's baseElevation		(only in 2D Gen)
	public float	terrainScale;		//horizontal scale of the terrain
	public float	mountainScale;		//vertical scale of the terrain details
	public int		recursivityAmount;	//how many recursive call of the noise functions should be done
	public float	recursivityFactor;	//how much smaller should each successive recursive call be from the previous one
	public float	heightExponent;		//how common are lowlands (for 2D terrain)

	[Header("Tree Gen")]

	[Range(0f, 1f)]
	public float	forestThreshold;
	public float	forestSpreadScale;

	[Range(0f, 1f)]
	public float	treeThreshold;
	public float	treeSpreadScale;

	public int		minTreeSpread;	//odd = straigt grid, even = diagonal grid
	public int		maxTreeHeight;
	public int		minTreeHiehgt;

	public Vein[]	veins;			//all ores and caves
}

[System.Serializable]
public class	Vein				//for ore and cave gen
{
	public bool			isUsed;
	public string		nodeName;
	public BlockID		blockID;
	public int			height;
	public int			spread;
	public bool			invertThreshold;
	[Range(0f, 1f)]
	public float		topThreshold;
	[Range(0f, 1f)]
	public float		bottomThreshold;
	public float		horizontalScale;
	public float		verticalScale;
	public float		noiseFactor;
	public int			n;
}