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
			noise += (Get2DRecursiveNoise(pos, offset, scale / factor, factor, n - 1) - 0.5f) * (1f / factor);
		return (noise);
	}

	//generalized 3D perlin noise (INVERTED SCALE FACTOR)
	public static float	Get3DNoise(Vector3 pos, Coords offset, float horizontalScale, float verticalScale)
	{
		float	xzScale = WorldData.ChunkSize * horizontalScale * WorldData.noiseScale;
		float	yScale = WorldData.ChunkSize * verticalScale * WorldData.noiseScale;

		float	x = (pos.x + 0.192837465f + (float)((offset.x - 1) * offsetFactor)) / xzScale;
		float	y = (pos.y + 0.192837465f + (float)((offset.y    ) * offsetFactor)) / yScale;
		float	z = (pos.z + 0.192837465f + (float)((offset.z + 1) * offsetFactor)) / xzScale;

		return ((
			Mathf.PerlinNoise(x, y - z) +
			Mathf.PerlinNoise(x, y) +
			Mathf.PerlinNoise(x, z) +
			Mathf.PerlinNoise(x, z - y) +

			//Mathf.PerlinNoise(y, x - z) +
			Mathf.PerlinNoise(y, x) +
			Mathf.PerlinNoise(y, z) +
			//Mathf.PerlinNoise(y, z - x) +

			Mathf.PerlinNoise(z, x - y) +
			Mathf.PerlinNoise(z, x) +
			Mathf.PerlinNoise(z, y) +
			Mathf.PerlinNoise(z, y - x)) / 10f);
	}

	//terrain recursive 3D perlin noise (INVERTED SCALE FACTOR)	//have noise be consitent trough different starting n values
	public static float	Get3DRecursiveNoise(Vector3 pos, Coords offset, float horizontalScale, float verticalScale, float factor, int n)
	{
		Coords	nOffset = new Coords(n, n, n);
		float	noise = Get3DNoise(pos, offset.AddPos(nOffset), horizontalScale * (n + 1), verticalScale * (n + 1));

		if (n > 0)
			noise += (Get3DRecursiveNoise(pos, offset, horizontalScale / factor, verticalScale / factor, factor, n - 1) - 0.5f) * (1f / factor);
		return (noise);
	}

	//for ore and cave noise (INVERTED SCALE FACTOR)
	public static bool	Get3DVeinNoise(Vector3 pos, Coords offset, Vein vein)
	{
		float	noise;
		float	factor = (Mathf.Abs(pos.y - vein.height) / vein.spread);
		float	strenght = 1 - Mathf.Pow(factor, 2);	//^2 or ^3?

		if (WorldData.UseSimpleGen)
			noise = Get3DNoise(pos, offset, vein.horizontalScale, vein.verticalScale);
		else
			noise = Get3DRecursiveNoise(pos, offset, vein.horizontalScale, vein.verticalScale, 2f, vein.n);

		if (0 < strenght && vein.threshold < noise * strenght)
			return (true);
		return (false);
	}
}

