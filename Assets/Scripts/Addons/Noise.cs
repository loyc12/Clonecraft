using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*	IDEAS

implement spagetti noise (true when close to threshold value) for caves, veins and rivers
implement biome noise (humidity/temperature/elevation/weirdness)

*/

//handles all the noise related stuff (2D, 3D and specialized)
public static class	Noise
{
	public static readonly int	offsetFactor = 256;

	private static	float GetNoise(World world, float a, float b)
	{
		float	c = 0.192837465f;

		return Mathf.PerlinNoise(a + c, b + c);
	}

	// ===== 2D noise =====

	//generalized noise
	public static float	Get2DNoise(World world, Vector2 pos, Coords offset, float scale)
	{
		float	mainScale = WorldData.ChunkSize * scale * WorldData.noiseScale;

		return (GetNoise(world,
			(pos.x + ((float)(offset.x - 1) * offsetFactor)) / mainScale,
			(pos.y + ((float)(offset.y + 1) * offsetFactor)) / mainScale
			));
	}

	//recursive noise (additive)
	public static float	Get2DRecursiveNoise(World world, Vector2 pos, Coords offset, float scale, float factor, int n)
	{
		Coords	nOffset = new Coords(n, n, n);
		float	noise = Get2DNoise(world, pos, offset.AddPos(nOffset), scale);

		if (n > 0)
			noise += (Get2DRecursiveNoise(world, pos, offset, scale / factor, factor, n - 1) - 0.5f) * (1f / factor);
		return (noise);
	}

	//compounded noise (multiplicative)
	public static float	Get2DCompoundedNoise(World world, Vector2 pos, Coords offset, float scale, float factor, int n)
	{
		Coords	nOffset = new Coords(n, n, n);
		float	noise = Get2DNoise(world, pos, offset.AddPos(nOffset), scale);

		if (n > 0)
			noise *= Get2DCompoundedNoise(world, pos, offset, scale / factor, factor, n - 1);
		return (noise);
	}

	// ===== 3D noise =====

	//generalized noise
	public static float	Get3DNoise(World world, Vector3 pos, Coords offset, float horizontalScale, float verticalScale)
	{
		float	xzScale = WorldData.ChunkSize * horizontalScale * WorldData.noiseScale;
		float	yScale = WorldData.ChunkSize * verticalScale * WorldData.noiseScale;

		float	x = (pos.x + (float)((offset.x - 1) * offsetFactor)) / xzScale;
		float	y = (pos.y + (float)((offset.y    ) * offsetFactor)) / yScale;
		float	z = (pos.z + (float)((offset.z + 1) * offsetFactor)) / xzScale;

		return ((
			GetNoise(world, x, z + y) +
			GetNoise(world, x, z - y) +

			GetNoise(world, z, x + y) +
			GetNoise(world, z, x - y)) / 4f); //+

			//Get2DSimplexNoise(world, y, y)) / 5f);	//removing y viable
	}

	//recursive noise (additive)
	public static float	Get3DRecursiveNoise(World world, Vector3 pos, Coords offset, float horizontalScale, float verticalScale, float factor, int n)
	{
		Coords	nOffset = new Coords(n, n, n);
		float	noise = Get3DNoise(world, pos, offset.AddPos(nOffset), horizontalScale, verticalScale);

		if (n > 0)
			noise += (Get3DRecursiveNoise(world, pos, offset, horizontalScale / factor, verticalScale / factor, factor, n - 1) - 0.5f) * (1f / factor);
		return (noise);
	}

	//compounded noise (multiplicative)
	public static float	Get3DCompoundedNoise(World world, Vector3 pos, Coords offset, float horizontalScale, float verticalScale, float factor, int n)
	{
		Coords	nOffset = new Coords(n, n, n);
		float	noise = Get3DNoise(world, pos, offset.AddPos(nOffset), horizontalScale, verticalScale);

		if (n > 0)
			noise *= Get3DCompoundedNoise(world, pos, offset, horizontalScale / factor, verticalScale / factor, factor, n - 1);
		return (noise);
	}

	// ===== applied noise =====

	//cheese noise (for caves and blobs)
	public static bool	Get3DVeinNoise(World world, Vector3 pos, Coords offset, Vein vein)
	{
		float	noise;
		float	factor = Mathf.Abs(pos.y - vein.height) / vein.spread;
		float	strenght = 1 - Mathf.Pow(factor, 2);	//^2 or ^3?

		if (world.UseSimpleGen)
			noise = Get3DNoise(world, pos, offset, vein.horizontalScale, vein.verticalScale);
		else
			noise = Get3DRecursiveNoise(world, pos, offset, vein.horizontalScale, vein.verticalScale, 1.618f, vein.n);

		float	value = noise * strenght;


		if (0 < value)
		{
			if ((!vein.invertThreshold && vein.bottomThreshold < value && value < vein.topThreshold)
			||
			(vein.invertThreshold && (value < vein.bottomThreshold || vein.topThreshold < value)))
			{
				return (true);
			}
		}
		return (false);
	}
}

