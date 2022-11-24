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
}