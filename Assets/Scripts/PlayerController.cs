using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using static UnityEditor.SceneView;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCamera playerCamera;

    public Animator Animator { get; private set; }
    public Collider cc { get; private set; }
    public bool CanMove { get; set; }
    public bool IsSprinting { get; private set; }
    public bool IsAttacking { get; set; }

    [Header("Main")]
    [SerializeField] private float playerMouseSensitivity;
    [SerializeField] private float playerSpeed;
    [Header("Sprint")]
    [SerializeField] private float playerSprintMultiplier;
    [SerializeField] private float playerSprintChargeup;
    [SerializeField] private AnimationCurve playerSprintCurve;
    [SerializeField] private float playerSprintDelay = 0.25f;
    private float sprintCooldown;
    private float sprintProgress;
    
    [Header("Jump")]
    [SerializeField] private float playerGravity;
    [SerializeField] private float playerJumpPower;
    [SerializeField] private float playerJumpBuffer = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool canJump;


    private CharacterController controller;
    private Camera mainCamera;

    private Vector3 startingModelPosition;

    private float ySpeed;
    private bool onGround;
    private float jumpBuffer;
    private float currentSprintDuration;

    private Vector3 externalVelocity;
    private float playerSpeedPercentage = 1.0f;

    public enum PlayerStates
    {
        Idle,
        Walking,
        Sprinting,
        Charging,
        Stunned,
        Dodging
    }

    public PlayerStates PlayerState = PlayerStates.Idle;
    private readonly int isWalking = Animator.StringToHash("isWalking");
    private readonly int walkSpeed = Animator.StringToHash("walkSpeed");
    private readonly int isSprinting = Animator.StringToHash("isSprinting");

    private void Awake()
    {
        // Initialise components
        controller = GetComponent<CharacterController>();
        cc = GetComponent<Collider>();
        Animator = GetComponentInChildren<Animator>();
        startingModelPosition = Animator.transform.localPosition;

        // Initialise variables
        CanMove = true;

        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;


    }


    private void OnEnable()
    {
        GameManager.playerController = this;
    }


    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {

        #region FIX_SLIDE_ROTATION
        // Rotate model during slide animation
        if (PlayerState == PlayerStates.Sprinting && Animator.GetCurrentAnimatorStateInfo(0).IsName("PeeweeSlide"))
        {
            Animator.transform.position = transform.position - Vector3.up / 1.5f;
            Animator.transform.localEulerAngles = new Vector3(75, Animator.transform.localRotation.y, Animator.transform.localRotation.z);
        }
        else
        {
            Animator.transform.localPosition = startingModelPosition;
            Animator.transform.rotation = transform.rotation;
        }
        #endregion
        
        MovePlayer();
        
        // Ground interaction
        if (onGround && !controller.isGrounded)
            Fall();
        if (!onGround && controller.isGrounded)
            Land();

        // Camera movement
        float cameraX = Input.GetAxis("Mouse X") * playerMouseSensitivity * Time.deltaTime;
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y+ cameraX, transform.eulerAngles.z);
        playerCamera.transform.rotation = Quaternion.Euler(playerCamera.transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z);
        

    }
    

    private void MovePlayer()
    {
        // Sprint delay
        if (sprintCooldown > 0)
            sprintCooldown -= Time.deltaTime;
        // If the player is currently not holding the sprint key and the countdown reaches 0
        if (PlayerState == PlayerStates.Sprinting && !IsSprinting && sprintCooldown <= 0)
            PlayerState = PlayerStates.Idle;
        
        if(canJump)
            CheckJump();
        

        // Initialize velocity
        Vector3 velocity = Vector3.zero;

        // Store vector based on players input this frame
        if(CanMove)
            velocity = GetPlayerMovement();


        // Apply gravity
        if (!onGround)
            ySpeed -= playerGravity * Time.deltaTime;
        // Apply any vertical velocity
        velocity.y = ySpeed;
        // Apply any external velocity from other scripts
        velocity += externalVelocity;
        // Multiple velocity based on current speed percentage
        velocity *= playerSpeedPercentage;

        controller.Move(velocity * Time.deltaTime);
        
        // Initialize external velocity
        externalVelocity = Vector3.zero;
    }
    
    // Return vector based on player input
    private Vector3 GetPlayerMovement()
    {
        // Grab current input direction
        Vector3 moveDirection = GetMovementDirection();
        
        // If they're sliding, force a forward movement
        if (IsSprinting)
            moveDirection = transform.forward.normalized;

        if(moveDirection != Vector3.zero && PlayerState == PlayerStates.Idle)
            StartWalking(); 
        if(moveDirection == Vector3.zero && PlayerState == PlayerStates.Walking)
            StopWalking();

        // Apply sprint multiplier
        float sprintSpeed = GetSprintSpeed();
        float speed = playerSpeed * sprintSpeed;

        return moveDirection * speed;
    }
    
    private Vector3 GetMovementDirection()
    {
        // Grab vector based on players current rotation
        Vector3 inputDirection = transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical");

        // Return normalized version
        return inputDirection.normalized;

    }


    // Returns multiplier based on the players use of sprinting
    private float GetSprintSpeed()
    {
        // If the player is currently charging dont sprint
        if (PlayerState == PlayerStates.Charging)
            return 1;
        
        float cooldown = sprintCooldown / playerSprintDelay;
        
        // Start sprinting if the key is down and theres no cooldown
        if (Input.GetKey(KeyCode.LeftShift) && cooldown <= 0)
            StartSprinting();
        else if(IsSprinting && sprintProgress >= 1)
            StopSprinting();

        if (IsSprinting)
            currentSprintDuration += Time.deltaTime;

        // Set speed multiplier based on how long the player has been sprinting
        sprintProgress = currentSprintDuration / playerSprintChargeup;
        float speed = Mathf.SmoothStep(1, playerSprintMultiplier, playerSprintCurve.Evaluate(sprintProgress));

        return speed;

    }

    private void CheckJump()
    {
        // Jump logic

        // Buffer
        jumpBuffer -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space))
            jumpBuffer = playerJumpBuffer;

        // Jump detection
        if (onGround && jumpBuffer > 0.0f)
            Jump();
    }

    private void StartWalking()
    {
        PlayerState = PlayerStates.Walking;
        Animator.SetBool(isWalking, true);
    }

    private void StopWalking()
    {
        PlayerState = PlayerStates.Idle;
        Animator.SetBool(isWalking, false);
    }

    private void StartSprinting()
    {
        PlayerState = PlayerStates.Sprinting;
        IsSprinting = true;
        Animator.SetBool(isSprinting, true);
    }

    private void StopSprinting()
    {
        IsSprinting = false;
        Animator.SetBool(isSprinting, false);
        
        currentSprintDuration = 0;
        sprintCooldown = playerSprintDelay;

    }

    // Allows external scripts to apply velocity to the player
    public void AddExternalVelocity(Vector3 velocity)
    {
        externalVelocity += velocity;
    }

    // Allows the players speed to be limited a specific percentage
    public void AdjustPlayerSpeed(float percentage)
    {
        playerSpeedPercentage = percentage;
        Animator.SetFloat(walkSpeed, percentage);
    }



    // If the player falls off without jumping
    private void Fall()
    {
        onGround = false;
    }
    // If the player jumps
    private void Jump()
    {

        onGround = false;

        ySpeed = playerJumpPower;
    }
    // When the player hits the ground
    private void Land()
    {
        onGround = true;

        // Setting to a small negative values prevent issues with floating midair and other character controller quirks
        ySpeed = -0.5f;
    }
}
