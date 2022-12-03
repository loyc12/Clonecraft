using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class	Chunk
{
	public Coords		chunkPos;

	GameObject			chunkObject;
	MeshRenderer		meshRenderer;
	MeshFilter			meshFilter;

	List<Vector3> 		vertices = new List<Vector3>();
	List<int> 			triangles = new List<int>();
	List<Vector2>		uvs = new List<Vector2>();

	public BlockID[,,]	blockMap = new BlockID[WorldData.ChunkSize, WorldData.ChunkSize, WorldData.ChunkSize];	//map of the IDs of every block in the current chunk

	World				world;

	private bool	_isLoaded = false;
	public bool		isGenerated = false;
	public bool		isPopulated = false;
	public bool		isEmpty = true;

	//chunk fabricator
	public	Chunk (Coords _chunkPos, World _world, bool forceLoad)
	{
		chunkPos = _chunkPos;
		world = _world;

		if (forceLoad)
			Generate();
	}

	public void Generate()	//Init
	{
		chunkObject = new GameObject();
		meshFilter = chunkObject.AddComponent<MeshFilter>();
		meshRenderer = chunkObject.AddComponent<MeshRenderer>();

		meshRenderer.material = world.material;
		chunkObject.transform.SetParent(world.transform);
		chunkObject.transform.position = new Vector3(chunkPos.x * WorldData.ChunkSize, chunkPos.y * WorldData.ChunkSize, chunkPos.z * WorldData.ChunkSize);
		chunkObject.name = "Chunk " + chunkPos.x + ":" + chunkPos.y + ":" + chunkPos.z;

		PopulateBlockMap();
		if (isPopulated && !isEmpty)
		{
			LoadChunkMesh();
			BuildChunkMesh();
		}
		
		isGenerated = true;

	}

	public void Load()
	{
		isLoaded = true;
	}

	public void Unload()
	{
		isLoaded = false;
	}

	//pregenerate the chunk's voxel map for use in mesh loading
	void	PopulateBlockMap()
	{
		for (int y = 0; y < WorldData.ChunkSize; y++)
		{
			for (int x = 0; x < WorldData.ChunkSize; x++)
			{
				for (int z = 0; z < WorldData.ChunkSize; z++)
				{
					Coords blockPos = new Coords(x, y, z);
					blockMap[x, y, z] = world.GetBlockID(blockPos.AddPos(chunkWorldPos));
					if (blockMap[x, y, z] != BlockID.AIR)
						isEmpty = false;
				}
			}
		}

		isPopulated = true;
	}

	//returns true if the given voxel is inside the current chunk
	bool	IsBlockInChunk(Coords blockPos)
	{
		if (blockPos.x < 0 || WorldData.ChunkSize <= blockPos.x)
			return (false);
		if (blockPos.y < 0 || WorldData.ChunkSize <= blockPos.y)
			return (false);
		if (blockPos.z < 0 || WorldData.ChunkSize <= blockPos.z)
			return (false);
		return (true);
	}

	//returns true of the given voxel is solid
	bool	CheckBlockSolidity (Coords blockPos)		//CheckVoxel
	{
		Coords	worldPos = blockPos.AddPos(chunkWorldPos);
	
		if (!IsBlockInChunk(blockPos))
			return (world.IsBlockSolid(worldPos));

		return (world.blocktypes [(int)FindBlockID(worldPos)].isSolid);
	}

	//returns true of the given voxel is opaque
	bool	CheckBlockOpacity (Coords blockPos)	//CheckVoxel
	{
		Coords	worldPos = blockPos.AddPos(chunkWorldPos);
		
		if (!IsBlockInChunk(blockPos))
			return (world.IsBlockOpaque(worldPos));

		return (world.blocktypes [(int)FindBlockID(worldPos)].isOpaque);
	}

	public BlockID FindBlockID(Coords worldPos)	//GetVoxelFromGlobalVector3
	{
		Coords blockPos = new Coords(worldPos.SubPos(chunkPos));
		
		if (!IsBlockInChunk(blockPos))
			return (world.GetBlockID(worldPos));					//NORMALLY IS SUPPOSED TO TAKE THE DATA FROM ANOTHER CHUNK

		return blockMap[blockPos.x, blockPos.y, blockPos.z];
	}

	//adds the triangels and textures of a single block to the chunk mesh
	void	AddBlockDataToChunk(Coords blockPos)
	{
		BlockID blockID = blockMap[blockPos.x, blockPos.y, blockPos.z];

		if (blockID > 0)
		{
			for (int faceIndex = 0; faceIndex < 6; faceIndex++)
			{
				if (!CheckBlockOpacity(blockPos.GetNeighbor(faceIndex)))
				{
					Vector3 vPos = blockPos.ToVector3();
					AddQuad (
						vPos + VoxelData.voxelVerts [VoxelData.voxelQuads [faceIndex, 0]],
						vPos + VoxelData.voxelVerts [VoxelData.voxelQuads [faceIndex, 1]],
						vPos + VoxelData.voxelVerts [VoxelData.voxelQuads [faceIndex, 2]],
						vPos + VoxelData.voxelVerts [VoxelData.voxelQuads [faceIndex, 3]]
					);
					AddTexture(world.blocktypes[(int)blockID].GetTextureId(faceIndex));
				}
			}
		}
	}

	public Coords chunkWorldPos	//position
	{
		get { return (chunkPos.MulPos(WorldData.ChunkSize)); }
	}

	//wheter the chunk is loaded or not
	public bool	isLoaded	//isActive
	{
		get { return _isLoaded; }
		set
		{
			_isLoaded = value;
			if (chunkObject != null)
				chunkObject.SetActive(value);
		}
	}

	//loads the chunk's mesh
	void	LoadChunkMesh()	//CreateMeshData
	{
		for (int y = 0; y < WorldData.ChunkSize; y++)
		{
			for (int x = 0; x < WorldData.ChunkSize; x++)
			{
				for (int z = 0; z < WorldData.ChunkSize; z++)
				{
					AddBlockDataToChunk(new Coords(x, y, z));
				}
			}
		}
	}

	//builds the chunk's mesh
	void	BuildChunkMesh()	//CreateMesh
	{
		Mesh	mesh = new Mesh();

		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uvs.ToArray();

		mesh.RecalculateNormals();

		meshFilter.mesh = mesh;
	}

	//add a texture to the lastest two triangle
	void	AddTexture(int textureID)
	{
		float	y = textureID / WorldData.TextureAtlasSize;
		float	x = textureID - (y * WorldData.TextureAtlasSize);

		x *= WorldData.NormalizedTextureSize;
		y *= WorldData.NormalizedTextureSize;

		uvs.Add(new Vector2(x, y));
		uvs.Add(new Vector2(x, y + WorldData.NormalizedTextureSize));
		uvs.Add(new Vector2(x + WorldData.NormalizedTextureSize, y + WorldData.NormalizedTextureSize));
		uvs.Add(new Vector2(x + WorldData.NormalizedTextureSize, y));
	}

	//draws a square with two triangles
	void	AddQuad (Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
	{
		int vertexIndex = vertices.Count;

		vertices.Add(v0);
		vertices.Add(v1);
		vertices.Add(v2);
		vertices.Add(v3);

		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);

		triangles.Add(vertexIndex + 2);
		triangles.Add(vertexIndex + 3);
		triangles.Add(vertexIndex);
	}
	//draws a single triangle
	void	AddTriangle (Vector3 v0, Vector3 v1, Vector3 v2)
    {
		int vertexIndex = vertices.Count;

		vertices.Add(v0);
		vertices.Add(v1);
		vertices.Add(v2);

		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
	}

}
