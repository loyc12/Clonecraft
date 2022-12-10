using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class	Noise
{
	public static readonly int	offsetFactor = 256;

	//generalized 2D perlin noise (INVERTED SCALE FACTOR)
	public static float	Get2DNoise(Vector2 pos, Coords offset, float scale)
	{
		float	mainScale = WorldData.ChunkSize * scale * WorldData.noiseScale;

		return (Mathf.PerlinNoise(
			(pos.x + 0.192837465f + (float)((offset.x - 1) * offsetFactor)) / mainScale,
			(pos.y + 0.192837465f + (float)((offset.y + 1) * offsetFactor)) / mainScale
			));
	}

	//terrain recursive 2D perlin noise (INVERTED SCALE FACTOR)
	public static float	Get2DRecursiveNoise(Vector2 pos, Coords offset, float scale, float factor, int n)
	{
		Coords	nOffset = new Coords(n, n, n);
		float	noise = Get2DNoise(pos, offset.AddPos(nOffset), scale * (n + 1));

		if (n > 0)
			noise += (0.5f - Get2DRecursiveNoise(pos, offset, scale / factor, factor, n - 1)) * (1f / factor);
		return (noise);
	}

	//generalized 3D perlin noise (INVERTED SCALE FACTOR)
	public static float	Get3DNoise(Vector3 pos, Coords offset, float scale)
	{
		float	mainScale = WorldData.ChunkSize * scale * WorldData.noiseScale;

		float	x = (pos.x + 0.192837465f + (float)((offset.x - 1) * offsetFactor)) / mainScale;
		float	y = (pos.y + 0.192837465f + (float)((offset.y    ) * offsetFactor)) / mainScale;
		float	z = (pos.z + 0.192837465f + (float)((offset.z + 1) * offsetFactor)) / mainScale;

		float	XZ = Mathf.PerlinNoise(x, z);
		float	ZX = Mathf.PerlinNoise(z, x);

		float	YXZ = Mathf.PerlinNoise(y, x + z);
		float	ZXY = Mathf.PerlinNoise(z + x, y);

		float	WXZ = Mathf.PerlinNoise(y, x - z);
		float	ZXW = Mathf.PerlinNoise(z - x, y);

		return ((XZ + ZX + YXZ + ZXY + WXZ + ZXW) / 6f);
	}

	//terrain recursive 3D perlin noise (INVERTED SCALE FACTOR)
	public static float	Get3DRecursiveNoise(Vector3 pos, Coords offset, float scale, float factor, int n)
	{
		Coords	nOffset = new Coords(n, n, n);
		float	noise = Get3DNoise(pos, offset.AddPos(nOffset), scale * (n + 1));

		if (n > 0)
			noise += (0.5f - Get3DRecursiveNoise(pos, offset, scale / factor, factor, n - 1)) * (1f / factor);
		return (noise);
	}

	//for ore and cave noise (INVERTED SCALE FACTOR)
	public static bool	Get3DVeinNoise(Vector3 pos, Coords offset, Vein vein)
	{
		float	noise = Get3DRecursiveNoise(pos, offset, vein.scale, 1.5f, 2);

		float	factor = (Mathf.Abs(pos.y - vein.height) / vein.spread);
		float	strenght = 1 - Mathf.Pow(factor, 2);

		if (0 < strenght && vein.threshold < noise * strenght)
			return (true);
		return (false);
	}
}

