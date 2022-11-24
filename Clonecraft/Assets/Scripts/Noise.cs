using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class	Noise
{
	public static float	Get2DNoise (Vector2 pos, float offset, float scale)
	{
		return (Mathf.PerlinNoise(
			((pos.x + 0.192837465f) / WorldData.ChunkSize * scale) + offset,
			((pos.y + 0.192837465f) / WorldData.ChunkSize * scale) + offset
		));
	}
}

