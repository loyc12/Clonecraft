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

	public byte[,,]		voxelMap = new byte[WorldData.ChunkSize, WorldData.ChunkSize, WorldData.ChunkSize];	//map of the IDs of every block in the current chunk

	World				world;

	private bool		_isActive;
	public	bool		isPopulated = false;

	//chunk fabricator
	public	Chunk (Coords _chunkPos, World _world, bool forceLoad)
	{
		chunkPos = _chunkPos;
		world = _world;
		isActive = true;

		if (forceLoad)
			Load();
	}

	public void Load()
	{
		chunkObject = new GameObject();
		meshFilter = chunkObject.AddComponent<MeshFilter>();
		meshRenderer = chunkObject.AddComponent<MeshRenderer>();

		meshRenderer.material = world.material;
		chunkObject.transform.SetParent(world.transform);
		chunkObject.transform.position = new Vector3(chunkPos.x * WorldData.ChunkSize, chunkPos.y * WorldData.ChunkSize, chunkPos.z * WorldData.ChunkSize);
		chunkObject.name = "Chunk " + chunkPos.x + ":" + chunkPos.y + ":" + chunkPos.z;

		PopulateVoxelMap();
		LoadChunkMesh();
		CreateMesh();
	}

	//pregenerate the chunk's voxel map for use in mesh loading
	void	PopulateVoxelMap()
	{
		for (int y = 0; y < WorldData.ChunkSize; y++)
		{
			for (int x = 0; x < WorldData.ChunkSize; x++)
			{
				for (int z = 0; z < WorldData.ChunkSize; z++)
				{
					Coords blockPos = new Coords(x, y, z);
					voxelMap[x, y, z] = (byte)world.GetBlockID(blockPos.AddPos(chunkObjectPos));
				}
			}
		}

		isPopulated = true;
	}

	//returns true if the given voxel is inside the current chunk
	bool	IsVoxelInChunk(int x, int y, int z)
	{
		if (x < 0 || WorldData.ChunkSize <= x)
			return (false);
		if (y < 0 || WorldData.ChunkSize <= y)
			return (false);
		if (z < 0 || WorldData.ChunkSize <= z)
			return (false);
		return (true);
	}

	//returns true of the given voxel is not opaque
	bool	CheckVoxelTransparency (Coords blockPos)	//CheckVoxel
	{
		int	x = blockPos.x;
		int	y = blockPos.y;
		int	z = blockPos.z;

		if (!IsVoxelInChunk(x, y, z))
			return (world.CheckForVoxel(blockPos.AddPos(chunkObjectPos)));

		return (world.blocktypes [voxelMap [x, y, z]].isOpaque);
	}

	public byte FindBlockPos(Coords worldPos)	//GetVoxelFromGlobalVector3
	{
		Coords blockPos = new Coords(worldPos.DivPos(WorldData.ChunkSize));

		return voxelMap[blockPos.x, blockPos.y, blockPos.z];
	}

	//the current Chunk.position
	public Coords chunkObjectPos	//position
	{
		get
		{
			int	x = Mathf.FloorToInt(chunkObject.transform.position.x);
			int	y = Mathf.FloorToInt(chunkObject.transform.position.y);
			int	z = Mathf.FloorToInt(chunkObject.transform.position.z);

			return (new Coords(x, y, z));
			
		}
	}

	//creates the chunk's mesh
	void	LoadChunkMesh()
	{
		for (int y = 0; y < WorldData.ChunkSize; y++)
		{
			for (int x = 0; x < WorldData.ChunkSize; x++)
			{
				for (int z = 0; z < WorldData.ChunkSize; z++)
				{
					AddVoxelDataToChunk(new Coords(x, y, z));
				}
			}
		}
	}

	//adds the triangels and textures of a single block to the chunk mesh
	void	AddVoxelDataToChunk(Coords blockPos)
	{
		byte blockID = voxelMap [blockPos.x, blockPos.y, blockPos.z];

		if (blockID > 0)
		{
			for (int faceIndex = 0; faceIndex < 6; faceIndex++)
			{
				if (!CheckVoxelTransparency(blockPos.GetNeighbor(faceIndex)))
				{
					Vector3 vPos = blockPos.ToVector3();
					AddQuad (
						vPos + VoxelData.voxelVerts [VoxelData.voxelQuads [faceIndex, 0]],
						vPos + VoxelData.voxelVerts [VoxelData.voxelQuads [faceIndex, 1]],
						vPos + VoxelData.voxelVerts [VoxelData.voxelQuads [faceIndex, 2]],
						vPos + VoxelData.voxelVerts [VoxelData.voxelQuads [faceIndex, 3]]
					);
					AddTexture(world.blocktypes[blockID].GetTextureId(faceIndex));
				}
			}
		}
	}

	//creates a single chunk's mesh
	void	CreateMesh()
	{
		Mesh	mesh = new Mesh();

		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uvs.ToArray();

		mesh.RecalculateNormals();

		meshFilter.mesh = mesh;
	}

	//wheter the chunk is loaded or not
	public bool	isActive
	{
		get { return _isActive; }
		set
		{
			_isActive = value;
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
