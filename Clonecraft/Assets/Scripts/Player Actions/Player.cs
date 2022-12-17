using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*	IDEAS

	make crouch crouchier

*/

public class Player : MonoBehaviour
{
	private bool		isSprinting;
	private bool		isCrouching;
	private bool		isGrounded;
	private bool		isJumping;	//jumpRequest
	private bool		isFlying;
	private bool		isGhosting;
	private bool		hasMoved;

	private bool		getNextBlock;
	private bool		autoPlaceBlock;

	private Transform	playerCam;				//cam
	private Transform	playerFeet;				//body

	private float		mouseHorizontal;
	public float		yRotation;

	private float		mouseVertical;
	public float		xRotation;

	private float		mouseScroll;			//Scroll

	private Vector3		velocity;
	private float		verticalSpeed;			//vertical Momentum

	private float		horizontal;				//(w s)
	private float		vertical;				//(a d)

	public Coords		playerPos;
	public Coords		playerLastPos;
	public Coords		playerChunkPos;

	public Transform	placeBlock;
	public Transform	breakBlock;				//highlightBlock

	public Text			selectedBlockText;
	public BlockID		selectedBlockID;

	private World		world;



	private void	Start()
	{
		playerFeet = GameObject.Find("Player").transform;
		playerCam = GameObject.Find("Camera").transform;
		world = GameObject.Find("World").GetComponent<World>();

		playerPos = new Coords(playerFeet.position);
		playerLastPos = playerPos;

		Cursor.lockState = CursorLockMode.Locked;

		UpdateSelectedBlockID(PlayerData.defaultBlock);
	}

	private void	FixedUpdate()
	{
		CalculatePosition();
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

		if (Input.GetButtonDown("F"))
			getNextBlock = !getNextBlock;

		if (Input.GetButtonDown("Alt"))
			autoPlaceBlock = !autoPlaceBlock;

		if (Input.GetButtonDown("Fly"))
			isFlying = !isFlying;

		if (Input.GetButtonDown("TP"))
			transform.Translate((Vector3.up * WorldData.ChunkSize), Space.World);

		mouseScroll = Input.GetAxis("Mouse ScrollWheel");
		//if (mouseScroll != 0)
			//ChangeSelectedBlock(BlockID.AIR);

		if (breakBlock.gameObject.activeSelf)
			BlockAction();
	}

	private void	FindTargetedBlocks()	//doesn't entirely prevent placing a block that clips
	{
		Vector3	lastPos = new Vector3();
		Vector3	currentPos = new Vector3();
		Vector3	nextPos = new Vector3();

		float	step = 0;

		//lastPos = playerCam.position;		//redundant
		currentPos = playerCam.position;
		nextPos = playerCam.position;

		Coords	camPos = new Coords(playerCam.position);

		while (step <= PlayerData.reach)
		{
			lastPos = currentPos;
			currentPos = nextPos;
			nextPos = playerCam.position + (playerCam.forward * step);

			Coords	lPos = new Coords(lastPos);
			Coords	cPos = new Coords(currentPos);
			Coords	nPos = new Coords(nextPos);

			if (BlockID.AIR < world.FindBlockID(cPos))					//optimize me
			{
				breakBlock.position = new Vector3(
					Mathf.FloorToInt(currentPos.x),
					Mathf.FloorToInt(currentPos.y),
					Mathf.FloorToInt(currentPos.z));

				breakBlock.gameObject.SetActive(true);

				if (getNextBlock)
				{
					while (step <= PlayerData.reach && (nPos.SamePosAs(cPos)))
					{
						step += PlayerData.reachIncrement;
						nextPos = playerCam.position + (playerCam.forward * step);
						nPos = new Coords(nextPos);
					}

					if (BlockID.AIR == world.FindBlockID(nPos))
					{
						placeBlock.position = new Vector3(
							Mathf.FloorToInt(nextPos.x),
							Mathf.FloorToInt(nextPos.y),
							Mathf.FloorToInt(nextPos.z));

						placeBlock.gameObject.SetActive(true);

						return ;
					}
					else
						placeBlock.gameObject.SetActive(false);
				}
				if (lPos.x != camPos.x || lPos.z != camPos.z || (lPos.y != camPos.y && lPos.y != camPos.y - 1))
				{
					placeBlock.position = new Vector3(
						Mathf.FloorToInt(lastPos.x),
						Mathf.FloorToInt(lastPos.y),
						Mathf.FloorToInt(lastPos.z));

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
/*
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
			selectedBlockText.text = world.blocktypes[(int)selectedBlockID].blockName + " block selected";
		}
		else
			UpdateSelectedBlockID(value);
	}
*/
	private void	BlockAction()			//do action again if key held and player moved(?)
	{
		Coords	worldBreakPos = new Coords(breakBlock.position);
		Coords	worldPlacePos = new Coords(placeBlock.position);

		if (Input.GetMouseButtonDown(0) || (Input.GetMouseButton(0) && hasMoved && autoPlaceBlock))	//break block
		{
			if (breakBlock.gameObject.activeSelf)
			{
				Coords	chunkPos = worldBreakPos.WorldToChunkPos();
				Chunk	targetChunk = world.FindChunk(chunkPos);

				if (worldBreakPos.IsBlockInWorld() && targetChunk.isLoaded)
					targetChunk.SetBlockID(worldBreakPos, BlockID.AIR);
			}
		}
		if (Input.GetMouseButtonDown(1) || (Input.GetMouseButton(1) && hasMoved && autoPlaceBlock))	//place block
		{
			if (placeBlock.gameObject.activeSelf)
			{
				Coords	chunkPos = worldPlacePos.WorldToChunkPos();
				Chunk	targetChunk = world.FindChunk(chunkPos);

				if (worldPlacePos.IsBlockInWorld() && targetChunk.isLoaded)
					targetChunk.SetBlockID(worldPlacePos, selectedBlockID);
			}
		}
		if (Input.GetMouseButtonDown(2) && breakBlock.gameObject.activeSelf)	//copy block
		{
			Coords	chunkPos = worldBreakPos.WorldToChunkPos();
			Chunk	targetChunk = world.FindChunk(chunkPos);

			if (worldBreakPos.IsBlockInWorld() && targetChunk.isLoaded)
			{
				BlockID blockID = targetChunk.FindBlockID(worldBreakPos.WorldToBlockPos());

				if (BlockID.AIR < blockID)
					UpdateSelectedBlockID(blockID);
			}
		}
		hasMoved = false;
	}

	public void	UpdateSelectedBlockID(BlockID value)
	{
		selectedBlockID = value;
		selectedBlockText.text = world.blocktypes[(int)selectedBlockID].blockName + " selected";
	}

	private void	CalculateVelocity()
	{
		//horizontal movements controls
		velocity = (transform.forward * vertical) + (transform.right * horizontal);

		//speed controls
		if (isCrouching)
			velocity *= PlayerData.crouchSpeed;
		else if (isSprinting)
			velocity *= PlayerData.sprintSpeed;
		else
			velocity *= PlayerData.walkSpeed;

		velocity.x *= Time.fixedDeltaTime;
		velocity.z *= Time.fixedDeltaTime;

		velocity = CheckHorizontalSpeed(velocity);

		//gravity implementation
		if (isGrounded || isFlying)
			verticalSpeed *= 0;
		if (verticalSpeed > PlayerData.maxFallSpeed)
			verticalSpeed += PlayerData.gravityForce * Time.fixedDeltaTime;

		//fly controls
		if (isFlying)
		{
			if (isSprinting)
				velocity *= PlayerData.flySpeed;
			else
				velocity *= 2f;

			if (isJumping && isCrouching)
				velocity.y = 0;
			else if (isJumping)
				velocity.y = PlayerData.ascentSpeed;
			else if (isCrouching)
				velocity.y = -PlayerData.ascentSpeed;

			if (isSprinting)
				velocity.y *= PlayerData.flyFactor;
		}
		else
			velocity.y += verticalSpeed;

		//makes movement speed proporional to time
		velocity.y *= Time.fixedDeltaTime;

		velocity = CheckVerticalSpeed(velocity);
	}

	private void	CalculatePosition()
	{
		Coords playerCurrentPos = new Coords(playerFeet.position);

		if (playerPos.y != playerCurrentPos.y || playerPos.x != playerCurrentPos.x || playerPos.z != playerCurrentPos.z)
		{
			playerChunkPos = playerCurrentPos.WorldToChunkPos();

			playerLastPos = playerPos;
			playerPos = playerCurrentPos;

			hasMoved = true;
		}
	}

	private Vector3	CheckHorizontalSpeed(Vector3 velocity)	//TODO : calculate where the player should end up after colision
	{
		if (!isGhosting)
		{
			if (velocity.x > 0 && isRightBlocked)
			{
				velocity.x = 0;
			}
			if (velocity.x < 0 && isLeftBlocked)
			{
				velocity.x = 0;
			}
			if (velocity.z > 0 && isFrontBlocked)
			{
				velocity.z = 0;
			}
			if (velocity.z < 0 && isBackBlocked)
			{
				velocity.z = 0;
			}
		}
		return (velocity);
	}

	private Vector3	CheckVerticalSpeed(Vector3 velocity)	//TODO : calculate where the player should end up after colision
	{
		if (!isGhosting)
		{
			if (velocity.y < 0 && isBottomBlocked(velocity.y))
			{
				velocity.y = 0;
				isGrounded = true;
			}
			else
				isGrounded = false;
			if (velocity.y > 0 && isTopBlocked(velocity.y))
				velocity.y = 0;
		}
		return (velocity);
	}

	private void	TurnCamera()
	{
		yRotation += mouseHorizontal * PlayerData.cameraSpeed;
		xRotation += mouseVertical * PlayerData.cameraSpeed;

		yRotation %= 360f;
		xRotation = Mathf.Clamp(xRotation, -90f, 90f);

		playerCam.transform.localRotation = Quaternion.AngleAxis(xRotation, Vector3.left);		//moves the head around
		transform.localRotation = Quaternion.AngleAxis(yRotation, Vector3.up);					//turns the body left/right
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

	private bool	isBottomBlocked (float fallSpeed)	//checkDownSpeed
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
			return (true);
		}
		return (false);
	}

	private bool	isTopBlocked (float jumpSpeed)	//checkUpSpeed
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
			return (true);
		}
		return (false);
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
