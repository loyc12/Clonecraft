using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockMod		//VoxelMod
{
	public Coords	worldPos;
	public BlockID	blockID;

	public	BlockMod()
	{
		worldPos = new Coords();
		blockID = BlockID.AIR;
	}
	public	BlockMod(Coords _worldPos)
	{
		worldPos = _worldPos;
		blockID = BlockID.AIR;
	}
	public	BlockMod(Coords _worldPos, BlockID _blockID)
	{
		worldPos = _worldPos;
		blockID = _blockID;
	}
}
