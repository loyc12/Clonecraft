using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class	World : MonoBehaviour
{
	public int				seed;
	public Coords			randomOffset;

	public BiomeAttributes	biome;
	public Terrain			defaultTerrain;

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

		defaultTerrain = new Terrain(this, biome);

		SpawnPlayer();
		if (WorldData.PreGenSpawn)
			GenerateSpawn();
	}

	private void	Update()
	{
		playerChunk = FindChunkPos(player.position);

		if (!playerChunk.SamePosAs(playerLastChunk))
			ApplyRenderDistance();

		if (0 < queuedChunks.Count && !isLoadingChunks)
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
		for (int y = playerChunk.y + WorldData.RenderDistance; y >= playerChunk.y - WorldData.RenderDistance ; y--)
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

		for (int r = 1; r < WorldData.RenderLimit; r++)
		{
			int	i = 0;
			while (0 < queuedChunks.Count && i < queuedChunks.Count)
			{
				Chunk	targetChunk = FindChunk(queuedChunks[i]);

				if (targetChunk.isGenerated)
				{
					LoadOrUnload(targetChunk);
					queuedChunks.RemoveAt(i);
				}
				else
				{
					if (IsChunkTooFar(targetChunk.chunkPos, WorldData.RenderLimit))
						queuedChunks.RemoveAt(i);
					else if (!IsChunkTooFar(targetChunk.chunkPos, r))
					{
						queuedChunks.RemoveAt(i);
						targetChunk.Generate();
						LoadOrUnload(targetChunk);

						r = 1;
						yield return (null);
					}
					else
						i++;
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
		if (!chunkPos.IsChunkInWorld() || IsChunkTooFar(chunkPos, WorldData.RenderDistance))
			return (false);

		if (WorldData.RenderDistance < playerChunk.SphereDistance(chunkPos))
			return (false);

		return (true);
	}

	//returns true if the given pos further than render limit (cube)
	bool	IsChunkTooFar(Coords chunkPos, float chunkDistance)
	{
		if (!chunkPos.IsChunkInWorld() || chunkDistance < playerChunk.CubeDistance(chunkPos))
			return (true);

		return (false);
	}

	//returns the block ID of the specified location
	public BlockID	FindBlockID(Coords worldPos)				//CheckForVoxel
	{

		//if block is outside world, return air
		if (!worldPos.IsBlockInWorld())
			return (BlockID.AIR);

		Coords	chunkPos = new Coords(worldPos.DivPos(WorldData.ChunkSize));
		Chunk	targetChunk = FindChunk(chunkPos);

		//is block's chunk exist, go get block
		if (targetChunk != null)
		{
			if (targetChunk.isPopulated && targetChunk.isEmpty)
				return (BlockID.AIR);

			Coords blockPos = worldPos.SubPos(chunkPos.MulPos(WorldData.ChunkSize));

			if (targetChunk.isPopulated)
				return (chunkMap[chunkPos.x, chunkPos.y, chunkPos.z].blockMap[blockPos.x, blockPos.y, blockPos.z]);
		}
		//else get new block
		return (defaultTerrain.GetBlockID(worldPos));
	}

	public Chunk	FindChunk(Coords chunkPos)
	{
		if (chunkPos.IsChunkInWorld())
			return (chunkMap[chunkPos.x, chunkPos.y, chunkPos.z]);

		return (null);
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
