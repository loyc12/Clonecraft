using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class	Noise
{
	public static readonly int	offsetFactor = 1024;

	//generalized 2D perlin noise
	public static float	Get2DNoise(Vector2 pos, float offset, float scale)
	{
		float	mainScale = WorldData.ChunkSize * scale * WorldData.noiseScale;

		return (Mathf.PerlinNoise(
			(pos.x + 0.192837465f + offset) / mainScale,
			(pos.y + 0.192837465f + offset) / mainScale
			));
	}

	//generalized 3D perlin noise (INVERTED SCALE FACTOR)
	public static bool	Get3DNoise(Vector3 pos, float offset, float scale, float threshold)
	{
		float	mainScale = WorldData.ChunkSize * scale * WorldData.noiseScale;

		float	x = (pos.x + 0.192837465f + (offset * offsetFactor)) / mainScale;
		float	y = (pos.y + 0.192837465f + (offset * offsetFactor)) / mainScale;
		float	z = (pos.z + 0.192837465f + (offset * offsetFactor)) / mainScale;

		float	XY = Mathf.PerlinNoise(x, y);
		float	XZ = Mathf.PerlinNoise(x, z);
		float	YZ = Mathf.PerlinNoise(y, z);
		float	ZY = Mathf.PerlinNoise(z, y);
		float	ZX = Mathf.PerlinNoise(z, x);
		float	YX = Mathf.PerlinNoise(y, x);

		if ((XY + XZ + YZ + ZY + ZX + YX) / 6 > threshold)
			return (true);
		return (false);
	}

	//for ore and cave noise (INVERTED SCALE FACTOR)
	public static bool	Get3DVeinNoise(Vector3 pos, Vein vein)
	{
		float	mainScale = WorldData.ChunkSize * vein.scale * WorldData.noiseScale;

		float	x = (pos.x + 0.192837465f + vein.offset) / mainScale;
		float	y = (pos.y + 0.192837465f + vein.offset) / mainScale;
		float	z = (pos.z + 0.192837465f + vein.offset) / mainScale;

		float	XY = Mathf.PerlinNoise(x, y);
		float	XZ = Mathf.PerlinNoise(x, z);
		float	YZ = Mathf.PerlinNoise(y, z);
		//float	ZY = Mathf.PerlinNoise(z, y);
		//float	ZX = Mathf.PerlinNoise(z, x);
		//float	YX = Mathf.PerlinNoise(y, x);

		float	factor = (Mathf.Abs(pos.y - vein.height) / vein.spread);
		float	strenght = 1 - (factor * factor);

		//if (strenght > 0 && ((XY + XZ + YZ + ZY + ZX + YX) / 6) * strenght > vein.threshold)
		if (strenght > 0 && ((XY + XZ + YZ) / 3) * strenght > vein.threshold)
			return (true);
		return (false);
	}
}

