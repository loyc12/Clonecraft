using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*	IDEAS

	make crouch crouchier

*/

public class Player : MonoBehaviour
{
	private bool	isSprinting;
	private bool	isCrouching;
	private bool	isGrounded;
	private bool	isJumping;	//jumpRequest
	private bool	isFlying;
	private bool	isGhosting;

	private Transform		playerCam;				//cam

	private float			mouseHorizontal;
	private float			yRotation;

	private float			mouseVertical;
	private float			xRotation;

	private float			mouseScroll;			//Scroll

	private Vector3			velocity;
	private float			verticalSpeed;			//vertical Momentum

	private float			horizontal;				//(w s)
	private float			vertical;				//(a d)

	public Transform		placeBlock;
	public Transform		breakBlock;				//highlightBlock

	public Text				selectedBlockText;
	public BlockID			selectedBlockID;

	private World			world;

	private void	Start()
	{
		playerCam = GameObject.Find("Camera").transform;
		world = GameObject.Find("World").GetComponent<World>();

		Cursor.lockState = CursorLockMode.Locked;

		UpdatedSelectedBlockID(PlayerData.defaultBlock);
	}

	private void	FixedUpdate()
	{
		CalculateVelocity();

		if (isJumping)
			Jump();

		transform.Translate(velocity, Space.World);
	}

	private void	Update()
	{
		FindTargetedBlocks();
		GetPlayerInputs();
		TurnCamera();
	}

	private void	GetPlayerInputs()
	{
		mouseVertical = Input.GetAxis("Mouse Y");
		mouseHorizontal = Input.GetAxis("Mouse X");

		vertical = Input.GetAxis("Vertical");
		horizontal = Input.GetAxis("Horizontal");

		if (Input.GetButtonDown("Sprint"))
			isSprinting = true;
		else if (Input.GetButtonUp("Sprint"))
			isSprinting = false;

		if (Input.GetButtonDown("Crouch"))
			isCrouching = true;
		else if (Input.GetButtonUp("Crouch"))
			isCrouching = false;

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

		mouseScroll = Input.GetAxis("Mouse ScrollWheel");
		if (mouseScroll != 0)
			ChangeSelectedBlock(BlockID.AIR);

		if (breakBlock.gameObject.activeSelf)
			BlockAction();
	}

	private void	FindTargetedBlocks()	//doesn't entirely prevent placing a block that clips
	{
		Vector3	firstPos = new Vector3();
		Vector3	secondPos = new Vector3();
		//Vector3	thirdPos = new Vector3();

		float	step = 0;

		firstPos = playerCam.position;

		while (step <= PlayerData.reach)
		{
			secondPos = firstPos;
			firstPos = playerCam.position + (playerCam.forward * step);

			if (BlockID.AIR < world.FindBlockID(new Coords(firstPos)))					//optimize me
			{
				breakBlock.position = new Vector3(
					Mathf.FloorToInt(firstPos.x),
					Mathf.FloorToInt(firstPos.y),
					Mathf.FloorToInt(firstPos.z));

				breakBlock.gameObject.SetActive(true);

				Coords	rPos = new Coords(secondPos);
				Coords	camPos = new Coords(playerCam.position);

				if ((rPos.y != camPos.y && rPos.y != (camPos.y - 1)) || rPos.x != camPos.x || rPos.z != camPos.z)
				{
					placeBlock.position = new Vector3(
						Mathf.FloorToInt(secondPos.x),
						Mathf.FloorToInt(secondPos.y),
						Mathf.FloorToInt(secondPos.z));

					placeBlock.gameObject.SetActive(true);
				}
				else
					placeBlock.gameObject.SetActive(false);

				return ;
			}

			step += PlayerData.reachIncrement;
		}

		breakBlock.gameObject.SetActive(false);
		placeBlock.gameObject.SetActive(false);
	}

	private void	ChangeSelectedBlock(BlockID value)
	{
		if (value == BlockID.AIR)
		{
			if (mouseScroll > 0)
				selectedBlockID++;
			else
				selectedBlockID--;

			BlockID maxID = BlockType.maxID;

			if (selectedBlockID > maxID - 1)
				selectedBlockID = BlockID.GRASS;
			if (selectedBlockID < BlockID.GRASS)
				selectedBlockID = maxID - 1;
		}
		else
			selectedBlockID = value;

		selectedBlockText.text = world.blocktypes[(int)selectedBlockID].blockName + " block selected";
	}

	private void	UpdatedSelectedBlockID(BlockID value)
	{
		selectedBlockID = value;
		selectedBlockText.text = world.blocktypes[(int)selectedBlockID].blockName + " block selected";
	}

	private void	BlockAction()
	{
		if (Input.GetMouseButtonDown(0))	//break block
		{
			Coords worldPos = new Coords(breakBlock.position);
			Coords chunkPos = worldPos.WorldToChunkPos();

			if (worldPos.IsBlockInWorld())
				world.FindChunk(chunkPos).SetBlockID(worldPos, BlockID.AIR);
		}
		if (Input.GetMouseButtonDown(1))	//place block
		{
			Coords worldPos = new Coords(placeBlock.position);
			Coords chunkPos = worldPos.WorldToChunkPos();

			if (worldPos.IsBlockInWorld())
				world.FindChunk(chunkPos).SetBlockID(worldPos, selectedBlockID);
		}
		if (Input.GetMouseButtonDown(2))	//copy block
		{
			Coords worldPos = new Coords(breakBlock.position);
			Coords chunkPos = worldPos.WorldToChunkPos();

			if (worldPos.IsBlockInWorld())
				UpdatedSelectedBlockID(world.FindChunk(chunkPos).FindBlockID(worldPos));
		}
	}

	private void	CalculateVelocity()
	{
		//gravity implementation
		if (isGrounded || isFlying)
			verticalSpeed *= 0;
		if (verticalSpeed > PlayerData.maxFallSpeed)
			verticalSpeed += PlayerData.gravityForce * Time.fixedDeltaTime;

		//horizontal movements controls
		velocity = (transform.forward * vertical) + (transform.right * horizontal);

		//speed controls
		if (isCrouching)
			velocity *= PlayerData.crouchSpeed;
		else if (isSprinting)
			velocity *= PlayerData.sprintSpeed;
		else
			velocity *= PlayerData.walkSpeed;

		//checks for sideways obstacles
		if (!isGhosting && ((velocity.x > 0 && isRightBlocked) || (velocity.x < 0 && isLeftBlocked)))
			velocity.x = 0;
		if (!isGhosting && ((velocity.z > 0 && isFrontBlocked) || (velocity.z < 0 && isBackBlocked)))
			velocity.z = 0;

		//fly controls
		if (isFlying)
		{
			velocity *= PlayerData.flySpeed;

			if (isJumping && isCrouching)
				velocity.y = 0;
			else if (isJumping)
				velocity.y = PlayerData.ascentSpeed;
			else if (isCrouching)
				velocity.y = -PlayerData.ascentSpeed;
			if (isSprinting)
				velocity.y *= PlayerData.sprintSpeed / 3f;
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
		mouseHorizontal *= PlayerData.cameraSpeed * Time.deltaTime * 100f;
		yRotation += mouseHorizontal;

		mouseVertical *= PlayerData.cameraSpeed * Time.deltaTime * 100f;
		xRotation -= mouseVertical;
		xRotation = Mathf.Clamp(xRotation, -89f, 89f);

		playerCam.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);		//moves the head around
		transform.rotation = Quaternion.Euler(0f, yRotation, 0f);						//turns the body left/right
	}

	private void	Jump()
	{
		if (isGrounded)
		{
			verticalSpeed = PlayerData.jumpForce;
			if (isCrouching)
				verticalSpeed *= PlayerData.crouchJumpFactor;
			isGrounded = false;
		}
	}

	private float	checkFallSpeed (float fallSpeed)	//checkDownSpeed
	{
		if (world.IsBlockSolid(new Coords(
				transform.position.x + PlayerData.playerWidht,
				transform.position.y + fallSpeed,
				transform.position.z + PlayerData.playerWidht))
			||
			world.IsBlockSolid(new Coords(
				transform.position.x - PlayerData.playerWidht,
				transform.position.y + fallSpeed,
				transform.position.z + PlayerData.playerWidht))
			||
			world.IsBlockSolid(new Coords(
				transform.position.x + PlayerData.playerWidht,
				transform.position.y + fallSpeed,
				transform.position.z - PlayerData.playerWidht))
			||
			world.IsBlockSolid(new Coords(
				transform.position.x - PlayerData.playerWidht,
				transform.position.y + fallSpeed,
				transform.position.z - PlayerData.playerWidht)))
		{
			isGrounded = true;
			return (0);
		}

		isGrounded = false;

		return (fallSpeed);
	}

	private float	checkJumpSpeed (float jumpSpeed)	//checkUpSpeed
	{
		if (world.IsBlockSolid(new Coords(
				transform.position.x + PlayerData.playerWidht,
				transform.position.y + PlayerData.playerHeight + jumpSpeed,
				transform.position.z + PlayerData.playerWidht))
			||
			world.IsBlockSolid(new Coords(
				transform.position.x - PlayerData.playerWidht,
				transform.position.y + PlayerData.playerHeight + jumpSpeed,
				transform.position.z + PlayerData.playerWidht))
			||
			world.IsBlockSolid(new Coords(
				transform.position.x + PlayerData.playerWidht,
				transform.position.y + PlayerData.playerHeight + jumpSpeed,
				transform.position.z - PlayerData.playerWidht))
			||
			world.IsBlockSolid(new Coords(
				transform.position.x - PlayerData.playerWidht,
				transform.position.y + PlayerData.playerHeight + jumpSpeed,
				transform.position.z - PlayerData.playerWidht)))
		{
			return (0);
		}

		return (jumpSpeed);
	}

	public bool	isFrontBlocked
	{
		get
		{
			if (world.IsBlockSolid(new Coords(
					transform.position.x,
					transform.position.y,
					transform.position.z + PlayerData.playerWidht))
				||
				world.IsBlockSolid(new Coords(
					transform.position.x,
					transform.position.y + (PlayerData.playerHeight / 2),
					transform.position.z + PlayerData.playerWidht))
				||
				world.IsBlockSolid(new Coords(
					transform.position.x,
					transform.position.y + PlayerData.playerHeight,
					transform.position.z + PlayerData.playerWidht)))
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
			if (world.IsBlockSolid(new Coords(
					transform.position.x,
					transform.position.y,
					transform.position.z - PlayerData.playerWidht))
				||
				world.IsBlockSolid(new Coords(
					transform.position.x,
					transform.position.y + (PlayerData.playerHeight / 2),
					transform.position.z - PlayerData.playerWidht))
				||
				world.IsBlockSolid(new Coords(
					transform.position.x,
					transform.position.y + PlayerData.playerHeight,
					transform.position.z - PlayerData.playerWidht)))
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
			if (world.IsBlockSolid(new Coords(
					transform.position.x + PlayerData.playerWidht,
					transform.position.y, transform.position.z))
				||
				world.IsBlockSolid(new Coords(
					transform.position.x + PlayerData.playerWidht,
					transform.position.y + (PlayerData.playerHeight / 2),
					transform.position.z))
				||
				world.IsBlockSolid(new Coords(
					transform.position.x + PlayerData.playerWidht,
					transform.position.y + PlayerData.playerHeight,
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
			if (world.IsBlockSolid(new Coords(
					transform.position.x - PlayerData.playerWidht,
					transform.position.y,
					transform.position.z))
				||
				world.IsBlockSolid(new Coords(
					transform.position.x - PlayerData.playerWidht,
					transform.position.y + (PlayerData.playerHeight / 2),
					transform.position.z))
				||
				world.IsBlockSolid(new Coords(
					transform.position.x - PlayerData.playerWidht,
					transform.position.y + PlayerData.playerHeight,
					transform.position.z)))
			{
				return (true);
			}
			return (false);
		}
	}
}
