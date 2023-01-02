using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*	IDEAS



*/

public class	Chunk
{
	public Coords		chunkPos;

	GameObject			chunkObject;
	MeshRenderer		meshRenderer;
	MeshFilter			meshFilter;

	List<Vector3> 		vertices = new List<Vector3>();
	List<int> 			triangles = new List<int>();
	List<int> 			transparentTriangles = new List<int>();
	Material[]			atlasMap = new Material[2];
	List<Vector2>		uvs = new List<Vector2>();

	public BlockID[,,]	blockMap = new BlockID[WorldData.ChunkSize, WorldData.ChunkSize, WorldData.ChunkSize];	//map of the IDs of every block in the current chunk

	World				world;

	private bool		_isLoaded = false;
	public bool			isGenerated = false;
	public bool			isPopulated = false;
	public bool			isEmpty = true;

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

		atlasMap[0] = world.blockAtlas;
		atlasMap[1] = world.transparentAtlas;
		meshRenderer.materials = atlasMap;

		chunkObject.transform.SetParent(world.transform);
		chunkObject.transform.position = new Vector3(chunkPos.x * WorldData.ChunkSize, chunkPos.y * WorldData.ChunkSize, chunkPos.z * WorldData.ChunkSize);
		chunkObject.name = "Chunk " + chunkPos.x + ":" + chunkPos.y + ":" + chunkPos.z;

		PopulateBlockMap();

		if (isPopulated && !isEmpty)
			BuildChunkMesh();

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
		for (int y = WorldData.ChunkSize - 1; y >= 0; y--)
		{
			for (int x = 0; x < WorldData.ChunkSize; x++)
			{
				for (int z = 0; z < WorldData.ChunkSize; z++)
				{
					Coords blockPos = new Coords(x, y, z);
					blockMap[x, y, z] = world.defaultTerrain.GetBlockID(blockPos.AddPos(chunkWorldPos));
					if (blockMap[x, y, z] != BlockID.AIR)
						isEmpty = false;
				}
			}
		}

		isPopulated = true;
	}

	//returns true of the given voxel is solid
	bool	CheckBlockSolidity (Coords blockPos)		//CheckVoxel
	{
		if (!blockPos.IsBlockInChunk())
			return (world.blocktypes [(int)FindBlockID(blockPos)].isSolid);

		Coords	worldPos = blockPos.AddPos(chunkWorldPos);
		return (world.IsBlockSolid(worldPos));
	}

	//returns true of the given voxel is opaque
	bool	CheckBlockOpacity (Coords blockPos)	//CheckVoxel
	{
		if (blockPos.IsBlockInChunk())
			return (world.blocktypes [(int)FindBlockID(blockPos)].isOpaque);

		Coords	worldPos = blockPos.AddPos(chunkWorldPos);
		return (world.IsBlockOpaque(worldPos));
	}

	//compares the BlockID of two given blockPos
	bool	SameBlockID(Coords firstBlockPos, Coords secondBlockPos)
	{
		BlockID	firstBlockID = FindBlockID(firstBlockPos);
		BlockID	secondBlockID = FindBlockID(secondBlockPos);

		if (firstBlockID == secondBlockID)
			return (true);
		return (false);
	}

	public BlockID FindBlockID(Coords blockPos)	//GetVoxelFromGlobalVector3
	{

		//is block is in chunk, get it
		if (blockPos.IsBlockInChunk())
			return (blockMap[blockPos.x, blockPos.y, blockPos.z]);

		//else, go look in world
		Coords worldPos = blockPos.BlockToWorldPos(chunkPos);
		return (world.FindBlockID(worldPos));
	}

	public void	SetBlockID(Coords worldPos, BlockID value)	//EditVoxel
	{
		Coords blockPos = worldPos.WorldToBlockPos();

		if (!blockPos.IsBlockInChunk() || BlockType.maxID < value)
			return;

		blockMap[blockPos.x, blockPos.y, blockPos.z] = value;

		BuildChunkMesh();

		UpdateNeighboringChunk(blockPos);
	}

	//(re)loads the chunk's mesh
	void	UpdateNeighboringChunk(Coords blockPos)	//UpdateSurroundingVoxels
	{
		for (int faceIndex = 0; faceIndex < 6; faceIndex++)
		{
			Coords updateBlock = blockPos.GetNeighbor(faceIndex);

			if (!updateBlock.IsBlockInChunk() && updateBlock.BlockToWorldPos(updateBlock).IsBlockInWorld())
				world.FindChunk(chunkPos.GetNeighbor(faceIndex)).BuildChunkMesh();
		}
	}

	//(re)loads the chunk's mesh
	void	BuildChunkMesh()	//UpdateChunk
	{
		ClearChunkMesh();

		for (int y = 0; y < WorldData.ChunkSize; y++)
		{
			for (int x = 0; x < WorldData.ChunkSize; x++)
			{
				for (int z = 0; z < WorldData.ChunkSize; z++)
				{
					AddBlockDataToChunk(new Coords(x, y, z));	//put non opaque blocks in a different mesh?
				}
			}
		}

		CreateChunkMesh();
	}

	//adds the triangels and textures of a single block to the chunk mesh
	void	AddBlockDataToChunk(Coords blockPos)
	{
		BlockID	blockID = blockMap[blockPos.x, blockPos.y, blockPos.z];

		if (blockID > BlockID.AIR)
		{
			isEmpty = false;
			for (int faceIndex = 0; faceIndex < 6; faceIndex++)
			{
				Coords	neighborPos = blockPos.GetNeighbor(faceIndex);

				if (!CheckBlockOpacity(neighborPos) && !SameBlockID(blockPos, neighborPos))
				{
					Vector3 vPos = blockPos.ToVector3();

					if (CheckBlockOpacity(blockPos))
					{
						AddQuad (
							vPos + VoxelData.voxelVerts [VoxelData.voxelQuads [faceIndex, 0]],
							vPos + VoxelData.voxelVerts [VoxelData.voxelQuads [faceIndex, 1]],
							vPos + VoxelData.voxelVerts [VoxelData.voxelQuads [faceIndex, 2]],
							vPos + VoxelData.voxelVerts [VoxelData.voxelQuads [faceIndex, 3]]
						);
					}
					else
					{
						AddTransparentQuad (
							vPos + VoxelData.voxelVerts [VoxelData.voxelQuads [faceIndex, 0]],
							vPos + VoxelData.voxelVerts [VoxelData.voxelQuads [faceIndex, 1]],
							vPos + VoxelData.voxelVerts [VoxelData.voxelQuads [faceIndex, 2]],
							vPos + VoxelData.voxelVerts [VoxelData.voxelQuads [faceIndex, 3]]
						);
					}
					AddTexture(world.blocktypes[(int)blockID].GetTextureId(faceIndex));

				}
			}
		}
	}

	//clears the chunk's mesh
	void	ClearChunkMesh()	//ClearMeshData
	{
		Mesh	mesh = new Mesh();

		isEmpty = true;

		vertices.Clear();
		triangles.Clear();
		transparentTriangles.Clear();
		uvs.Clear();
	}

	//initiates the chunk's mesh
	void	CreateChunkMesh()	//CreateMesh
	{
		Mesh	mesh = new Mesh();

		mesh.vertices = vertices.ToArray();
		//mesh.triangles = triangles.ToArray();

		mesh.subMeshCount = 2;
		mesh.SetTriangles(triangles.ToArray(), 0);
		mesh.SetTriangles(transparentTriangles.ToArray(), 1);
		mesh.uv = uvs.ToArray();

		mesh.RecalculateNormals();

		meshFilter.mesh = mesh;
	}

	public Coords chunkWorldPos	//position
	{
		get { return (chunkPos.ChunkToWorldPos()); }
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
	//draws a square with two triangles
	void	AddTransparentQuad (Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
	{
		int vertexIndex = vertices.Count;

		vertices.Add(v0);
		vertices.Add(v1);
		vertices.Add(v2);
		vertices.Add(v3);

		transparentTriangles.Add(vertexIndex);
		transparentTriangles.Add(vertexIndex + 1);
		transparentTriangles.Add(vertexIndex + 2);

		transparentTriangles.Add(vertexIndex + 2);
		transparentTriangles.Add(vertexIndex + 3);
		transparentTriangles.Add(vertexIndex);
	}

	//draws a single triangle
	void	AddTransparentTriangle (Vector3 v0, Vector3 v1, Vector3 v2)
    {
		int vertexIndex = vertices.Count;

		vertices.Add(v0);
		vertices.Add(v1);
		vertices.Add(v2);

		transparentTriangles.Add(vertexIndex);
		transparentTriangles.Add(vertexIndex + 1);
		transparentTriangles.Add(vertexIndex + 2);
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
