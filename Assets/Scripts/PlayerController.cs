using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.SceneView;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCamera playerCamera;
    public Collider cc { get; private set; }
    public bool CanMove { get; set; }

    [Header("Config")]
    [SerializeField] private float playerMouseSensitivity;
    [SerializeField] private float playerSpeed;
    [SerializeField] private float playerSprintMultiplier;
    [SerializeField] private float playerSprintChargeup;
    [SerializeField] private AnimationCurve playerSprintCurve;
    [SerializeField] private float playerGravity;
    [SerializeField] private float playerJumpPower;
    [SerializeField] private float playerJumpBuffer = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool canJump;


    private CharacterController controller;
    private Camera mainCamera;
    private PlayerAttacker attacker;

    private float ySpeed;
    private bool onGround;
    private float jumpBuffer;
    private float currentSprintDuration;

    private Vector3 externalVelocity;
    private float playerSpeedPercentage = 1.0f;

    private void Awake()
    {
        // Initialise components
        controller = GetComponent<CharacterController>();
        cc = GetComponent<Collider>();
        attacker = GetComponentInChildren<PlayerAttacker>();
        attacker.OnAddVelocity += AddExternalVelocity;
        attacker.OnChangeSpeed += AdjustPlayerSpeed;

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

        // Ground interaction
        if (onGround && !controller.isGrounded)
            Fall();
        if (!onGround && controller.isGrounded)
            Land();

        // Camera movement
        float cameraX = Input.GetAxis("Mouse X") * playerMouseSensitivity * Time.deltaTime;
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y+ cameraX, transform.eulerAngles.z);
        playerCamera.transform.rotation = Quaternion.Euler(playerCamera.transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z);

        // Initialize external velocity
        externalVelocity = Vector3.zero;

    }


    // Returns multiplier based on the players use of sprinting
    private float GetSprintSpeed()
    {
        // Increase timer if sprinting, reset if not
        if (Input.GetKey(KeyCode.LeftShift))
            currentSprintDuration += Time.deltaTime;
        else
            currentSprintDuration = 0;

        // Set speed multiplier based on how long the player has been sprinting
        float t = currentSprintDuration / playerSprintChargeup;
        float speed = Mathf.SmoothStep(1, playerSprintMultiplier, playerSprintCurve.Evaluate(t));

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

    // Return vector based on player input
    private Vector3 GetPlayerMovement()
    {
        // Grab current input direction
        Vector3 moveDirection = GetMovementDirection();

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

    // Allows external scripts to apply velocity to the player
    private void AddExternalVelocity(Vector3 velocity)
    {
        externalVelocity += velocity;
    }

    // Allows the players speed to be limited a specific percentage
    private void AdjustPlayerSpeed(float percentage)
    {
        playerSpeedPercentage = percentage;
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
