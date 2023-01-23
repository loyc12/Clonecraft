using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockID : byte
{
	AIR		= 0,
	GRASS	= 1,
	DIRT	= 2,
	SLATE	= 3,
	GRANITE = 4,
	ROCK	= 5,
	STONE	= 6,
	MARBLE	= 7,
	SAND	= 8,
	GRAVEL	= 9,
	SNOW	= 10,
	WATER	= 11,
	LAVA	= 12,
	ACID	= 13,
	GLASS	= 14,
	OAKLOG	= 15,
	OAKLEAVES = 16,
	OAKPLANKS = 17
}

[System.Serializable]
public class	BlockType
{
	public string 			blockName;

	public bool				isSolid;
	public bool				isOpaque;				//!isTransparent
	public bool				isMonofaced;

	public static BlockID	maxID = (BlockID)17;		// Max Index --- DE-HARDCODE ME

	[Header("Textures")]
	public Sprite	icon;
	public int		topFaceTexture;
	public int		bottomFaceTexture;
	public int		frontFaceTexture;
	public int		backFaceTexture;
	public int		leftFaceTexture;
	public int		rightFaceTexture;

	public int		GetTextureId(int faceIndex)
	{
		if (isMonofaced)
			return topFaceTexture;
		switch (faceIndex)
		{
			case 0:
				return (topFaceTexture);
			case 1:
				return (bottomFaceTexture);
			case 2:
				return (frontFaceTexture);
			case 3:
				return (backFaceTexture);
			case 4:
				return (leftFaceTexture);
			case 5:
				return (rightFaceTexture);
			default:
				Debug.Log("Error in BlockType.GetTextureID() : invalide face index given");
				return (0);
		}
	}
}