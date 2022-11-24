using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class	Chunk
{
	public ChunkCoord	coord;

	GameObject			chunkObject;
	MeshRenderer		meshRenderer;
	MeshFilter			meshFilter;

	List<Vector3> 		vertices = new List<Vector3>();
	List<int> 			triangles = new List<int>();
	List<Vector2>		uvs = new List<Vector2>();

	World				world;

	byte[,,]	voxelMap = new byte[WorldData.ChunkSize, WorldData.ChunkSize, WorldData.ChunkSize];	//map of the IDs of every block in the current chunk

	//chunk fabricator
	public	Chunk (ChunkCoord _coord, World _world)
	{
		coord = _coord;
		world = _world;
		chunkObject = new GameObject();
		meshFilter = chunkObject.AddComponent<MeshFilter>();
		meshRenderer = chunkObject.AddComponent<MeshRenderer>();

		meshRenderer.material = world.material;
		chunkObject.transform.SetParent(world.transform);
		chunkObject.transform.position = new Vector3(coord.cx * WorldData.ChunkSize, coord.cy * WorldData.ChunkSize, coord.cz * WorldData.ChunkSize);
		chunkObject.name = "Chunk " + coord.cx + ":" + coord.cy + ":" + coord.cz;

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
					voxelMap[x, y, z] = world.GetBlockID(new Vector3(x, y, z) + chunkpos);
				}
			}
		}
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
	bool	CheckVoxelTransparency (Vector3 pos)
	{
		int	x = (int)pos.x;
		int	y = (int)pos.y;
		int	z = (int)pos.z;

		if (!IsVoxelInChunk(x, y, z))
			return (world.blocktypes [world.GetBlockID(pos + chunkpos)].isOpaque);
		return (world.blocktypes [voxelMap [x, y, z]].isOpaque);
	}

	//wheter the chunk is loaded or not
	public bool	isActive
	{
		get { return chunkObject.activeSelf; }
		set { chunkObject.SetActive(value); }
	}

	//the current chunk's pos
	public Vector3 chunkpos
	{
		get { return chunkObject.transform.position; }
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
					AddVoxelDataToChunk(new Vector3(x, y, z));
				}
			}
		}
	}

	//adds the triangels and textures of a single block to the chunk mesh
	void	AddVoxelDataToChunk(Vector3 pos)
	{
		byte blockID = voxelMap [(int)pos.x, (int)pos.y, (int)pos.z];

		if (blockID > 0)
		{
			for (int face = 0; face < 6; face++)
			{
				if (!CheckVoxelTransparency(pos + VoxelData.neighbors[face]))
				{
					AddQuad (
						pos + VoxelData.voxelVerts [VoxelData.voxelQuads [face, 0]],
						pos + VoxelData.voxelVerts [VoxelData.voxelQuads [face, 1]],
						pos + VoxelData.voxelVerts [VoxelData.voxelQuads [face, 2]],
						pos + VoxelData.voxelVerts [VoxelData.voxelQuads [face, 3]]
					);
					AddTexture(world.blocktypes[blockID].GetTextureId(face));
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
//coordinate system for chunks only
public class	ChunkCoord
{
	public int	cx;
	public int	cy;
	public int	cz;

	public	ChunkCoord (int _cx, int _cy, int _cz)
	{
		cx = _cx;
		cy = _cy;
		cz = _cz;
	}
}