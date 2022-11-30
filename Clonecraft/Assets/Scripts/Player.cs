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

	public readonly float	croutchSpeed = 3f;
	public readonly float	walkSpeed = 6f;
	public readonly float	sprintSpeed = 12f;

	public readonly float	flySpeed = 4f;
	public readonly float	ascentSpeed = 12f;

	public readonly float	jumpForce = 8f;
	public readonly float	gravityForce = -24f;
	public readonly float 	maxFallSpeed = -120f;

	public readonly float	playerWidht = 0.3f;		//radius~~
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
	
		if (Input.GetButtonDown("Fly"))
			isFlying = true;
		if (Input.GetButtonDown("Alt"))
			isFlying = false;
		if (Input.GetButtonDown("TP"))
			transform.Translate((Vector3.up * 8f), Space.World);
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
		if ((velocity.x > 0 && isRightBlocked) || (velocity.x < 0 && isLeftBlocked))
			velocity.x = 0;
		if ((velocity.z > 0 && isFrontBlocked) || (velocity.z < 0 && isBackBlocked))
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
		if (velocity.y < 0)
			velocity.y = checkFallSpeed(velocity.y);
		else if (velocity.y > 0)
			velocity.y = checkJumpSpeed(velocity.y);
	}

	private void TurnCamera()
	{
		transform.Rotate(Vector3.up * mouseHorizontal * cameraSpeed);					// left/right cam movement

		//playerCamera.transform.Rotate(Vector3.right * -mouseVertical * cameraSpeed);	//  up/down   cam movement

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









/*
public class Player : MonoBehaviour {

    public bool isGrounded;
    public bool isSprinting;

    private Transform cam;
    private World world;

    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float jumpForce = 5f;
    public float gravity = -9.8f;

    public float playerWidth = 0.15f;
    public float boundsTolerance = 0.1f;

    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private Vector3 velocity;
    private float verticalMomentum = 0;
    private bool jumpRequest;

    private void Start() {

        cam = GameObject.Find("Player Camera").transform;
        world = GameObject.Find("World").GetComponent<World>();

    }

    private void FixedUpdate() {
        
        CalculateVelocity();
        if (jumpRequest)
            Jump();

        transform.Rotate(Vector3.up * mouseHorizontal);
        cam.Rotate(Vector3.right * -mouseVertical);
        transform.Translate(velocity, Space.World);

    }

    private void Update() {

        GetPlayerInputs();

    }

    void Jump () {

        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;

    }

    private void CalculateVelocity () {

        // Affect vertical momentum with gravity.
        if (verticalMomentum > gravity)
            verticalMomentum += Time.fixedDeltaTime * gravity;

        // if we're sprinting, use the sprint multiplier.
        if (isSprinting)
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
        else
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;

        // Apply vertical momentum (falling/jumping).
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
            velocity.x = 0;

        if (velocity.y < 0)
            velocity.y = checkDownSpeed(velocity.y);
        else if (velocity.y > 0)
            velocity.y = checkUpSpeed(velocity.y);


    }

    private void GetPlayerInputs () {

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        if (Input.GetButtonDown("Sprint"))
            isSprinting = true;
        if (Input.GetButtonUp("Sprint"))
            isSprinting = false;

        if (isGrounded && Input.GetButtonDown("Jump"))
            jumpRequest = true;

    }

    private float checkDownSpeed (float downSpeed) {

        if (
            world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth) ||
            world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth) ||
            world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth) ||
            world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)
           ) {

            isGrounded = true;
            return 0;

        } else {

            isGrounded = false;
            return downSpeed;

        }

    }

    private float checkUpSpeed (float upSpeed) {

        if (
            world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth) ||
            world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth) ||
            world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth) ||
            world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth)
           ) {

            return 0;

        } else {

            return upSpeed;

        }

    }

    public bool front {

        get {
            if (
                world.CheckForVoxel(transform.position.x, transform.position.y, transform.position.z + playerWidth) ||
                world.CheckForVoxel(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidth)
                )
                return true;
            else
                return false;
        }

    }

    public bool back {

        get {
            if (
                world.CheckForVoxel(transform.position.x, transform.position.y, transform.position.z - playerWidth) ||
                world.CheckForVoxel(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth)
                )
                return true;
            else
                return false;
        }

    }

    public bool left {

        get {
            if (
                world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y, transform.position.z) ||
                world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + 1f, transform.position.z)
                )
                return true;
            else
                return false;
        }

    }

    public bool right {

        get {
            if (
                world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y, transform.position.z) ||
                world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + 1f, transform.position.z)
                )
                return true;
            else
                return false;
        }

    }

}
*/