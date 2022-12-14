using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class	Terrain
{
	World			world;
	BiomeAttributes	biome;

	float			soilDepth = 2f;
	float			soilDepthFactor = 8f;

	public	Terrain (World _world, BiomeAttributes _biome)
	{
		world = _world;
		biome = _biome;
	}

	public BlockID GetBlockID(Coords worldPos)		//GetVoxel
	{
		if (WorldData.Use3DGen)
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

		/* === SEA PASS === */
		if (blockID == BlockID.AIR)
			if (y < WorldData.SeaLevel)
				blockID = BlockID.WATER;

		/* === ORE PASS === */
		blockID = SpawnVein(worldPos, blockID);

		/* === SEA PASS === */
		if (blockID == BlockID.AIR)
			if (y < WorldData.MagmaLevel)
				blockID = BlockID.LAVA;

		/* === BASIC TERRAIN PASS === */
		if (blockID == BlockID.STONE)
		{
			if (y < WorldData.SlateLevel)
				blockID = BlockID.SLATE;
			else if (y < WorldData.RockLevel)
				blockID = BlockID.ROCK;
			else if (y > WorldData.SnowLevel)
				blockID = BlockID.MARBLE;
			else
				blockID = BlockID.STONE;
		}
		else if (blockID == BlockID.DIRT)
		{
			if (y < WorldData.SeaLevel - 3)
				blockID = BlockID.GRAVEL;
			else if  (y < WorldData.SeaLevel + 3)
				blockID = BlockID.SAND;
			else if (y > WorldData.SnowLevel)
				blockID = BlockID.SNOW;
			//else
				//blockID = GetSoilBlockID(worldPos);

		}
		return (blockID);
	}

	//returns true if there should be a block at the given worldPos
	private BlockID	Get3DTerrain(Coords worldPos)
	{
		// threshold = 0.5 * ( a(y - c)**3 + b(y - c) - d )
		// a above 0
		// b between 0 and 2
		// c between 0 and 2
		// d between 0 and 2

		float	slope = 16f;				//a		(3.20)	(2.00)	(2.50)	(0.6)	(8.0)
		float	strenghtOffset = 0.32f;		//b		(0.10)	(0.25)	(0.30)	(0.6)	(1.4)
		float	thresholdOffset = 0.62f;	//c		(0.55)	(0.64)	(0.60)	(0.6)	(0.75)
		float	verticalOffset = 1.02f;		//d		(0.45)	(0.32)	(0.36)	(0.3)	(1.4)

		float	heightValue = ((float)worldPos.y / WorldData.WorldBlockHeight) - thresholdOffset;
		float	heightValueCubed =  Mathf.Pow((heightValue), 3);

		float	noiseValue;

		if (WorldData.UseSimpleGen)
			noiseValue = Noise.Get3DNoise(worldPos.ToVector3(), world.randomOffset, biome.terrainScale, biome.mountainScale);
		else
			noiseValue = Noise.Get3DRecursiveNoise(worldPos.ToVector3(), world.randomOffset, biome.terrainScale, biome.mountainScale, 1.618f, 4);

		float	threshold = 0.5f * ((slope * heightValueCubed) + (strenghtOffset * heightValue) + verticalOffset);
		float	soilDepthOffset = 2f * Mathf.Abs(threshold - 0.5f);
		float	soilThreshold = threshold + ((soilDepth + (soilDepthFactor * soilDepthOffset)) / WorldData.WorldBlockHeight);
		//float	soilThreshold = threshold + ((soilDepth - ((float)worldPos.y / WorldData.WorldBlockHeight)) / WorldData.WorldBlockHeight);

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
			return (BlockID.GRASS);
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
			return (BlockID.AIR);

		else if (y == 0)
			return (BlockID.SLATE);


		/* === BASIC TERRAIN PASS === */
		int	height = GetTerrainHeight(worldPos);

		if (y > height)
			return (blockID);
		else if (y > WorldData.SnowLevel)
			blockID = BlockID.STONE;
		else if (y > height - 3 && height < WorldData.SeaLevel - 2)
			blockID = BlockID.GRAVEL;
		else if (y > height - 3 && height < WorldData.SeaLevel + 2)
			blockID = BlockID.SAND;
		else if (y == height)
			blockID = BlockID.GRASS;
		else if (y > height - 3)
			blockID = BlockID.DIRT;
		else if (height < WorldData.SeaLevel + 2 && y > height - 6)
			blockID = BlockID.MARBLE;
		else
			blockID = BlockID.STONE;

		/* === ORE PASS === */
		blockID = SpawnVein(worldPos, blockID);

		/* === FINAL PASS === */
		if (y < WorldData.SlateLevel && blockID == BlockID.STONE)
			blockID = BlockID.SLATE;
		else if (y < WorldData.RockLevel && blockID == BlockID.STONE)
			blockID = BlockID.ROCK;

		return (blockID);
	}

	private int	GetTerrainHeight(Coords worldPos)
	{
		float	height;

		if (WorldData.UseSimpleGen)
			height = Noise.Get2DNoise(worldPos.ToVector2(), world.randomOffset, biome.terrainScale);
		else
			height = Noise.Get2DRecursiveNoise(worldPos.ToVector2(), world.randomOffset, biome.terrainScale, 2f, 4);

		height *= biome.maxElevation;
		height += biome.baseElevation;

		return (Mathf.FloorToInt(height));
	}

	private BlockID	SpawnVein(Coords worldPos, BlockID blockID)
	{
		float	y = worldPos.y;

		if (WorldData.UseCaveGen && blockID != BlockID.AIR)
		{
			foreach (Vein vein in biome.veins)
			{
				if (vein.isUsed && y >= vein.height - vein.spread && y <= vein.height + vein.spread)
					if (blockID != BlockID.AIR && Noise.Get3DVeinNoise(worldPos.ToVector3(), world.randomOffset, vein))
						blockID = vein.blockID;
			}
		}
		return (blockID);
	}
}
