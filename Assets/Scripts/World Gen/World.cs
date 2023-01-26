using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class	World : MonoBehaviour
{

	public int				chunkLoadLimit;			//chunk generation attempt# before force yield (WIP, broken)
	public bool				Flatland;
	public bool				UseSimpleGen;
	public bool				Use3DGen;
	public bool				ProcessSoil;
	public bool				UseCaveGen;
	public bool				UseTreeGen;
	public bool				SpawnAtCenter;
	public bool				PreGenSpawn;

	public int				seed;
	public Coords			randomOffset;

	public BiomeAttributes	biome;
	public Terrain			defaultTerrain;

	public Transform		player;
	public Coords 			spawnPoint;

	public Material			blockAtlas;
	public Material			transparentAtlas;
	public BlockType[]		blocktypes;

	Chunk[,,]				chunkMap = new Chunk[WorldData.WorldChunkSize, WorldData.WorldChunkHeight, WorldData.WorldChunkSize];	//Chunks

	public Coords			playerChunk;
	Coords					playerLastChunk;

	List<Coords>			loadedChunks = new List<Coords>();			//activeChunks
	List<Coords>			queuedChunks = new List<Coords>();			//chunksToCreate
	List<Chunk>				chunksToUpdate = new List<Chunk>();			//TODO fixe these names smh
	private bool			isModifyingChunks = false;					//applyingModifications

	Queue<BlockMod>			worldBlockQueue = new Queue<BlockMod>();	//modifications (for generating structures)
	//private bool			isWorldBlockQueueUsed = false;				USEME

	public GameObject		debugScreen;

	private void	Start()
	{
		InitializeRandomness();

		defaultTerrain = new Terrain(this, biome);

		SpawnPlayer();
		if (PreGenSpawn)
			GenerateSpawn();
		else
			ApplyRenderDistance();
	}

	private void	Update()
	{
		playerChunk = FindChunkPos(player.position);

		if (!playerChunk.IsEqual(playerLastChunk))
			ApplyRenderDistance();

		if (!isModifyingChunks && 0 < worldBlockQueue.Count)
			StartCoroutine(ModifyChunks());

		if (!isModifyingChunks && 0 < queuedChunks.Count)
			StartCoroutine(LoadChunk());						//BUGGED
			//CreateChunk();

		if (0 < chunksToUpdate.Count)
			UpdateChunks();

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
		for (int y = playerChunk.y - WorldData.GenerationDistance; y < playerChunk.y + WorldData.GenerationDistance; y++)
		{
			for (int x = playerChunk.x -WorldData.GenerationDistance; x < playerChunk.x + WorldData.GenerationDistance; x++)
			{
				for (int z = playerChunk.z - WorldData.GenerationDistance; z < playerChunk.z + WorldData.GenerationDistance; z++)
				{
					Coords	chunkPos = new Coords(x, y, z);

					if (chunkPos.IsChunkInWorld())
					{
						chunkMap[x, y, z] = new Chunk(chunkPos, this, true);
						chunkMap[x, y, z].Load();
						loadedChunks.Add(chunkPos);
					}
				}
			}
		}
		ProcessWorldBlockQueue(true);
	}

	void	ProcessWorldBlockQueue(bool forceLoad)
	{
		while (worldBlockQueue.Count > 0)
		{
			BlockMod	mod = worldBlockQueue.Dequeue();

			Coords	chunkPos = mod.worldPos.WorldToChunkPos();
			int		x = chunkPos.x;
			int		y = chunkPos.y;
			int		z = chunkPos.z;

			if (chunkMap[x, y, z] == null)
			{
				if (forceLoad)
				{
					chunkMap[x, y, z] = new Chunk(chunkPos, this, true);
					chunkMap[x, y, z].Load();
					loadedChunks.Add(chunkPos);
				}
				else
					chunkMap[x, y, z] = new Chunk(chunkPos, this, false);
					queuedChunks.Add(chunkPos);
			}

			chunkMap[x, y, z].chunkBlockQueue.Enqueue(mod);

			//if (chunkMap[x, y, z].isLoaded || forceLoad)		//laggy af
			if (forceLoad)
			{
				if (!chunksToUpdate.Contains(chunkMap[x, y, z]))
					chunksToUpdate.Add(chunkMap[x, y, z]);

				while(chunksToUpdate.Count > 0)
				{
					chunksToUpdate[0].BuildChunkMesh();
					chunksToUpdate.RemoveAt(0);
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

		ProcessWorldBlockQueue(false);							//REMOVE ME : TRYING TO FIT THIS IN LOGICALLY

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
		if (SpawnAtCenter)
			spawnPoint = new Coords(Mathf.FloorToInt(WorldData.WorldBlockSize / 2f), WorldData.WorldBlockHeight, Mathf.FloorToInt(WorldData.WorldBlockSize / 2f));
		else
			spawnPoint = new Coords(8, WorldData.WorldBlockHeight, 8);

		if (Flatland)
			spawnPoint.y = (1 + WorldData.WorldBlockHeight / 2);
		else if (!Use3DGen)
			spawnPoint.y = (1 + defaultTerrain.GetTerrainHeight(spawnPoint));

		player.position = new Vector3(spawnPoint.x + 0.5f, spawnPoint.y + 0.1f, spawnPoint.z + 0.5f);

		playerLastChunk = FindChunkPos(player.position);
		playerChunk = FindChunkPos(player.position);
	}

	void	CreateChunk()
	{
		Coords  chunkPos = queuedChunks[0];

		loadedChunks.Add(chunkPos);
		queuedChunks.RemoveAt(0);

		int	y = chunkPos.y;
		int	x = chunkPos.x;
		int	z = chunkPos.z;

		chunkMap[x, y, z].Generate();
		LoadOrUnload(chunkMap[x, y, z]);
	}

	IEnumerator LoadChunk()	//CreateChunk
	{
		isModifyingChunks = true;

		for (int r = 1; r < WorldData.RenderLimit; r++)
		{
			int	i = 0;

			while (0 < queuedChunks.Count && i < queuedChunks.Count)
			{
				Chunk	targetChunk = FindChunk(queuedChunks[i]);

				//if (chunkLoadLimit < i)
					//yield return (null);
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
						targetChunk.Generate();
						queuedChunks.RemoveAt(i);
						LoadOrUnload(targetChunk);

						r = 1;					//superfluous?
						yield return (null);
					}
					else
						i++;
				}
			}
		}
		isModifyingChunks = false;
	}

	public void		AddBlockToQueue(Coords worldPos, BlockID blockID, bool forcePlace)
	{
		if (worldPos.IsBlockInWorld())
			worldBlockQueue.Enqueue(new BlockMod(worldPos, blockID, forcePlace));
	}

	IEnumerator ModifyChunks()	//ApplyModifications
	{
		int	count = 0;

		isModifyingChunks = true;

		while(worldBlockQueue.Count > 0)
		{
			BlockMod	mod = worldBlockQueue.Dequeue();

			Coords	chunkPos = mod.worldPos.WorldToChunkPos();
			int		x = chunkPos.x;
			int		y = chunkPos.y;
			int		z = chunkPos.z;

			if (chunkMap[x, y, z] == null)
			{
				chunkMap[x, y, z] = new Chunk(chunkPos, this, false);
				queuedChunks.Add(chunkPos);
			}

			count++;

			if (chunkLoadLimit < count)
			{
				count = 0;
				yield return (null);
			}
		}

		isModifyingChunks = false;
	}

	void	UpdateChunks()
	{
		bool	hasUpdated = false;
		int		i = 0;

		while (!hasUpdated && i < chunksToUpdate.Count)	//	- 1?
		{
			if (chunksToUpdate[i].isPopulated)
			{
				chunksToUpdate[i].BuildChunkMesh();
				chunksToUpdate.RemoveAt(i);
				hasUpdated = true;
			}
			else
				i++;
		}
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

	public bool	IsBlockSolid(Coords worldPos)
	{
		return (blocktypes[(int)FindBlockID(worldPos)].isSolid);
	}

	public bool IsBlockOpaque(Coords worldPos)
	{
		return (blocktypes[(int)FindBlockID(worldPos)].isOpaque);
	}

	public bool IsBlockPresent(Coords worldPos)
	{
		if (FindBlockID(worldPos) == BlockID.AIR)
			return (false);
		return (true);
	}
}
