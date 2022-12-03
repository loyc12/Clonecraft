using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour
{
	private bool	isSprinting;
	private bool	isCroutching;
	private bool	isGrounded;
	private bool	isJumping;	//jumpRequest
	private bool	isFlying;
	private bool	isGhosting;

	public readonly float	croutchSpeed = 3f;
	public readonly float	walkSpeed = 6f;
	public readonly float	sprintSpeed = 12f;

	public readonly float	flySpeed = 8f;
	public readonly float	ascentSpeed = 12f;

	public readonly float	jumpForce = 8f;
	public readonly float	gravityForce = -24f;
	public readonly float 	maxFallSpeed = -120f;

	public readonly float	playerWidht = 0.32f;		//radius~~
	public readonly float	playerHeight = 1.85f;

	public readonly float	cameraSpeed = 4f;
	private Transform		playerCamera;
	private float			verticalRotation;

	private float			mouseHorizontal;
	private float			mouseVertical;

	private Vector3			velocity;
	private float			verticalSpeed;			//vertical Momentum

	private float			rightward;				//Horizontal	(w s)
	private float			frontward;				//Vertical		(a d)

	private World			world;

	private void	Start()
	{
		playerCamera = GameObject.Find("Player Camera").transform;
		world = GameObject.Find("World").GetComponent<World>();

		verticalRotation = 0;
	}

	private void	FixedUpdate()
	{
		CalculateVelocity();

		if (isJumping)
			Jump();

		transform.Translate(velocity, Space.World);
		TurnCamera();
	}

	private void	Update()
	{
		GetPlayerInputs();
	}

	private void	GetPlayerInputs()
	{
		rightward = Input.GetAxis("Horizontal");	//in project settings
		frontward = Input.GetAxis("Vertical");
		mouseHorizontal = Input.GetAxis("Mouse X");
		mouseVertical = Input.GetAxis("Mouse Y");

		if (Input.GetButtonDown("Sprint"))
			isSprinting = true;
		else if (Input.GetButtonUp("Sprint"))
			isSprinting = false;

		if (Input.GetButtonDown("Croutch"))
			isCroutching = true;
		else if (Input.GetButtonUp("Croutch"))
			isCroutching = false;

		if (Input.GetButtonDown("Jump"))
			isJumping = true;
		else if (Input.GetButtonUp("Jump"))
			isJumping = false;

		if (Input.GetButtonDown("Ghost"))
			isGhosting = !isGhosting;
	
		if (Input.GetButtonDown("Fly"))
			isFlying = !isFlying;

		if (Input.GetButtonDown("TP"))
			transform.Translate((Vector3.up * WorldData.ChunkSize), Space.World);
	}

	private void	CalculateVelocity()
	{
		//gravity implementation
		if (isGrounded || isFlying)
			verticalSpeed *= 0;
		if (verticalSpeed > maxFallSpeed)
			verticalSpeed += gravityForce * Time.fixedDeltaTime;
		
		//horizontal movements controls
		velocity = (transform.forward * frontward) + (transform.right * rightward);

		//speed controls
		if (isCroutching)
			velocity *= croutchSpeed;
		else if (isSprinting)
			velocity *= sprintSpeed;
		else
			velocity *= walkSpeed;

		//checks for sideways obstacles
		if (!isGhosting && ((velocity.x > 0 && isRightBlocked) || (velocity.x < 0 && isLeftBlocked)))
			velocity.x = 0;
		if (!isGhosting && ((velocity.z > 0 && isFrontBlocked) || (velocity.z < 0 && isBackBlocked)))
			velocity.z = 0;

		//fly controls
		if (isFlying)
		{
			velocity *= flySpeed;

			if (isJumping && isCroutching)
				velocity.y = 0;
			else if (isJumping)
				velocity.y = ascentSpeed;
			else if (isCroutching)
				velocity.y = -ascentSpeed;
			if (isSprinting)
				velocity.y *= sprintSpeed / 3f;
		}
		else
			velocity.y += verticalSpeed;

		//makes movement speed proporional to time
		velocity *= Time.fixedDeltaTime;
	
		//checks for vertical obstacles
		if (!isGhosting && velocity.y < 0 )
			velocity.y = checkFallSpeed(velocity.y);
		else if (!isGhosting && velocity.y > 0)
			velocity.y = checkJumpSpeed(velocity.y);
	}

	private void TurnCamera()
	{
		transform.Rotate(Vector3.up * mouseHorizontal * cameraSpeed);					// left/right cam movement

		//playerCamera.transform.Rotate(Vector3.right * -mouseVertical * cameraSpeed);	//  up/down   cam movement (unbounded)

		verticalRotation += -mouseVertical * cameraSpeed;

		//clamping camera
		if (verticalRotation > 90f)
			verticalRotation = 90f;
		else if (verticalRotation < -90f)
			verticalRotation = -90f;
	
		playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);	//  up/down   cam movement
	}

	private void	Jump()
	{
		if (isGrounded)
		{
			verticalSpeed = jumpForce;
			if (isCroutching)
				verticalSpeed *= 0.5f;			//REMOVE ???
			isGrounded = false;
		}
	}

	private float	checkFallSpeed (float fallSpeed)	//checkDownSpeed
	{
		if (world.CheckForSolidity(new Coords(
				transform.position.x + playerWidht,
				transform.position.y + fallSpeed,
				transform.position.z + playerWidht)) ||

			world.CheckForSolidity(new Coords(
				transform.position.x - playerWidht,
				transform.position.y + fallSpeed,
				transform.position.z + playerWidht)) ||

			world.CheckForSolidity(new Coords(
				transform.position.x + playerWidht,
				transform.position.y + fallSpeed,
				transform.position.z - playerWidht)) ||

			world.CheckForSolidity(new Coords(
				transform.position.x - playerWidht, 
				transform.position.y + fallSpeed,
				transform.position.z - playerWidht)))
		{
			isGrounded = true;
			return (0);
		}

		isGrounded = false;

		return (fallSpeed);
	}

	private float	checkJumpSpeed (float jumpSpeed)	//checkUpSpeed
	{
		if (world.CheckForSolidity(new Coords(
				transform.position.x + playerWidht,
				transform.position.y + playerHeight + jumpSpeed,
				transform.position.z + playerWidht)) ||

			world.CheckForSolidity(new Coords(
				transform.position.x - playerWidht,
				transform.position.y + playerHeight + jumpSpeed,
				transform.position.z + playerWidht)) ||

			world.CheckForSolidity(new Coords(
				transform.position.x + playerWidht,
				transform.position.y + playerHeight + jumpSpeed,
				transform.position.z - playerWidht)) ||

			world.CheckForSolidity(new Coords(
				transform.position.x - playerWidht, 
				transform.position.y + playerHeight + jumpSpeed,
				transform.position.z - playerWidht)))
		{
			return (0);
		}

		return (jumpSpeed);
	}

	public bool	isFrontBlocked
	{
		get
		{
			if (world.CheckForSolidity(new Coords(
					transform.position.x,
					transform.position.y,
					transform.position.z + playerWidht))
				||
				world.CheckForSolidity(new Coords(
					transform.position.x,
					transform.position.y + (playerHeight / 2),
					transform.position.z + playerWidht))
				||
				world.CheckForSolidity(new Coords(
					transform.position.x,
					transform.position.y + playerHeight,
					transform.position.z + playerWidht)))
			{
				return (true);
			}
			return (false);
		}
	}

	public bool	isBackBlocked
	{
		get
		{
			if (world.CheckForSolidity(new Coords(
					transform.position.x,
					transform.position.y,
					transform.position.z - playerWidht))
				||
				world.CheckForSolidity(new Coords(
					transform.position.x,
					transform.position.y + (playerHeight / 2),
					transform.position.z - playerWidht))
				||
				world.CheckForSolidity(new Coords(
					transform.position.x,
					transform.position.y + playerHeight,
					transform.position.z - playerWidht)))
			{
				return (true);
			}
			return (false);
		}
	}

	public bool	isRightBlocked
	{
		get
		{
			if (world.CheckForSolidity(new Coords(
					transform.position.x + playerWidht,
					transform.position.y, transform.position.z))
				||
				world.CheckForSolidity(new Coords(
					transform.position.x + playerWidht,
					transform.position.y + (playerHeight / 2),
					transform.position.z))
				||
				world.CheckForSolidity(new Coords(
					transform.position.x + playerWidht,
					transform.position.y + playerHeight,
					transform.position.z)))
			{
				return (true);
			}
			return (false);
		}
	}

	public bool	isLeftBlocked
	{
		get
		{
			if (world.CheckForSolidity(new Coords(
					transform.position.x - playerWidht,
					transform.position.y,
					transform.position.z))
				||
				world.CheckForSolidity(new Coords(
					transform.position.x - playerWidht,
					transform.position.y + (playerHeight / 2),
					transform.position.z))
				||
				world.CheckForSolidity(new Coords(
					transform.position.x - playerWidht,
					transform.position.y + playerHeight,
					transform.position.z)))
			{
				return (true);
			}
			return (false);
		}
	}
}
