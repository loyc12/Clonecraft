using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class	World : MonoBehaviour
{
	public int				seed;
	public BiomeAttributes	biome;

	public Transform		player;
	public Vector3 			spawnPoint;
	public ChunkCoord		playerChunk;
	public ChunkCoord		playerLastChunk;

	public Material			material;
	public BlockType[]		blocktypes;

	Chunk[,,]				region = new Chunk[WorldData.WorldSize, WorldData.WorldHeight, WorldData.WorldSize];	//stores all chunks for now

	List<ChunkCoord>		activeChunks = new List<ChunkCoord>();	//stores active chunks

	private void	Start()
	{
		Random.InitState(seed);
	
		spawnPoint = new Vector3(WorldData.WorldVoxelSize / 2f, WorldData.WorldVoxelHeight, WorldData.WorldVoxelSize / 2f);
		player.position = spawnPoint;
		playerChunk = GetChunkPos(player.position);
		playerLastChunk = GetChunkPos(player.position);

		GenerateWorld();
	}

	private void	Update()
	{
		playerChunk = GetChunkPos (player.position);
		if (playerChunk.cx != playerLastChunk.cx || playerChunk.cy != playerLastChunk.cy || playerChunk.cz != playerLastChunk.cz)
		{
			playerLastChunk = playerChunk;
			RetractRenderDistance();
			ExtendRenderDistance();
		}
	}

	//generate the chunks inside the render distance at spawn
	void	GenerateWorld()
	{
		int	center = (int)(WorldData.WorldSize / 2);

		for (int cx = center - WorldData.RenderDistance; cx < center + WorldData.RenderDistance; cx++)
		{
			for (int cy = 0; cy < WorldData.WorldHeight; cy++)
			{
				for (int cz = center - WorldData.RenderDistance; cz < center + WorldData.RenderDistance; cz++)
				{
					ChunkCoord	coord = new ChunkCoord(cx, cy, cz);

					if (IsChunkInRenderDistance(coord))
						CreateNewChunk(cx, cy, cz);
				}
			}
		}
	}

	//finds the chunk of a given voxel pos
	ChunkCoord GetChunkPos (Vector3 pos)
	{
		int x = Mathf.FloorToInt(pos.x / WorldData.ChunkSize);
		int y = Mathf.FloorToInt(pos.y / WorldData.ChunkSize);
		int z = Mathf.FloorToInt(pos.z / WorldData.ChunkSize);

		return (new ChunkCoord (x, y, z));
	}

	//creates or reactivates chunks inside the player's render distance
	void	ExtendRenderDistance()
	{
		for (int cx = playerChunk.cx - WorldData.RenderDistance; cx <= playerChunk.cx + WorldData.RenderDistance; cx++)
		{
			for (int cz = playerChunk.cz - WorldData.RenderDistance; cz <= playerChunk.cz + WorldData.RenderDistance; cz++)
			{
				for (int cy = 0; cy < WorldData.WorldHeight; cy++)
				{
					ChunkCoord	renderChunk = new ChunkCoord(cx, cy, cz);

					if (IsChunkInRenderDistance(renderChunk))
					{
						if (region[cx, cy, cz] == null)
							CreateNewChunk(cx, cy, cz);
						else if (!region[cx, cy, cz].isActive)
						{
							region[cx, cy, cz].isActive = true;
							activeChunks.Add(renderChunk);
						}

					}
				}
			}
		}
	}

	//deactivates chunks outside the player's render distance
	void	RetractRenderDistance()
	{
		for (int i = 0; i < activeChunks.Count; i++)
		{
			ChunkCoord	coord = activeChunks[i];
			if (!IsChunkInRenderDistance(coord))
			{
				region[coord.cx, coord.cy, coord.cz].isActive = false;
				activeChunks.Remove(coord);
				i--;
			}
		}
	}

	//creates and activates a chunk form a given chunk pos
	void	CreateNewChunk(int cx, int cy, int cz)
	{
		region[cx, cy, cz] = new Chunk(new ChunkCoord(cx, cy, cz), this);
		activeChunks.Add(new ChunkCoord(cx, cy, cz));
	}

	public byte GetBlockID(Vector3 pos)		//GetVoxel
	{
		int	y = (int)pos.y;

		/* === ABSOLUTE PASS === */
		if (!IsVoxelInWorld(pos))
			return ((byte)BlockID.AIR);
		else if (y == 0)
			return ((byte)BlockID.BEDROCK);
		else if (y < WorldData.RockLevel)
			return ((byte)BlockID.ROCK);

		/* === BASIC TERRAIN PASS === */
		int	height = (int)(WorldData.RockLevel + biome.baseElevation + (
			biome.maxElevation * Noise.Get2DNoise(new Vector2(pos.x, pos.z), 0, biome.terrainScale)
			));

		if (y > height)
			return ((byte)BlockID.AIR);
		else if (y > height - 3 && height < WorldData.SeaLevel - 2)
			return ((byte)BlockID.GRAVEL);
		else if (y > height - 3 && height < WorldData.SeaLevel + 1)
			return ((byte)BlockID.SAND);
		else if (y == height)
			return ((byte)BlockID.GRASS);
		else if (y > height - 3)
			return ((byte)BlockID.DIRT);
		else if (height < WorldData.SeaLevel && y > height - 6)
			return ((byte)BlockID.MARBLE);
		else
			return ((byte)BlockID.STONE);

	}

	//returns true if the given pos is inside the player's render distance
	bool	IsChunkInRenderDistance(ChunkCoord coord)
	{

		if (!IsChunkInWorld(coord) || WorldData.RenderDistance < playerChunk.ChunkDistance(coord))
			return (false);
		return (true);
	}

	//returns true if the given chunk pos is inside the worldgen limits
	bool	IsChunkInWorld(ChunkCoord coord)
	{

		if (coord.cx < 0 || WorldData.WorldSize <= coord.cx)
			return (false);
		if (coord.cy < 0 || WorldData.WorldHeight <= coord.cy)
			return (false);
		if (coord.cz < 0 || WorldData.WorldSize <= coord.cz)
			return (false);
		return (true);
	}

	//returns true if the given voxel pos is inside the worldgen limits
	bool	IsVoxelInWorld(Vector3 pos)
	{
		if (pos.x < 0 || WorldData.WorldVoxelSize <= pos.x)
			return (false);
		if (pos.y < 0 || WorldData.WorldVoxelHeight <= pos.y)
			return (false);
		if (pos.z < 0 || WorldData.WorldVoxelSize <= pos.z)
			return (false);
		return (true);
	}
}
