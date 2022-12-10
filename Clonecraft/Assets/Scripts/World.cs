using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class	World : MonoBehaviour
{
	public int				seed;
	public Coords			randomOffset;

	public BiomeAttributes	biome;

	public Transform		player;
	public Coords 			spawnPoint;

	public Material			material;
	public BlockType[]		blocktypes;

	Chunk[,,]				chunkMap = new Chunk[WorldData.WorldChunkSize, WorldData.WorldChunkHeight, WorldData.WorldChunkSize];	//Chunks

	public Coords			playerChunk;
	Coords					playerLastChunk;

	List<Coords>			loadedChunks = new List<Coords>();
	List<Coords>			queuedChunks = new List<Coords>();		//chunksToCreate
	private bool			isLoadingChunks;

	public GameObject		debugScreen;

	private void	Start()
	{
		InitializeRandomness();

		SpawnPlayer();
		if (WorldData.PreGenSpawn)
			GenerateSpawn();
	}

	private void	Update()
	{
		playerChunk = FindChunkPos(player.position);

		if (!playerChunk.SamePosAs(playerLastChunk))
			ApplyRenderDistance();

		if (queuedChunks.Count > 0 && !isLoadingChunks)
			StartCoroutine("LoadChunks");

		if (Input.GetButtonDown("F3"))
			debugScreen.SetActive(!debugScreen.activeSelf);
	}

	private void	InitializeRandomness()
	{
		Random.InitState(seed);
		randomOffset = new Coords(
			Random.Range(-WorldData.RandomRange, WorldData.RandomRange),
			Random.Range(-WorldData.RandomRange, WorldData.RandomRange),
			Random.Range(-WorldData.RandomRange, WorldData.RandomRange));
	}

	//generate the chunks inside the initial render distance (square) (at spawn)
	void	GenerateSpawn()
	{
		int	center = Mathf.FloorToInt(WorldData.WorldChunkSize / 2f);

		for (int y = 0; y < WorldData.WorldChunkHeight; y++)
		{
			for (int x = center - WorldData.RenderDistance; x < center + WorldData.RenderDistance; x++)
			{
				for (int z = center - WorldData.RenderDistance; z < center + WorldData.RenderDistance; z++)
				{
					Coords	chunkPos = new Coords(x, y, z);

					if (chunkPos.IsChunkInWorld())
					{
						if (chunkPos.IsChunkInWorld())
						{
							chunkMap[x, y, z] = new Chunk(chunkPos, this, true);
							chunkMap[x, y, z].Load();
							loadedChunks.Add(chunkPos);
						}
					}
				}
			}
		}
	}

	//informs LoadChunks about which chunks to create, activate or deactivate based on rander distance
	void	ApplyRenderDistance()
	{
		playerLastChunk = playerChunk;

		foreach (Coords chunkPos in loadedChunks)
		{
			if (chunkPos.IsChunkInWorld() && !IsChunkInRenderDistance(chunkPos))
				FindChunk(chunkPos).Unload();
		}

		for (int y = 0; y < WorldData.WorldChunkHeight; y++)
		{
			for (int x = playerChunk.x - WorldData.RenderDistance; x <= playerChunk.x + WorldData.RenderDistance; x++)
			{
				for (int z = playerChunk.z - WorldData.RenderDistance; z <= playerChunk.z + WorldData.RenderDistance; z++)
				{
					Coords	chunkPos = new Coords(x, y, z);

					if (chunkPos.IsChunkInWorld() && IsChunkInRenderDistance(chunkPos))
					{
						//if chunk doesn't exist, create it
						if (chunkMap[x, y, z] == null)
						{
							chunkMap[x, y, z] = new Chunk(chunkPos, this, false);
							queuedChunks.Add(chunkPos);
						}
						//if chunk isn't generated, generate it
						else if (!chunkMap[x, y, z].isGenerated)
						{
							queuedChunks.Add(chunkPos);
						}
						//if chunk isn't loaded, load it
						else if (!chunkMap[x, y, z].isLoaded)
						{
							chunkMap[x, y, z].Load();
							loadedChunks.Add(chunkPos);
						}
					}
				}
			}
		}
	}

	//puts the player at the spawnpoint
	void	SpawnPlayer()
	{
		spawnPoint = new Coords(Mathf.FloorToInt(WorldData.WorldBlockSize / 2f), WorldData.WorldBlockHeight, Mathf.FloorToInt(WorldData.WorldBlockSize / 2f));
		//spawnPoint.y = (1 + GetTerrainHeight(spawnPoint));

		player.position = new Vector3(spawnPoint.x + 0.5f, spawnPoint.y + 0.1f, spawnPoint.z + 0.5f);

		playerLastChunk = FindChunkPos(player.position);
		playerChunk = FindChunkPos(player.position);
	}

	IEnumerator LoadChunks()	//CreateChunk
	{
		isLoadingChunks = true;

		while (0 < queuedChunks.Count)
		{
			Chunk	targetChunk = FindChunk(queuedChunks[0]);

			queuedChunks.RemoveAt(0);

			if (targetChunk.isGenerated)
				LoadOrUnload(targetChunk);
			else
			{
				if (!IsChunkInTooFar(targetChunk.chunkPos, WorldData.ChunkTooFarFactor))
				{
					targetChunk.Generate();

					LoadOrUnload(targetChunk);

					yield return (null);
				}
			}
		}

		isLoadingChunks = false;
	}

	void	LoadOrUnload(Chunk targetChunk)
	{
		Coords	chunkPos = targetChunk.chunkPos;

		if (IsChunkInRenderDistance(chunkPos))
		{
			targetChunk.Load();
			loadedChunks.Add(chunkPos);
		}
		else
		{
			targetChunk.Unload();
			loadedChunks.Remove(chunkPos);
		}
	}

	//finds chunk Coords from world Coords
	Coords FindChunkPos (Vector3 vPos)	//GetChunkCoordFromVoxel3D
	{
		int x = Mathf.FloorToInt(vPos.x / WorldData.ChunkSize);
		int y = Mathf.FloorToInt(vPos.y / WorldData.ChunkSize);
		int z = Mathf.FloorToInt(vPos.z / WorldData.ChunkSize);

		return (new Coords (x, y, z));
	}

	//returns true if the given pos is inside the player's render distance (sphere)
	bool	IsChunkInRenderDistance(Coords chunkPos)
	{
		if (!chunkPos.IsChunkInWorld() || IsChunkInTooFar(chunkPos, 1f))
			return (false);

		if (WorldData.RenderDistance < playerChunk.SphereDistance(chunkPos))
			return (false);

		return (true);
	}

	//returns true if the given pos further than factor * render distance (cube)
	bool	IsChunkInTooFar(Coords chunkPos, float factor)
	{
		if (!chunkPos.IsChunkInWorld() || factor * WorldData.RenderDistance < playerChunk.CubeDistance(chunkPos))
			return (true);

		return (false);
	}

	private int	GetTerrainHeight(Coords worldPos)
	{
		float	height;

		if (WorldData.UseSimpleGen)
			height = Noise.Get2DNoise(worldPos.ToVector2(), randomOffset, biome.terrainScale);
		else
			height = Noise.Get2DRecursiveNoise(worldPos.ToVector2(), randomOffset, biome.terrainScale, 2f, 3);

		height *= biome.maxElevation;
		height += biome.baseElevation;

		return (Mathf.FloorToInt(height));
	}

	//returns true if there should be a block at the given worldPos
	private BlockID	Get3DTerrain(Coords worldPos)
	{
		// x = -( a(y - c)**3 + b(y - c) - d )
		// a above 0
		// b and d between 0 and 1
		// c between -1 and 1

		float	slope = 2.5f;				//a		(3.20)	(2.00)
		float	strenghtOffset = 0.3f;		//b		(0.10)	(0.25)
		float	thresholdOffset = 0.60f;	//c		(0.55)	(0.64)
		float	verticalOffset = 0.36f;		//d		(0.45)	(0.32)

		float	heightValue = ((float)worldPos.y / WorldData.WorldBlockHeight) - thresholdOffset;
		float	heightValueCubed =  Mathf.Pow((heightValue), 3);

		float	threshold = -((slope * heightValueCubed) + (strenghtOffset * heightValue) - verticalOffset);

		if (1f < threshold )
			return (BlockID.STONE);
		else if (threshold < 0f)
			return (BlockID.DIRT);

		float	noiseValue;

		if (WorldData.UseSimpleGen)
			noiseValue = Noise.Get3DNoise(worldPos.ToVector3(), randomOffset, biome.terrainScale, biome.mountainScale);
		else
			noiseValue = Noise.Get3DRecursiveNoise(worldPos.ToVector3(), randomOffset, biome.terrainScale, biome.mountainScale, 1.5f, 4);

		float soilThreshold = threshold - ((2.5f - ((float)worldPos.y / WorldData.WorldBlockHeight)) / WorldData.WorldBlockHeight);

		if (noiseValue < soilThreshold)
			return (BlockID.STONE);

		if (noiseValue < threshold)
			return (BlockID.DIRT);
		else
			return (BlockID.AIR);
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
			return (BlockID.BEDROCK);

		/* === 3D NOISE PASS === */

		blockID = Get3DTerrain(worldPos);

		/* === ORE PASS === */
		if (WorldData.UseCaveGen && blockID != BlockID.AIR)
		{
			foreach (Vein vein in biome.veins)
			{
				if (y >= vein.height - vein.spread && y <= vein.height + vein.spread)
					if (blockID != BlockID.AIR && Noise.Get3DVeinNoise(worldPos.ToVector3(), randomOffset, vein))
						blockID = vein.blockID;
			}
		}

		/* === BASIC TERRAIN PASS === */
		if (blockID == BlockID.STONE)
		{
			if (y < WorldData.SeaLevel)
				blockID = BlockID.ROCK;
			else if (y > WorldData.SnowLevel)
				blockID = BlockID.MARBLE;
			else
				blockID = BlockID.STONE;
		}
		else if (blockID == BlockID.DIRT)
		{
			if (y < WorldData.SeaLevel - 2)
				blockID = BlockID.GRAVEL;
			else if  (y < WorldData.SeaLevel + 2)
				blockID = BlockID.SAND;
			else if (y > WorldData.SnowLevel)
				blockID = BlockID.SNOW;
		}
		return (blockID);
	}

	private BlockID GetBlockID2D(Coords worldPos)
	{
		int	y = worldPos.y;
		BlockID blockID = BlockID.AIR;

		/* === ABSOLUTE PASS === */
		if (!worldPos.IsBlockInWorld())
			return (BlockID.AIR);

		else if (y == 0)
			return (BlockID.BEDROCK);


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
		if (WorldData.UseCaveGen && (blockID == BlockID.STONE || blockID == BlockID.DIRT || blockID == BlockID.GRASS))
		{
			foreach (Vein vein in biome.veins)
			{
				if (y >= vein.height - vein.spread && y <= vein.height + vein.spread)	//TEMP
					if (blockID != BlockID.AIR && Noise.Get3DVeinNoise(worldPos.ToVector3(), randomOffset, vein))
						blockID = (BlockID)vein.blockID;
			}
		}

		/* === FINAL PASS === */
		if (y < WorldData.RockLevel && blockID == BlockID.STONE)
			blockID = BlockID.ROCK;

		return (blockID);
	}

	//returns the block ID of the specified location
	public BlockID	FindBlockID(Coords worldPos)				//CheckForVoxel
	{
		Coords	chunkPos = new Coords(worldPos.DivPos(WorldData.ChunkSize));

		if (!worldPos.IsBlockInWorld())
			return (BlockID.AIR);

		Chunk	targetChunk = FindChunk(chunkPos);

		if (targetChunk != null)
		{
			if (targetChunk.isPopulated && targetChunk.isEmpty)
				return (BlockID.AIR);

			Coords blockPos = worldPos.SubPos(chunkPos.MulPos(WorldData.ChunkSize));

			if (targetChunk.isPopulated)
				return (chunkMap[chunkPos.x, chunkPos.y, chunkPos.z].blockMap[blockPos.x, blockPos.y, blockPos.z]);
		}

		return (GetBlockID(worldPos));
	}

	public Chunk	FindChunk(Coords chunkPos)
	{
		return (chunkMap[chunkPos.x, chunkPos.y, chunkPos.z]);
	}

	public bool	IsBlockSolid(Coords blockPos)
	{
		return (blocktypes[(int)FindBlockID(blockPos)].isSolid);
	}

	public bool IsBlockOpaque(Coords blockPos)
	{
		return (blocktypes[(int)FindBlockID(blockPos)].isOpaque);
	}
}
