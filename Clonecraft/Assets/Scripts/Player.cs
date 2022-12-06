using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*	IDEAS

	make croutch croutchier

*/

public class Player : MonoBehaviour
{
	private bool	isSprinting;
	private bool	isCroutching;
	private bool	isGrounded;
	private bool	isJumping;	//jumpRequest
	private bool	isFlying;
	private bool	isGhosting;

	private Transform		playerCamera;			//cam
	private float			verticalRotation;

	private float			mouseHorizontal;
	private float			mouseVertical;
	private float			mouseScroll;				//Scroll

	private Vector3			velocity;
	private float			verticalSpeed;			//vertical Momentum

	private float			rightward;				//Horizontal	(w s)
	private float			frontward;				//Vertical		(a d)

	public Transform		placeBlock;
	public Transform		breakBlock;				//highlightBlock

	public Text				selectedBlockText;
	public BlockID			selectedBlockID;

	private World			world;

	private void	Start()
	{
		playerCamera = GameObject.Find("Player Camera").transform;
		world = GameObject.Find("World").GetComponent<World>();

		Cursor.lockState = CursorLockMode.Locked;

		verticalRotation = 0;

		UpdatedSelectedBlockID(PlayerData.defaultBlock);
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
		FindTargetedBlocks();
		GetPlayerInputs();
	}

	private void	GetPlayerInputs()
	{
		rightward = Input.GetAxis("Horizontal");	//in project settings
		frontward = Input.GetAxis("Vertical");
		mouseHorizontal = Input.GetAxis("Mouse X");
		mouseVertical = Input.GetAxis("Mouse Y");
		mouseScroll = Input.GetAxis("Mouse ScrollWheel");

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

		if (mouseScroll != 0)
			ChangeSelectedBlock(BlockID.AIR);

		if (breakBlock.gameObject.activeSelf)
			BlockAction();
	}

	private void	UpdatedSelectedBlockID(BlockID value)
	{
		selectedBlockID = value;
		selectedBlockText.text = world.blocktypes[(int)selectedBlockID].blockName + " block selected";
	}

	private void	FindTargetedBlocks()	//doesn't entirely prevent placing a block that clips
	{
		Vector3	firstPos = new Vector3();
		Vector3	secondPos = new Vector3();
		//Vector3	thirdPos = new Vector3();

		float	step = 0;

		firstPos = playerCamera.position;

		while (step <= PlayerData.reach)
		{
			secondPos = firstPos;
			firstPos = playerCamera.position + (playerCamera.forward * step);

			if (BlockID.AIR < world.FindBlockID(new Coords(firstPos)))					//optimize me
			{
				breakBlock.position = new Vector3(
					Mathf.FloorToInt(firstPos.x),
					Mathf.FloorToInt(firstPos.y),
					Mathf.FloorToInt(firstPos.z));

				breakBlock.gameObject.SetActive(true);

				Coords	rPos = new Coords(secondPos);
				Coords	camPos = new Coords(playerCamera.position);

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

	private void	BlockAction()
	{
		if (Input.GetMouseButton(0))	//break block
		{
			Coords worldPos = new Coords(breakBlock.position);
			Coords chunkPos = worldPos.WorldToChunkPos();

			if (worldPos.IsBlockInWorld())
				world.FindChunk(chunkPos).SetBlockID(worldPos, BlockID.AIR);
		}
		if (Input.GetMouseButton(1))	//place block
		{
			Coords worldPos = new Coords(placeBlock.position);
			Coords chunkPos = worldPos.WorldToChunkPos();

			if (worldPos.IsBlockInWorld())
				world.FindChunk(chunkPos).SetBlockID(worldPos, selectedBlockID);
		}
		if (Input.GetMouseButton(2))	//copy block
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
		velocity = (transform.forward * frontward) + (transform.right * rightward);

		//speed controls
		if (isCroutching)
			velocity *= PlayerData.croutchSpeed;
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

			if (isJumping && isCroutching)
				velocity.y = 0;
			else if (isJumping)
				velocity.y = PlayerData.ascentSpeed;
			else if (isCroutching)
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
		transform.Rotate(Vector3.up * mouseHorizontal * PlayerData.cameraSpeed);					// left/right cam movement

		//playerCamera.transform.Rotate(Vector3.right * -mouseVertical * cameraSpeed);	//  up/down   cam movement (unbounded)

		verticalRotation += -mouseVertical * PlayerData.cameraSpeed;

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
			verticalSpeed = PlayerData.jumpForce;
			if (isCroutching)
				verticalSpeed *= PlayerData.croutchJumpFactor;
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
