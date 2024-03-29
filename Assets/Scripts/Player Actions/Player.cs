using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*	IDEAS

	test both directions are safe TOGETHER before allowing to go in croutch mode

*/

public class Player : MonoBehaviour
{
	private bool		isSprinting;
	private bool		isCrouching;
	private bool		isGrounded;
	private bool		isJumping;				//jumpRequest
	private bool		isFlying;
	private bool		isGhosting;
	private bool		hasMoved;

	private bool		getNextBlock;			//replace/combine with isCroutching(?)
	private bool		autoPlaceBlock;

	private Transform	playerCam;				//cam
	private Transform	playerFeet;				//body

	private float		mouseHorizontal;
	public float		yRotation;

	private float		mouseVertical;
	public float		xRotation;

	private float		mouseScroll;			//scroll

	private Vector3		velocity;
	private float		verticalSpeed;			//vertical Momentum

	private float		horizontal;				//(w s)
	private float		vertical;				//(a d)

	public Coords		playerPos;
	public Coords		playerChunkPos;

	public Coords		hitBoxCorner1;
	public Coords		hitBoxCorner2;

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

		Cursor.lockState = CursorLockMode.Locked;

		UpdateSelectedBlockID(0);			//hardcoded: move me to hotbar (why no worky???)
	}

	private void	FixedUpdate()
	{
		JumpCheck();
		UpdatePosition();
		CalculateVelocity();

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

	private void	FindTargetedBlocks()
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

			if (cPos.IsBlockInWorld() && BlockID.AIR < world.FindBlockID(cPos))					//optimize me
			{
				breakBlock.position = new Vector3(
					Mathf.FloorToInt(currentPos.x),
					Mathf.FloorToInt(currentPos.y),
					Mathf.FloorToInt(currentPos.z));

				breakBlock.gameObject.SetActive(true);

				if (getNextBlock)
				{
					while (step <= PlayerData.reach && (nPos.IsEqual(cPos)))
					{
						step += PlayerData.reachIncrement;
						nextPos = playerCam.position + (playerCam.forward * step);
						nPos = new Coords(nextPos);
					}

					if (nPos.IsBlockInWorld() && BlockID.AIR == world.FindBlockID(nPos))
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
				if (lPos.IsBlockInWorld() && (isGhosting || !(lPos.IsInVolume(hitBoxCorner1, hitBoxCorner2))))
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
		selectedBlockText.text = world.blocktypes[(int)selectedBlockID].blockName;
	}

	private void	CalculateVelocity()
	{
		//horizontal movements manager
		velocity = (transform.forward * vertical) + (transform.right * horizontal);
		if (vertical != 0f && horizontal != 0f)
			velocity /= 1.4142f;//Mathf.Sqrt(Mathf.Pow(horizontal, 2) + Mathf.Pow(horizontal, 2));	//always sqrt(2)

		//horizontal speed manager
		velocity *= PlayerData.walkSpeed;
		if (isCrouching)
			velocity *= PlayerData.crouchSpeedFactor;
		else if (isSprinting)
			velocity *= PlayerData.sprintSpeedFactor;

		//gravity implementation
		if (isFlying || isGrounded)
			verticalSpeed *= 0;
		if (verticalSpeed > PlayerData.maxFallSpeed)
			verticalSpeed += PlayerData.gravityForce * Time.fixedDeltaTime;

		//vertical speed manager
		if (isFlying)
		{
			velocity *= PlayerData.flySpeedFactor;

			if (isJumping && isCrouching)
				velocity.y = 0;
			else if (isJumping)
				velocity.y = PlayerData.flyAscentFactor;
			else if (isCrouching)
				velocity.y = -PlayerData.flyAscentFactor;

			if (isSprinting)
				velocity *= PlayerData.flySprintFactor;
		}
		else
			velocity.y += verticalSpeed;

		//makes movement speed proporional to time
		velocity *= Time.fixedDeltaTime;

		if (!isGhosting)	//TODO : divide speed a few time before terminating checks instead???
			velocity = CheckCollisions(velocity);
		else
			isGrounded = false;
	}

	private void	UpdatePosition()
	{
		hitBoxCorner1 = new Coords(
			playerFeet.position.x - PlayerData.playerWidht,
			playerFeet.position.y,
			playerFeet.position.z - PlayerData.playerWidht);
		hitBoxCorner2 = new Coords(
			playerFeet.position.x + PlayerData.playerWidht,
			playerFeet.position.y + PlayerData.playerHeight,
			playerFeet.position.z + PlayerData.playerWidht);

		Coords playerCurrentPos = new Coords(playerFeet.position);

		if (playerPos.y != playerCurrentPos.y || playerPos.x != playerCurrentPos.x || playerPos.z != playerCurrentPos.z)
		{
			playerChunkPos = playerCurrentPos.WorldToChunkPos();

			playerPos = playerCurrentPos;

			hasMoved = true;
		}
	}

	private Vector3 CheckCollisions(Vector3 velocity)
	{
		if (IsYBlocked(velocity))
		{
			verticalSpeed = 0;
			velocity.y = 0;
		}

		if (IsXBlocked(velocity))
			velocity.x = 0;

		if (IsZBlocked(velocity))
			velocity.z = 0;

		return (velocity);
	}

	//checking if its possible to step up to allow movement
	private bool	IsSteppable(Coords pos1, Coords pos2)
	{
		if (WillCollide((		//step zone
				new Coords(pos1.x, pos1.y, pos1.z).ListCoordsInVolume(
				new Coords(pos2.x, pos1.y, pos2.z))))
			&&
			!WillCollide((		//pos after step
				new Coords(pos1.x, pos1.y + 1, pos1.z).ListCoordsInVolume(
				new Coords(pos2.x, pos2.y + 1, pos2.z)))))
		{
			return (true);
		}
		return (false);
	}

	//checking if the movement will result in falling
	private bool	IsFallable(Coords pos1, Coords pos2)
	{
		if (!WillCollide((		//pos after move
				new Coords(pos1.x, pos1.y - 2, pos1.z).ListCoordsInVolume(
				new Coords(pos2.x, pos1.y - 1, pos2.z)))))
		{
			return (true);
		}
		return (false);
	}

	//checking top-bottom collisions
	private bool	IsYBlocked(Vector3 velocity)
	{
		Vector3 nextPos = transform.position;
		nextPos.y += velocity.y;

		Coords	pos1 = new Coords(nextPos.x - PlayerData.playerWidht, nextPos.y, nextPos.z - PlayerData.playerWidht);
		Coords	pos2 = new Coords(nextPos.x + PlayerData.playerWidht, nextPos.y + PlayerData.playerHeight, nextPos.z + PlayerData.playerWidht);

		if (velocity.y > 0 && WillCollide((		//top
				new Coords(pos1.x, pos2.y, pos1.z).ListCoordsInVolume(
				new Coords(pos2.x, pos2.y, pos2.z))))
			||
			velocity.y < 0 && WillCollide((		//bottom
				new Coords(pos1.x, pos1.y, pos1.z).ListCoordsInVolume(
				new Coords(pos2.x, pos1.y, pos2.z)))))
		{
			if (velocity.y < 0)
				isGrounded = true;
			return (true);
		}
		isGrounded = false;
		return (false);
	}

	//checking left-right collisions
	private bool	IsXBlocked(Vector3 velocity)
	{
		Vector3 nextPos = transform.position;
		nextPos.x += velocity.x;

		Coords	pos1 = new Coords(nextPos.x - PlayerData.playerWidht, nextPos.y, nextPos.z - PlayerData.playerWidht);
		Coords	pos2 = new Coords(nextPos.x + PlayerData.playerWidht, nextPos.y + PlayerData.playerHeight, nextPos.z + PlayerData.playerWidht);

		if (velocity.x > 0 && WillCollide((		//left
				new Coords(pos2.x, pos1.y, pos1.z).ListCoordsInVolume(
				new Coords(pos2.x, pos2.y, pos2.z))))
			||
			velocity.x < 0 && WillCollide((		//right
				new Coords(pos1.x, pos1.y, pos1.z).ListCoordsInVolume(
				new Coords(pos1.x, pos2.y, pos2.z)))))
		{
			if (isGrounded && IsSteppable(pos1, pos2))
			{
				transform.Translate(new Vector3(0f, 1f, 0f), Space.World);
			}
			else
				return (true);
		}
		else if (isGrounded && isCrouching && !isFlying  && IsFallable(pos1, pos2))
			return (true);
		return (false);
	}

	//checking front-back collisions
	private bool	IsZBlocked(Vector3 velocity)
	{
		Vector3 nextPos = transform.position;
		nextPos.z += velocity.z;

		Coords	pos1 = new Coords(nextPos.x - PlayerData.playerWidht, nextPos.y, nextPos.z - PlayerData.playerWidht);
		Coords	pos2 = new Coords(nextPos.x + PlayerData.playerWidht, nextPos.y + PlayerData.playerHeight, nextPos.z + PlayerData.playerWidht);

		if (velocity.z > 0 && WillCollide((		//front
				new Coords(pos1.x, pos1.y, pos2.z).ListCoordsInVolume(
				new Coords(pos2.x, pos2.y, pos2.z))))
			||
			velocity.z < 0 && WillCollide((		//back
				new Coords(pos1.x, pos1.y, pos1.z).ListCoordsInVolume(
				new Coords(pos2.x, pos2.y, pos1.z)))))
		{
			if (isGrounded && IsSteppable(pos1, pos2))
			{
				transform.Translate(new Vector3(0f, 1f, 0f), Space.World);
			}
			else
				return (true);
		}
		else if (isGrounded && isCrouching && !isFlying && IsFallable(pos1, pos2))
			return (true);
		return (false);
	}

	private bool	WillCollide(Coords[] blockArray)
	{
		foreach (Coords blockPos in blockArray)
			if (world.IsBlockSolid(blockPos))
				return (true);
		return (false);
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

	private void	JumpCheck()
	{
		if (isJumping && isGrounded)
		{
			verticalSpeed = PlayerData.jumpForce;
			if (isCrouching)
				verticalSpeed *= PlayerData.crouchJumpFactor;
			isGrounded = false;
		}
	}
}
