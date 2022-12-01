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

	Chunk[,,]				region = new Chunk[WorldData.WorldChunkSize, WorldData.WorldChunkHeight, WorldData.WorldChunkSize];	//Chunks

	List<Coords>			loadedChunks = new List<Coords>();
	public Coords			playerChunk;
	public Coords			playerLastChunk;

	List<Coords>			queuedChunks = new List<Coords>();		//chunksToCreate
	private bool			isLoadingChunks;

	private void	Start()
	{
		Random.InitState(seed);

		SpawnPlayer();
		GenerateSpawn();
	}

	private void	Update()
	{
		playerChunk = FindChunkPos(player.position);
	
		if (!playerChunk.SamePos(playerLastChunk))
			ApplyRenderDistance();

		if (queuedChunks.Count > 0 && !isLoadingChunks)
			StartCoroutine("LoadChunks");

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

					if (IsChunkInWorld(chunkPos))
					{
						if (IsChunkInRenderDistance(chunkPos))
						{
							region[x, y, z] = new Chunk(chunkPos, this, true);
							region[x, y, z].Load();
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
			if (IsChunkInWorld(chunkPos) && !IsChunkInRenderDistance(chunkPos))
				FindChunk(chunkPos).Unload();
		}

		for (int y = 0; y < WorldData.WorldChunkHeight; y++)
		{
			for (int x = playerChunk.x - WorldData.RenderDistance; x <= playerChunk.x + WorldData.RenderDistance; x++)
			{
				for (int z = playerChunk.z - WorldData.RenderDistance; z <= playerChunk.z + WorldData.RenderDistance; z++)
				{
					Coords	chunkPos = new Coords(x, y, z);

					if (IsChunkInWorld(chunkPos) && IsChunkInRenderDistance(chunkPos))
					{
						//if chunk doesn't exist, create it
						if (region[x, y, z] == null)
						{
							region[x, y, z] = new Chunk(chunkPos, this, false);
							queuedChunks.Add(chunkPos);
						}
						//if chunk isn't generated, generate it
						else if (!region[x, y, z].isGenerated)
						{
							queuedChunks.Add(chunkPos);
						}
						//if chunk isn't loaded, load it
						else if (!region[x, y, z].isLoaded)
						{
							region[x, y, z].Load();
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
		spawnPoint = new Coords(Mathf.FloorToInt(WorldData.WorldVoxelSize / 2f), 0, Mathf.FloorToInt(WorldData.WorldVoxelSize / 2f));
		spawnPoint.y = (1 + GetTerrainHeight(spawnPoint));

		player.position = new Vector3(spawnPoint.x - 0.5f, spawnPoint.y + 0.1f, spawnPoint.z - 0.5f);

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
				if (!IsChunkInTooFar(targetChunk.chunkPos, 3f))
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

	//finds the chunk of a given voxel pos
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
		if (!IsChunkInWorld(chunkPos) || IsChunkInTooFar(chunkPos, 1f))
			return (false);

		if (WorldData.RenderDistance < playerChunk.SphereDistance(chunkPos))
			return (false);

		return (true);
	}

	//returns true if the given pos further than factor * render distance (cube)
	bool	IsChunkInTooFar(Coords chunkPos, float factor)
	{
		if (!IsChunkInWorld(chunkPos) || factor * WorldData.RenderDistance < playerChunk.CubeDistance(chunkPos))
			return (true);

		return (false);
	}

	//returns true if the given chunk pos is inside the worldgen limits
	bool	IsChunkInWorld(Coords chunkPos)
	{
		if (chunkPos.x < 0 || WorldData.WorldChunkSize <= chunkPos.x)
			return (false);
		if (chunkPos.y < 0 || WorldData.WorldChunkHeight <= chunkPos.y)
			return (false);
		if (chunkPos.z < 0 || WorldData.WorldChunkSize <= chunkPos.z)
			return (false);
		return (true);
	}

	//returns true if the given voxel pos is inside the worldgen limits
	bool	IsVoxelInWorld(Coords worldPos)
	{
		if (worldPos.x < 0 || WorldData.WorldVoxelSize <= worldPos.x)
			return (false);
		if (worldPos.y < 0 || WorldData.WorldVoxelHeight <= worldPos.y)
			return (false);
		if (worldPos.z < 0 || WorldData.WorldVoxelSize <= worldPos.z)
			return (false);
		return (true);
	}

	int	GetTerrainHeight(Coords worldPos)
	{
		float	height = Noise.Get2DNoise(new Vector2(worldPos.x, worldPos.z), 0, biome.terrainScale);
		//float	height = Noise.Get2DRecursiveNoise(new Vector2(worldPos.x, worldPos.z), 0, biome.terrainScale, 2f, 3);
		
		height *= biome.maxElevation;
		height += biome.baseElevation;

		return (Mathf.FloorToInt(height));
	}

	public BlockID GetBlockID(Coords worldPos)		//GetVoxel
	{
		int	y = worldPos.y;
		BlockID blockID = BlockID.AIR;

		/* === ABSOLUTE PASS === */
		if (!IsVoxelInWorld(worldPos))
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

	public bool	CheckForOpacity(Coords worldPos)
	{
		Coords	chunkPos = new Coords(worldPos.DivPos(WorldData.WorldChunkSize));

		if (!IsChunkInWorld(worldPos))
			return (false);

		Chunk	targetChunk = FindChunk(chunkPos);

		if(targetChunk == null)
			return (false);

		if (targetChunk.isPopulated && targetChunk.isEmpty)
			return (false);

		if (targetChunk != null && targetChunk.isPopulated)
			return (blocktypes[(int)targetChunk.FindBlockID(worldPos)].isOpaque);

		return (blocktypes[(int)GetBlockID(worldPos)].isOpaque);
	}

	public bool	CheckForSolidity(Coords worldPos)
	{
		Coords	chunkPos = new Coords(worldPos.DivPos(WorldData.WorldChunkSize));

		if (!IsChunkInWorld(worldPos))
			return (false);

		Chunk	targetChunk = FindChunk(chunkPos);

		if (targetChunk.isEmpty)
			return (false);

		if (targetChunk != null && targetChunk.isPopulated)
			return (blocktypes[(int)targetChunk.FindBlockID(worldPos)].isSolid);

		return (blocktypes[(int)GetBlockID(worldPos)].isSolid);
	}

	public Chunk	FindChunk(Coords chunkPos)
	{
		return (region[chunkPos.x, chunkPos.y, chunkPos.z]);
	}

	

}

/*
	public bool	CheckForVoxel(float _x, float _y, float _z)
	{

		Coords mixPos = new Coords(Mathf.FloorToInt(_x), Mathf.FloorToInt(_y), Mathf.FloorToInt(_z));

		if (!IsVoxelInWorld(mixPos))
			return (false);

		Coords chunkPos = new Coords();
		
		chunkPos.x = Mathf.FloorToInt(mixPos.x / WorldData.ChunkSize);
		chunkPos.y = Mathf.FloorToInt(mixPos.y / WorldData.ChunkSize);
		chunkPos.z = Mathf.FloorToInt(mixPos.z / WorldData.ChunkSize);

		mixPos.x -= chunkPos.x * WorldData.ChunkSize;
		mixPos.y -= chunkPos.y * WorldData.ChunkSize;
		mixPos.z -= chunkPos.z * WorldData.ChunkSize;

		return (blocktypes (int)[FindChunk(chunkPos).voxelMap [mixPos.x, mixPos.y, mixPos.z]].isSolid);
	}
*/