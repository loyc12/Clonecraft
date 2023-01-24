using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure		//Prefab instead?
{

	public static void CreateOakTree(Coords worldPos, World world)
	{
		BiomeAttributes	biome = world.biome;

		float	height = Noise.Get2DNoise(world, worldPos.ToVector2(), world.randomOffset, 0.01f);
		height *= (float)(biome.maxTreeHeight - biome.minTreeHeight);
		height += (float)biome.minTreeHeight;

		for (int i = 0; i <= (int)height; i++)
		{
			world.AddBlockToQueue(worldPos.Copy(), BlockID.OAKLOG);
			worldPos.y += 1;
		}
	}

}
