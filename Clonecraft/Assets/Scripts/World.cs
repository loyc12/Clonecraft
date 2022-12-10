using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class	World : MonoBehaviour
{
	public int				seed;
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
		Random.InitState(seed);

		SpawnPlayer();
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
		spawnPoint = new Coords(Mathf.FloorToInt(WorldData.WorldBlockSize / 2f), 0, Mathf.FloorToInt(WorldData.WorldBlockSize / 2f));
		spawnPoint.y = (1 + GetTerrainHeight(spawnPoint));

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
				if (!IsChunkInTooFar(targetChunk.chunkPos, 2f))
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

	int	GetTerrainHeight(Coords worldPos)
	{
		float	height;

		if (WorldData.UseSimpleGen)
			height = Noise.Get2DNoise(new Vector2(worldPos.x, worldPos.z), 0, biome.terrainScale);
		else
			height = Noise.Get2DRecursiveNoise(new Vector2(worldPos.x, worldPos.z), 0, biome.terrainScale, 2f, 3);

		height *= biome.maxElevation;
		height += biome.baseElevation;

		return (Mathf.FloorToInt(height));
	}

	float	Get3DTerrain(Coords worldPos)	//EXPERIMENTAL
	{
		float	value;

		//if (WorldData.UseSimpleGen)
			//value = Noise.Get3DNoise(new Vector3(worldPos.x, worldPos.y, worldPos.z), 0, biome.terrainScale);
		//else
			value = Noise.Get3DRecursiveNoise(new Vector3(worldPos.x, worldPos.y, worldPos.z), 0, biome.terrainScale, 2f, 3);

		return (value);
	}

	public BlockID GetBlockID(Coords worldPos)		//GetVoxel
	{
		int		y = worldPos.y;

		/* === ABSOLUTE PASS === */
		if (!worldPos.IsBlockInWorld())
			return (BlockID.AIR);

		else if (y == 0)
			return (BlockID.BEDROCK);

		/* === BASIC TERRAIN PASS === */

		float	center = 0.5f;
		float	slope = 4f;
		float	noiseValue = 1f; //Get3DTerrain(worldPos);
		float	heightFactor = 0.5f; //(y / WorldData.WorldBlockHeight);
		float	threshold = (slope * Mathf.Pow((heightFactor - center), 3) + heightFactor);

		if (threshold < noiseValue)
			return (BlockID.STONE);
		return (BlockID.AIR);
	}

	public BlockID GetBlockID_Old(Coords worldPos)		//GetVoxel
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

		/* === ORE TERRAIN PASS === *//*
		if (blockID == BlockID.STONE || blockID == BlockID.DIRT || blockID == BlockID.GRASS)
		{
			foreach (Vein vein in biome.veins)
			{
				if (y >= vein.height - vein.spread && y <= vein.height + vein.spread)	//TEMP
					if (blockID != BlockID.AIR && Noise.Get3DVeinNoise(worldPos.ToVector3(), vein))
						blockID = (BlockID)vein.blockID;
			}
		}*/

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
