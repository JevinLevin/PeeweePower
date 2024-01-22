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

    [Header("Config")]
    [SerializeField] private float playerMouseSensitivity;
    [SerializeField] private float playerSpeed;
    [SerializeField] private float playerSprintMultiplier;
    [SerializeField] private float playerSprintChargeup;
    [SerializeField] private AnimationCurve playerSprintCurve;
    [SerializeField] private float playerGravity;
    [SerializeField] private float playerJumpPower;
    [SerializeField] private float playerJumpBuffer = 0.2f;


    private CharacterController controller;
    private Camera mainCamera;

    private float ySpeed;
    private bool onGround;
    private float jumpBuffer;
    private float currentSprintDuration;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        Vector3 moveDirection = GetMovementDirection();

        float sprintSpeed = GetSprintSpeed();
        float speed = playerSpeed * sprintSpeed;
        Vector3 velocity = moveDirection * speed;

        // Jump logic
        // Buffer
        jumpBuffer -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space))
            jumpBuffer = playerJumpBuffer;

        if (onGround && jumpBuffer > 0.0f)
            Jump();

        // Apply gravity
        if (!onGround)
            ySpeed -= playerGravity * Time.deltaTime;

        velocity.y = ySpeed;

        controller.Move(velocity * Time.deltaTime);

        // Ground interaction
        if (onGround && !controller.isGrounded)
            Fall();
        if (!onGround && controller.isGrounded)
            Land();

        float cameraX = Input.GetAxis("Mouse X") * playerMouseSensitivity * Time.deltaTime;
        transform.eulerAngles += new Vector3(0, cameraX, 0);
        playerCamera.transform.eulerAngles += new Vector3(0, cameraX, 0);

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
