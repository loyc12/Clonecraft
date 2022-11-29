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
		//spawnPoint += new Vector3(0.5f, 0.1f, 0.5f);
		spawnPoint.y = GetTerrainHeight(spawnPoint) + 0.1f;
	
		player.position = spawnPoint;
		playerChunk = GetChunkPos(player.position);
		playerLastChunk = GetChunkPos(player.position);

		GenerateWorld();
	}

	private void	FixedUpdate()
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
		int	center = Mathf.FloorToInt(WorldData.WorldSize / 2);

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

	float	GetTerrainHeight(Vector3 pos)
	{
		float	height = biome.maxElevation * Noise.Get2DNoise(new Vector2(pos.x, pos.z), 0, biome.terrainScale);
		
		height += WorldData.RockLevel + biome.baseElevation;

		return (height);
	}

	public BlockID GetBlockID(Vector3 pos)		//GetVoxel
	{
		int	y = Mathf.FloorToInt(pos.y);
		BlockID blockID = BlockID.AIR;

		/* === ABSOLUTE PASS === */
		if (!IsVoxelInWorld(pos))
			return (BlockID.AIR);

		else if (y == 0)
			return (BlockID.BEDROCK);


		/* === BASIC TERRAIN PASS === */
		int	height = Mathf.FloorToInt(GetTerrainHeight(pos));

		if (y > height)
			return (blockID);
		else if (y > height - 3 && height < WorldData.SeaLevel - 2)
			blockID = BlockID.GRAVEL;
		else if (y > height - 3 && height < WorldData.SeaLevel + 1)
			blockID = BlockID.SAND;
		else if (y == height)
			blockID = BlockID.GRASS;
		else if (y > height - 3)
			blockID = BlockID.DIRT;
		else if (height < WorldData.SeaLevel + 2 && y > height - 6)
			blockID = BlockID.MARBLE;
		else
			blockID = BlockID.STONE;

		/* === ORE TERRAIN PASS === */
		if (blockID == BlockID.STONE || blockID == BlockID.DIRT || blockID == BlockID.GRASS)
		{
			foreach (Vein vein in biome.veins)
			{
				if (y >= vein.height - vein.spread && y <= vein.height + vein.spread)	//TEMP
					if (blockID != BlockID.AIR && Noise.Get3DVeinNoise(pos, vein))
						blockID = (BlockID)vein.blockID;
			}
		}

		/* === FINAL PASS === */
		if (y < WorldData.RockLevel && blockID == BlockID.STONE)
			blockID = BlockID.ROCK;

		return (blockID);
	}

	public bool	CheckForVoxel(float _x, float _y, float _z)
	{

		int x = Mathf.FloorToInt(_x);	//use Mathf.FloorToInt if buggy
		int y = Mathf.FloorToInt(_y);
		int z = Mathf.FloorToInt(_z);

		if (!IsVoxelInWorld(new Vector3(x, y, z)))
			return (false);

		int xChunk = x / WorldData.ChunkSize;
		int yChunk = y / WorldData.ChunkSize;
		int zChunk = z / WorldData.ChunkSize;

		x -= xChunk * WorldData.ChunkSize;
		y -= yChunk * WorldData.ChunkSize;
		z -= zChunk * WorldData.ChunkSize;

		return (blocktypes [region [xChunk, yChunk, zChunk].voxelMap [x, y, z]].isSolid);
	}

}
