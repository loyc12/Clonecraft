using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class PlayerData
{
	public static readonly float	walkSpeed = 12f;
	public static readonly float	crouchSpeedFactor = 0.42f;
	public static readonly float	sprintSpeedFactor = 1.618f;

	public static readonly float	flySpeedFactor = 2f;
	public static readonly float	flySprintFactor = 2f;
	public static readonly float	flyAscentFactor = 32f;

	public static readonly float	jumpForce = 18f;
	public static readonly float	crouchJumpFactor = 1.28f;

	public static readonly float	gravityForce = -64f;
	public static readonly float 	maxFallSpeed = -256f;

	public static readonly float	playerWidht = 0.8f;			//0.32f;		//radius~~
	public static readonly float	playerHeight = 4.5f;		//1.84f;

	public static readonly float	reachIncrement = 0.04f;		//checkIncrement
	public static readonly float	reach = 12f;

	public static readonly float	cameraSpeed = 3f;
}
