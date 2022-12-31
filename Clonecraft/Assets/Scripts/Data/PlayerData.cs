using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class PlayerData
{
	public static readonly float	crouchSpeed = 3f;
	public static readonly float	walkSpeed = 6f;
	public static readonly float	sprintSpeed = 12f;

	public static readonly float	flyFactor = 3f;
	public static readonly float	flySpeed = 6f;
	public static readonly float	ascentSpeed = 12f;

	public static readonly float	jumpForce = 8f;
	public static readonly float	crouchJumpFactor = 1.25f;

	public static readonly float	gravityForce = -24f;
	public static readonly float 	maxFallSpeed = -120f;

	public static readonly float	playerWidht = 0.32f;		//radius~~
	public static readonly float	playerHeight = 1.84f;

	public static readonly float	reachIncrement = 0.04f;		//checkIncrement
	public static readonly float	reach = 5f;

	public static readonly float	cameraSpeed = 3f;
}
