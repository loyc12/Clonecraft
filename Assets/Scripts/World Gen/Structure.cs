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
		int	r = 4;
		for (int y = -r; y <= r; y++)				//ugly ass trees
		{
			for (int x = -r; x <= r; x++)
			{
				for (int z = -r; z <= r; z++)
				{
					Coords leafPos = worldPos.AddPos(new Coords(x, y, z));

					//if (!(y < 0 && x == 0 && z == 0))
					world.AddBlockToQueue(leafPos, BlockID.OAKLEAVES, false);
				}
			}
		}

		//add the leaves around this new worldPos (tip of tree)
	}

}
