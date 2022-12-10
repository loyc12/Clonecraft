using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "CloneCraft/Biome")]
public class BiomeAttributes : ScriptableObject
{

	public string	biomeName;

	public int		baseElevation;	//elevation above world's rockLevel
	public int		maxElevation;	//elevation above biome's baseElevation
	public float	terrainScale;
	public float	mountainScale;

	public Vein[]	veins;			//all ores
}

[System.Serializable]
public class	Vein				//for ore gen (and cave??)
{
	public string		nodeName;
	public BlockID		blockID;
	public int			height;
	public int			spread;
	public float		horizontalScale;
	public float		verticalScale;
	public float		threshold;
	public int			n;
}