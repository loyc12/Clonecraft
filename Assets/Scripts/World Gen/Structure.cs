using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure		//Prefab instead?
{

	public static void CreateOakTree(Coords worldPos, World world)
	{
		BiomeAttributes	biome = world.biome;

		float	height = Noise.Get2DNoise(world, worldPos.ToVector2(), world.randomOffset, 0.01f);		//hardcoded smallest scale value?
		height *= (float)(biome.maxTreeHeight - biome.minTreeHeight);
		height += (float)biome.minTreeHeight;

		for (int i = 1; i <= (int)height; i++) //from 1 cause stump is already placed
		{
			world.AddBlockToQueue(worldPos.Copy(), BlockID.OAKLOG);
			worldPos.y += 1;
		}

		//add the leaves around this new worldPos (tip of tree)
	}

	public static void CreateStoneCone(Coords worldPos, World world)
	{
		BiomeAttributes	biome = world.biome;

		float	height = Noise.Get2DNoise(world, worldPos.ToVector2(), world.randomOffset, 0.01f);		//hardcoded smallest scale value?
		height *= (float)(biome.maxTreeHeight - biome.minTreeHeight);									//change me to cone stuff
		height += (float)biome.minTreeHeight;

		int		radius = 5;

		for (int dy = 0; dy <= (int)height; dy++)
		{
			Coords	centerPos = new Coords(0, dy, 0);

			for (int dx = -radius; dx <= radius; dx++)
			{
				for (int dz = -radius; dz <= radius; dz++)
				{
					Coords	deltaPos = new Coords(dx, dy, dz);
					float		distance = centerPos.CircleDistance(deltaPos);

					if (distance <= (float)radius)
						world.AddBlockToQueue(worldPos.Copy(), BlockID.STONE);
				}
			}
			worldPos.y += 1;
		}

		//add the leaves around this new worldPos (tip of tree)
	}

}
