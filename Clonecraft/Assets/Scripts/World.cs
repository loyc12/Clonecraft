using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class	World : MonoBehaviour
{
	public Transform		player;
	public Vector3 			spawnPoint;
	public ChunkCoord		playerChunk;
	public ChunkCoord		playerLastChunk;

	public Material			material;
	public BlockType[]		blocktypes;

	Chunk[,,]				region = new Chunk[VoxelData.WorldSize, VoxelData.WorldHeight, VoxelData.WorldSize];

	List<ChunkCoord>		activeChunks = new List<ChunkCoord>();

	private void	Start()
	{
		spawnPoint = new Vector3(VoxelData.WorldVoxelSize / 2f, VoxelData.WorldVoxelHeight, VoxelData.WorldVoxelSize / 2f);
		player.position = spawnPoint;
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

	void	GenerateWorld()
	{
		int	center = (int)(VoxelData.WorldSize / 2);

		for (int cx = center - VoxelData.RenderDistance; cx < center + VoxelData.RenderDistance; cx++)
		{
			for (int cy = 0; cy < VoxelData.WorldHeight; cy++)
			{
				for (int cz = center - VoxelData.RenderDistance; cz < center + VoxelData.RenderDistance; cz++)
				{
					CreateNewChunk(cx, cy, cz);
				}
			}
		}
	}

	ChunkCoord GetChunkPos (Vector3 pos)
	{
		int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkSize);
		int y = Mathf.FloorToInt(pos.y / VoxelData.ChunkSize);
		int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkSize);

		return (new ChunkCoord (x, y, z));
	}

	void	ExtendRenderDistance()
	{
		for (int cx = playerChunk.cx - VoxelData.RenderDistance; cx <= playerChunk.cx + VoxelData.RenderDistance; cx++)
		{
			for (int cz = playerChunk.cz - VoxelData.RenderDistance; cz <= playerChunk.cz + VoxelData.RenderDistance; cz++)
			{
				for (int cy = 0; cy < VoxelData.WorldHeight; cy++)
				{
					if (IsChunkInWorld(new ChunkCoord(cx, cy, cz)))
					{
						if (region[cx, cy, cz] == null)
							CreateNewChunk(cx, cy, cz);
						else if (!region[cx, cy, cz].isActive)
						{
							region[cx, cy, cz].isActive = true;
							activeChunks.Add(new ChunkCoord(cx, cy, cz));
						}

					}
				}
			}
		}
	}

	void	RetractRenderDistance()
	{
		for (int i = 0; i < activeChunks.Count; i++)
		{
			ChunkCoord	coord = activeChunks[i];
			if (!IsChunkInRenderDistance(coord))
			{
				region[coord.cx, coord.cy, coord.cz].isActive = false;
				activeChunks.Remove(coord);
			}
		}
	}

	void	CreateNewChunk(int cx, int cy, int cz)
	{
		region[cx, cy, cz] = new Chunk(new ChunkCoord(cx, cy, cz), this);
		activeChunks.Add(new ChunkCoord(cx, cy, cz));
	}

	public byte GetVoxel(Vector3 pos)
	{
		if (!IsVoxelInWorld(pos))
			return ((byte)BlockList.AIR);

		int	y = (int)pos.y;

		if (y == 0)
			return ((byte)BlockList.BEDROCK);
		else if (y < 8)
			return ((byte)BlockList.ROCK);
		else if (y < VoxelData.ChunkSize - 3)
			return ((byte)BlockList.STONE);
		else if (y < VoxelData.ChunkSize - 1)
			return ((byte)BlockList.DIRT);
		else if (y == VoxelData.ChunkSize - 1)
			return ((byte)BlockList.GRASS);
		else
			return ((byte)BlockList.MARBLE);
	}

	bool	IsChunkInRenderDistance(ChunkCoord coord)
	{

		if (coord.cx < playerChunk.cx - VoxelData.RenderDistance|| playerChunk.cx + VoxelData.RenderDistance < coord.cx)
			return (false);
		//if (coord.cy < 0 || VoxelData.WorldHeight <= coord.cy)	//useless
			//return (false);
		if (coord.cz < playerChunk.cz - VoxelData.RenderDistance|| playerChunk.cz + VoxelData.RenderDistance < coord.cz)
			return (false);
		return (true);
	}

	bool	IsChunkInWorld(ChunkCoord coord)
	{

		if (coord.cx < 0 || VoxelData.WorldSize <= coord.cx)
			return (false);
		if (coord.cy < 0 || VoxelData.WorldHeight <= coord.cy)
			return (false);
		if (coord.cz < 0 || VoxelData.WorldSize <= coord.cz)
			return (false);
		return (true);
	}

	bool	IsVoxelInWorld(Vector3 pos)
	{
		if (pos.x < 0 || VoxelData.WorldVoxelSize <= pos.x)
			return (false);
		if (pos.y < 0 || VoxelData.WorldVoxelHeight <= pos.y)
			return (false);
		if (pos.z < 0 || VoxelData.WorldVoxelSize <= pos.z)
			return (false);
		return (true);
	}
}

[System.Serializable]
public class	BlockType
{
	public string 	blockName;
	public bool		isSolid;
	public bool		isMonofaced;

	[Header("Texture Values")]
	public int	frontFaceTexture;
	public int	backFaceTexture;
	public int	topFaceTexture;
	public int	bottomFaceTexture;
	public int	leftFaceTexture;
	public int	rightFaceTexture;

	public int		GetTextureId(int faceIndex)
	{
		if (isMonofaced)
			return frontFaceTexture;
		switch (faceIndex)
		{
			case 0:
				return (frontFaceTexture);
			case 1:
				return (backFaceTexture);
			case 2:
				return (topFaceTexture);
			case 3:
				return (bottomFaceTexture);
			case 4:
				return (leftFaceTexture);
			case 5:
				return (rightFaceTexture);
			default:
				Debug.Log("Error in GetTextureID : invalide face index given");
				return (0);
		}
	}
}