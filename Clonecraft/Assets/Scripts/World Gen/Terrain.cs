using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//handles the terrain gen (interface between world/chunk and noise)
public class	Terrain
{
	World			world;
	BiomeAttributes	biome;

	float			soilDepth = 6f;
	//float			soilDepthFactor = 6f;
	float[]			heightThresholdMap = new float[WorldData.WorldBlockHeight];

	public	Terrain (World _world, BiomeAttributes _biome)
	{
		world = _world;
		biome = _biome;

		InitializeThreshold();
	}

	private void	InitializeThreshold()
	{
		for (int y = 0; y < WorldData.WorldBlockHeight; y++)
		{
			heightThresholdMap[y] = GetHeightThreshold(y);
		}
	}

	private float	GetHeightThreshold(int y)
	{

		// threshold = a(c - y)**3 - b(c - y) + d
		// a above 0
		// b c d between 0 and 2

		float	a = -7.0f;	//a		(-0.4)	(-5.0)
		float	b = 0.64f;	//b		(0.64)	(0.68)
		float	c = 0.66f;	//c		(0.64)	(0.68)
		float	d = 0.68f;	//d		(0.64)	(0.68)

		float	h = c - ((float)y / WorldData.WorldBlockHeight);
		float	hc =  Mathf.Pow((h), 3);

		return (a * hc) - (b * h) + d;
/*
		// threshold = (a(y - c)**3 + d)**3 + b(y - c) + e
		// a above 0
		// b c d e between 0 and 2

		float	a = 12.0f;	//a
		float	b = 0.62f;	//b
		float	c = 0.62f;	//b
		float	d = 0.00f;	//d
		float	e = 0.54f;	//e

		float	heightFactor = ((float)y / WorldData.WorldBlockHeight) - c;
		float	heightFactorCubed =  Mathf.Pow((heightFactor), 3f);

		return	(Mathf.Pow(((a * heightFactorCubed) + d), 3f) + (b * heightFactor) + e);
		*/
	}

	public BlockID GetBlockID(Coords worldPos)		//GetVoxel
	{
		if (world.Flatland)
			return (GetBlockIDFlat(worldPos));
		else if (world.Use3DGen)
			return (GetBlockID3D(worldPos));
		else
			return (GetBlockID2D(worldPos));
	}

	private BlockID GetBlockID3D(Coords worldPos)
	{
		BlockID blockID = BlockID.AIR;
		float	y = worldPos.y;

		/* === ABSOLUTE PASS === */
		if (!worldPos.IsBlockInWorld())
			return (blockID);

		else if (y == 0)
			return (BlockID.SLATE);

		/* === 3D NOISE PASS === */
		blockID = Get3DTerrain(worldPos);

		/* === WATER PASS === */
		if (blockID == BlockID.AIR)
			if (y < WorldData.SeaLevel)
				blockID = BlockID.WATER;

		/* === ORE PASS === */
		if (world.UseCaveGen && blockID != BlockID.AIR)
			blockID = SpawnVeins(worldPos, blockID);

		/* === LAVA PASS === */
		if (blockID == BlockID.AIR)
			if (y < WorldData.MagmaLevel)
				blockID = BlockID.LAVA;

		/* === BASIC TERRAIN PASS === */
		if (blockID == BlockID.STONE)
		{
			if (y < WorldData.GraniteLevel)
				blockID = BlockID.GRANITE;
			else if (y < WorldData.RockLevel)
				blockID = BlockID.ROCK;
			else if (y > WorldData.SnowLevel)
				blockID = BlockID.MARBLE;
			else
				blockID = BlockID.STONE;
		}
		else if (blockID == BlockID.DIRT)
		{
			if (y < WorldData.SeaLevel - WorldData.BeachHeight)
				blockID = BlockID.GRAVEL;
			else if  (y < WorldData.SeaLevel + WorldData.BeachHeight)
				blockID = BlockID.SAND;
			else if (world.ProcessSoil)
				blockID = GetSoilBlockID(worldPos);
		}

		/* === TREE PASS === */
		if (world.UseTreeGen && blockID == BlockID.AIR)
			blockID = SpawnTrees(worldPos, blockID);

		return (blockID);
	}

	//returns true if there should be a block at the given worldPos
	private BlockID	Get3DTerrain(Coords worldPos)
	{

		float	noiseValue;

		if (world.UseSimpleGen)
			noiseValue = Noise.Get3DNoise(world, worldPos.ToVector3(), world.randomOffset, biome.terrainScale, biome.mountainScale);
		else
			noiseValue = Noise.Get3DRecursiveNoise(world, worldPos.ToVector3(), world.randomOffset, biome.terrainScale, biome.mountainScale,  biome.recursivityFactor, biome.recursivityAmount);

		float	threshold = heightThresholdMap[worldPos.y];
		//float	soilDepthOffset = 2f * Mathf.Abs(threshold - 0.5f);
		float	soilThreshold = threshold + ((soilDepth /*+ (soilDepthFactor * soilDepthOffset)*/) / WorldData.WorldBlockHeight);

		//noise needs to have a value ABOVE the threshold
		if (soilThreshold < noiseValue)
			return (BlockID.STONE);
		if (threshold < noiseValue)
			return (BlockID.DIRT);
		else
			return (BlockID.AIR);
	}

	private BlockID	GetSoilBlockID(Coords worldPos)		//find a better way to do this
	{
		if (world.FindBlockID(worldPos.AddPos(new Coords(0, 1, 0))) == BlockID.AIR)
		{
			if (worldPos.y > WorldData.SnowLevel)
				return (BlockID.SNOW);
			else
				return (BlockID.GRASS);
		}
		//else if (world.FindBlockID(worldPos.AddPos(new Coords(0, 1, 0))) == BlockID.GRASS)
			//return (BlockID.DIRT);
		else
			return (BlockID.STONE);
	}

	private BlockID GetBlockID2D(Coords worldPos)
	{
		int	y = worldPos.y;
		BlockID blockID = BlockID.AIR;

		/* === ABSOLUTE PASS === */
		if (!worldPos.IsBlockInWorld())
			return (blockID);

		else if (y == 0)
			return (BlockID.SLATE);

		/* === 2D NOISE PASS === */
		int	height = GetTerrainHeight(worldPos);

		if (y < height - (soilDepth / 2))
			blockID = BlockID.STONE;
		else if (y <= height)
			blockID = BlockID.DIRT;

		/* === WATER PASS === */
		if (blockID == BlockID.AIR)
			if (y < WorldData.SeaLevel)
				blockID = BlockID.WATER;

		/* === ORE PASS === */
		if (world.UseCaveGen && y <= height)
			blockID = SpawnVeins(worldPos, blockID);

		/* === LAVA PASS === */
		if (blockID == BlockID.AIR)
			if (y < WorldData.MagmaLevel)
				blockID = BlockID.LAVA;

		/* === BASIC TERRAIN PASS === */
		if (blockID == BlockID.STONE)
		{
			if (y < WorldData.GraniteLevel)
				blockID = BlockID.GRANITE;
			else if (y < WorldData.RockLevel)
				blockID = BlockID.ROCK;
			else if (y > WorldData.SnowLevel)
				blockID = BlockID.MARBLE;
			else
				blockID = BlockID.STONE;
		}
		else if (blockID == BlockID.DIRT && world.ProcessSoil)
		{
			if (y < WorldData.SeaLevel - WorldData.BeachHeight)
				blockID = BlockID.GRAVEL;
			else if  (y < WorldData.SeaLevel + WorldData.BeachHeight)
				blockID = BlockID.SAND;
			else if (y > WorldData.SnowLevel)
				blockID = BlockID.SNOW;
			else if (y == height)
				blockID = BlockID.GRASS;
		}

		/* === TREE PASS === */
		if (world.UseTreeGen && y == height + 1)
			blockID = SpawnTrees(worldPos, blockID);

		return (blockID);
	}

	public int	GetTerrainHeight(Coords worldPos)
	{
		float	height;

		if (world.UseSimpleGen)
			height = Noise.Get2DNoise(world, worldPos.ToVector2(), world.randomOffset, biome.terrainScale);
		else
			height = Noise.Get2DRecursiveNoise(world, worldPos.ToVector2(), world.randomOffset, biome.terrainScale, biome.recursivityFactor, biome.recursivityAmount + 1);

		height = Mathf.Pow(height, biome.heightExponent);

		height *= biome.maxElevation;
		height += biome.baseElevation;

		return (Mathf.FloorToInt(height));
	}

	private BlockID	GetBlockIDFlat(Coords worldPos)
	{
		int	y = worldPos.y;
		BlockID blockID = BlockID.AIR;

		/* === ABSOLUTE PASS === */
		if (!worldPos.IsBlockInWorld())
			return (blockID);

		else if (y == 0)
			return (BlockID.SLATE);

		/* === FLAT WORLD PASS === */
		int	height = WorldData.WorldBlockHeight / 2;

		if (y < height - soilDepth)
			blockID = BlockID.STONE;
		else if (y <= height)
			blockID = BlockID.DIRT;

		/* === ORE PASS === */
		if (world.UseCaveGen && y <= height)
			blockID = SpawnVeins(worldPos, blockID);

		/* === LAVA PASS === */
		if (blockID == BlockID.AIR)
			if (y < WorldData.MagmaLevel)
				blockID = BlockID.LAVA;

		/* === BASIC TERRAIN PASS === */
		if (blockID == BlockID.STONE)
		{
			if (y < WorldData.GraniteLevel)
				blockID = BlockID.GRANITE;
			else if (y < WorldData.RockLevel)
				blockID = BlockID.ROCK;
			else if (y > WorldData.SnowLevel)
				blockID = BlockID.MARBLE;
			else
				blockID = BlockID.STONE;
		}
		else if (blockID == BlockID.DIRT && world.ProcessSoil)
		{
			if (y < WorldData.SeaLevel - WorldData.BeachHeight)
				blockID = BlockID.GRAVEL;
			else if  (y < WorldData.SeaLevel + WorldData.BeachHeight)
				blockID = BlockID.SAND;
			else if (y > WorldData.SnowLevel)
				blockID = BlockID.SNOW;
			else if (y == height)
				blockID = BlockID.GRASS;
		}

		/* === TREE PASS === */
		if (world.UseTreeGen && y == height + 1)
			blockID = SpawnTrees(worldPos, blockID);

		return (blockID);
	}

	private BlockID	SpawnVeins(Coords worldPos, BlockID blockID)
	{
		float	y = worldPos.y;

		if (world.blocktypes[(int)blockID].isOpaque)
		{
			foreach (Vein vein in biome.veins)
			{
				if (vein.isUsed && y >= vein.height - vein.spread && y <= vein.height + vein.spread)
					if (blockID != BlockID.AIR && Noise.Get3DVeinNoise(world, worldPos.ToVector3(), world.randomOffset, vein))
						blockID = vein.blockID;
			}
		}
		return (blockID);
	}

	private BlockID	SpawnTrees(Coords worldPos, BlockID blockID)
	{
		if (worldPos.y > WorldData.SeaLevel + WorldData.BeachHeight)
		{
			//BROKEN WITH 3d GEN (too heavy?)
			if (!world.Use3DGen)// || world.FindBlockID(worldPos.GetNeighbor((int)FaceDir.BOTTOM)) == BlockID.GRASS)
			{
				Vector2	XZ = new Vector2(worldPos.x, worldPos.z);

				if (Noise.Get2DNoise(world, XZ, world.randomOffset, biome.forestSpreadScale) > biome.forestThreshold)
				{
					//blockID = BlockID.OAKLEAVES;	//FOR DEBUGGING (SHOW FORESTED ZONES)
					//TODO : find a better way to avoid fused trees
					if ((worldPos.x + worldPos.z) % biome.minTreeSpread == 0 && (worldPos.x - worldPos.z) % biome.minTreeSpread == 0)
						if (Noise.Get2DNoise(world, XZ, world.randomOffset, biome.treeSpreadScale) > biome.treeThreshold)
							blockID = BlockID.OAKLOG;
				}
			}
		}
		return (blockID);
	}
}
