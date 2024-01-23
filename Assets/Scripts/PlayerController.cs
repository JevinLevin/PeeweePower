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
        controller = GetComponent<CharacterController>();
        cc = GetComponent<Collider>();
        Cursor.lockState = CursorLockMode.Locked;

        attacker = GetComponentInChildren<PlayerAttacker>();
        attacker.OnAddVelocity += AddExternalVelocity;
        attacker.OnChangeSpeed += AdjustPlayerSpeed;

        CanMove = true;

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

        if(CanMove)
            velocity = GetPlayerMovement();


        // Apply gravity
        if (!onGround)
            ySpeed -= playerGravity * Time.deltaTime;

        velocity.y = ySpeed;

        velocity += externalVelocity;
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


        externalVelocity = Vector3.zero;

    }


    private float GetSprintSpeed()
    {
        if (Input.GetKey(KeyCode.LeftShift))
            currentSprintDuration += Time.deltaTime;
        else
            currentSprintDuration = 0;

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

        if (onGround && jumpBuffer > 0.0f)
            Jump();
    }

    private Vector3 GetPlayerMovement()
    {
        Vector3 moveDirection = GetMovementDirection();

        float sprintSpeed = GetSprintSpeed();
        float speed = playerSpeed * sprintSpeed;
        return moveDirection * speed;
    }

    private Vector3 GetMovementDirection()
    {
        // Get players input direction
        //Vector3 inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        //// Rotate based on camera direction
        //Vector3 moveDirection = mainCamera.transform.TransformDirection(inputDirection);
        //moveDirection.y = 0;

        Vector3 inputDirection = transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical");

        return inputDirection.normalized;

    }

    private void AddExternalVelocity(Vector3 velocity)
    {
        externalVelocity += velocity;
    }

    private void AdjustPlayerSpeed(float percentage)
    {
        playerSpeedPercentage = percentage;
    }



    private void Fall()
    {
        onGround = false;
    }
    private void Jump()
    {

        onGround = false;

        ySpeed = playerJumpPower;
    }
    private void Land()
    {
        onGround = true;

        // Setting to a small negative values prevent issues with floating midair and other character controller quirks
        ySpeed = -0.5f;
    }
}
