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
			world.AddBlockToQueue(worldPos.Copy(), BlockID.OAKLOG, true);
			worldPos.y += 1;
		}

		worldPos.y -= 1;
		int	r = 6;									//hardcoded
		for (int y = -r; y <= r; y++)				//ugly ass lolipop trees
		{
			for (int x = -r; x <= r; x++)
			{
				for (int z = -r; z <= r; z++)
				{
					Coords	leafPos = worldPos.AddPos(new Coords(x, y, z));
					float	d = leafPos.SphereDistance(worldPos);

					if (d < r + 0.5f)
						world.AddBlockToQueue(leafPos, BlockID.OAKLEAVES, false);
				}
			}
		}
	}

	public static void CreateStoneCone(Coords worldPos, World world, int bottomRadius, int topRadius)
	{
		BiomeAttributes	biome = world.biome;

		float	height = Noise.Get2DNoise(world, worldPos.ToVector2(), world.randomOffset, WorldData.minScale);
		height *= (float)(biome.maxTreeHeight - biome.minTreeHeight);									//change me to cone stuff
		height += (float)biome.minTreeHeight;

		int		radius = Mathf.Max(bottomRadius, topRadius);

		for (int dy = 0; dy <= (int)height; dy++)
		{
			Coords centerPos = new Coords(0, dy, 0);
			float	heightRadius = (bottomRadius * (1 - (dy / height))) + (topRadius * (dy / height));

			for (int dx = -radius; dx <= radius; dx++)
			{
				for (int dz = -radius; dz <= radius; dz++)
				{
					Coords	deltaPos = new Coords(dx, dy, dz);
					float	distance = centerPos.CircleDistance(deltaPos);

					if (distance <= heightRadius)
						world.AddBlockToQueue(worldPos.AddPos(deltaPos).Copy(), BlockID.STONE, false);
				}
			}
		}
	}

	public static void CreateStonePillar(Coords worldPos, World world, int bottomRadius, int centerRadius, int topRadius)
	{
		BiomeAttributes	biome = world.biome;

		float	height = Noise.Get2DNoise(world, worldPos.ToVector2(), world.randomOffset, WorldData.minScale);
		height *= (float)(biome.maxTreeHeight - biome.minTreeHeight);									//change me to cone stuff
		height += (float)biome.minTreeHeight;

		Structure.CreateStoneCone(worldPos, world, bottomRadius, centerRadius);
		worldPos.y += (int)height;
		Structure.CreateStoneCone(worldPos, world, centerRadius, topRadius);
	}

}
