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

	public float	croutchSpeed = 3f;
	public float	walkSpeed = 6f;
	public float	sprintSpeed = 12f;
	public float	flySpeed = 6f;
	public float	cameraSpeed = 1f;

	public float	jumpForce = 6f;
	public float	gravityForce = -16f;
	public float	maxFallSpeed = -64f;

	public float	playerWidht = 0.3f;		//radius~~
	public float	playerHeight = 1.85f;

	private Vector3	velocity;
	public float	verticalSpeed;			//vertical Momentum

	private Transform	playerCamera;
	private World		world;

	private float	rightward;		//Horizontal	(w s)
	private float	frontward;		//Vertical		(a d)
	private float	mouseHorizontal;
	private float	mouseVertical;

	private void	Start()
	{
		playerCamera = GameObject.Find("Player Camera").transform;
		world = GameObject.Find("World").GetComponent<World>();
	}

	private void	FixedUpdate()
	{
		CalculateVelocity();

		if (isJumping)
			Jump();

		transform.Rotate(Vector3.up * mouseHorizontal * cameraSpeed);		// left/right cam movement
		playerCamera.Rotate(Vector3.right * -mouseVertical * cameraSpeed);	//  up/down   cam movement
		transform.Translate(velocity, Space.World);
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
	
		if (Input.GetButtonDown("Fly"))
			isFlying = true;
		if (Input.GetButtonDown("Alt"))
			isFlying = false;
	}

	private void	Jump()
	{
		if (isGrounded)
		{
			verticalSpeed = jumpForce;
			isGrounded = false;
		}
	}

	private void	CalculateVelocity()
	{
		if (verticalSpeed > -64f)
			verticalSpeed += gravityForce * Time.fixedDeltaTime;
		
		velocity = (transform.forward * frontward) + (transform.right * rightward);

		if (isSprinting)
			velocity *= sprintSpeed;
		else if (isCroutching)
			velocity *= croutchSpeed;
		else
			velocity *= walkSpeed;

		if ((velocity.x > 0 && isRightBlocked) || (velocity.x < 0 && isLeftBlocked))
			velocity.x = 0;
		if ((velocity.z > 0 && isFrontBlocked) || (velocity.z < 0 && isBackBlocked))
			velocity.z = 0;

		if (!isFlying)
			velocity.y += verticalSpeed;
		else if (isJumping)
			velocity.y = flySpeed;
		else if (isCroutching)
			velocity.y = flySpeed;

		velocity *= Time.fixedDeltaTime;
	
		if (velocity.y < 0)
			velocity.y = checkFallSpeed(velocity.y);
		else if (velocity.y > 0)
			velocity.y = checkJumpSpeed(velocity.y);
	}

	private float	checkFallSpeed (float fallSpeed)	//checkDownSpeed
	{
		if (world.CheckForVoxel(transform.position.x + playerWidht,
			transform.position.y + fallSpeed,
			transform.position.z + playerWidht) ||

			world.CheckForVoxel(transform.position.x - playerWidht,
			transform.position.y + fallSpeed,
			transform.position.z + playerWidht) ||

			world.CheckForVoxel(transform.position.x + playerWidht,
			transform.position.y + fallSpeed,
			transform.position.z - playerWidht) ||

			world.CheckForVoxel(transform.position.x - playerWidht, 
			transform.position.y + fallSpeed,
			transform.position.z - playerWidht))
		{
			isGrounded = true;
			return (0);
		}

		isGrounded = false;

		//if (fallSpeed < maxFallSpeed * Time.fixedDeltaTime)
			//return (maxFallSpeed * Time.fixedDeltaTime);
		return (fallSpeed);
	}

	private float	checkJumpSpeed (float jumpSpeed)	//checkDownSpeed
	{
		if (world.CheckForVoxel(transform.position.x + playerWidht,
			transform.position.y + playerHeight + jumpSpeed,
			transform.position.z + playerWidht) ||

			world.CheckForVoxel(transform.position.x - playerWidht,
			transform.position.y + playerHeight + jumpSpeed,
			transform.position.z + playerWidht) ||

			world.CheckForVoxel(transform.position.x + playerWidht,
			transform.position.y + playerHeight + jumpSpeed,
			transform.position.z - playerWidht) ||

			world.CheckForVoxel(transform.position.x - playerWidht, 
			transform.position.y + playerHeight + jumpSpeed,
			transform.position.z - playerWidht))
		{
			return (0);
		}

		return (jumpSpeed);
	}

	public bool	isFrontBlocked
	{
		get
		{
			if (world.CheckForVoxel(transform.position.x,
				transform.position.y, transform.position.z + playerWidht) ||

				world.CheckForVoxel(transform.position.x,
				transform.position.y + 1f, transform.position.z + playerWidht)
				)
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
			if (world.CheckForVoxel(transform.position.x,
				transform.position.y, transform.position.z - playerWidht) ||

				world.CheckForVoxel(transform.position.x,
				transform.position.y + 1f, transform.position.z - playerWidht)
				)
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
			if (world.CheckForVoxel(transform.position.x + playerWidht,
				transform.position.y, transform.position.z) ||

				world.CheckForVoxel(transform.position.x + playerWidht,
				transform.position.y + 1f, transform.position.z)
				)
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
			if (world.CheckForVoxel(transform.position.x - playerWidht,
				transform.position.y, transform.position.z) ||

				world.CheckForVoxel(transform.position.x - playerWidht,
				transform.position.y + 1f, transform.position.z)
				)
			{
				return (true);
			}
			return (false);
		}
	}

}
