using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockMod		//VoxelMod
{
	public Coords	worldPos;
	public BlockID	blockID;
	public bool		forcePlace;

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
	public	BlockMod(Coords _worldPos, BlockID _blockID, bool _forcePlace)
	{
		worldPos = _worldPos;
		blockID = _blockID;
		forcePlace = _forcePlace;
	}
}
