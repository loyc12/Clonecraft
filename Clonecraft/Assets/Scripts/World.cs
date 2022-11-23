using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
	public Material		material;
	public BlockType[]	blocktypes;

	private void	Start()
	{
		Chunk	newChunk = new Chunk(this);
	}
}

[System.Serializable]
public class BlockType
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