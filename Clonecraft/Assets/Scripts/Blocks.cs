using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockID : byte
{
	AIR		= 0,
	GRASS	= 1,
	DIRT	= 2,
	STONE	= 3,
	ROCK	= 4,
	BEDROCK	= 5,
	MARBLE	= 6,
	SAND	= 7,
	GRAVEL	= 8
}

[System.Serializable]
public class	BlockType
{
	public string 			blockName;
	public bool				isOpaque;				//isSolid
	public bool				isSolid;				//isColidable
	public bool				isMonofaced;
	public static BlockID	maxID = (BlockID)9;		//DE-HARDCODE ME

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