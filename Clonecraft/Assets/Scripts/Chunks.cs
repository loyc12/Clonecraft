using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunks : MonoBehaviour
{

	public MeshRenderer		meshRenderer;
	public MeshFilter		meshFilter;
	public Color			defaultColor;

	byte[,,]		voxelMap;
	List<Vector3> 	vertices;
	List<int> 		triangles;
	List<Vector2>	uvs;
	Mesh 			mesh;
	World			world;

    void Start()
    {
		voxelMap = new byte[VoxelData.ChunkSize, VoxelData.ChunkSize, VoxelData.ChunkSize];
		vertices = new List<Vector3>();
		triangles = new List<int>();
		uvs = new List<Vector2>();
		mesh = new Mesh();
		world = GameObject.Find("World").GetComponent<World>();

		PopulateVoxelMap();
		LoadChunkMesh();
		CreateMesh();
    }

	void	PopulateVoxelMap()
	{
		for (int y = 0; y < VoxelData.ChunkSize; y++)
		{
			for (int x = 0; x < VoxelData.ChunkSize; x++)
			{
				for (int z = 0; z < VoxelData.ChunkSize; z++)
				{
					if (y < 1)
						voxelMap[x, y, z] = 0;
					else if (y == VoxelData.ChunkSize - 1)
						voxelMap[x, y, z] = 1;
					else if (y >  VoxelData.ChunkSize - 4)
						voxelMap[x, y, z] = 2;
					else
						voxelMap[x, y, z] = 3;
				}
			}
		}
	}

	bool CheckVoxel (Vector3 pos)
	{
		int	x = (int)pos.x;
		int	y = (int)pos.y;
		int	z = (int)pos.z;

		if (x < 0 || VoxelData.ChunkSize <= x)
			return (false);
		if (y < 0 || VoxelData.ChunkSize <= y)
			return (false);
		if (z < 0 || VoxelData.ChunkSize <= z)
			return (false);

		return (world.blocktypes[ voxelMap [x, y, z]].isSolid);
	}

	void	LoadChunkMesh()
	{
		for (int y = 0; y < VoxelData.ChunkSize; y++)
		{
			for (int x = 0; x < VoxelData.ChunkSize; x++)
			{
				for (int z = 0; z < VoxelData.ChunkSize; z++)
				{
					AddVoxelDataToChunk(new Vector3(x, y, z));
				}
			}
		}
	}

	void	AddVoxelDataToChunk(Vector3 pos)
	{
		byte blockID = voxelMap [(int)pos.x, (int)pos.y, (int)pos.z];

		for (int face = 0; face < 6; face++)
		{
			if (!CheckVoxel(pos + VoxelData.neighbors[face]))
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

	void	CreateMesh()
	{
		mesh.name = "Test Mesh";
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uvs.ToArray();

		mesh.RecalculateNormals();

		meshFilter.mesh = mesh;
	}

	void	AddTexture(int textureID)
	{
		float	y = textureID / VoxelData.TextureAtlasSize;
		float	x = textureID - (y * VoxelData.TextureAtlasSize);

		x *= VoxelData.NormalizedTextureSize;
		y *= VoxelData.NormalizedTextureSize;

		uvs.Add(new Vector2(x, y));
		uvs.Add(new Vector2(x, y + VoxelData.NormalizedTextureSize));
		uvs.Add(new Vector2(x + VoxelData.NormalizedTextureSize, y + VoxelData.NormalizedTextureSize));
		uvs.Add(new Vector2(x + VoxelData.NormalizedTextureSize, y));
	}


		//QUAD METHODS

	void AddQuad (Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
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

		//TRI METHODS

	void AddTriangle (Vector3 v0, Vector3 v1, Vector3 v2)
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
