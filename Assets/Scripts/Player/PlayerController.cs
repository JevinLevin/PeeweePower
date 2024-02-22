using System;
using System.Collections;
using System.Numerics;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Light deathLight;

    [SerializeField] private AudioClip peeweeDie;
        
    public Animator Animator { get; private set; }
    public Collider cc { get; private set; }
    public bool CanMove { get; set; }
    public bool IsSprinting { get; private set; }
    public bool IsAttacking { get; set; }
    public bool IsCharging { get; set; }
    public bool IsJumping { get; set; }

    private bool alive = true;


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
    [Header("Stun")] 
    [SerializeField] private float defaultStunDuration = 2;
    [SerializeField] private float stunKnockbackDuration;
    //[SerializeField] private float stunDistance;
    [SerializeField] private AnimationCurve stunCurve;
    [SerializeField] private float stunGracePeriodLength = 0.5f;
    [SerializeField] private Material stunBirdMaterial;
    [SerializeField] private float stunBirdFadeOutTime = 0.5f;
    private float stunGracePeriod;
    private float stunTime;
    
    
    [Header("Jump")]
    [SerializeField] private float playerGravity;
    [SerializeField] private float playerJumpPower;
    [SerializeField] private float playerJumpBuffer = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool canJump;
    [SerializeField] private bool immune;


    public CharacterController Controller { get; private set; }
    private Camera mainCamera;

    private Vector3 startingModelPosition;

    private float ySpeed;
    private bool onGround;
    private float jumpBuffer;
    private float currentSprintDuration;

    private Vector3 externalVelocity;
    private float playerSpeedPercentage = 1.0f;

    public Action OnStun;
    public Action<float> OnStunTime;
    public Action OnLand;

    public enum PlayerStates
    {
        Idle,
        Walking,
        Sprinting,
        Charging,
        Stunned,
        Dodging,
        Jumping
    }

    public PlayerStates PlayerState { get; set; }
    private readonly int isWalking = Animator.StringToHash("isWalking");
    private readonly int walkSpeed = Animator.StringToHash("walkSpeed");
    private readonly int isSprinting = Animator.StringToHash("isSprinting");
    private readonly int isStunned = Animator.StringToHash("isStunned");
    private static readonly int IsDead = Animator.StringToHash("isDead");
    private static readonly int Alpha = Shader.PropertyToID("_Alpha");

    private void Awake()
    {
        // Initialise components
        Controller = GetComponent<CharacterController>();
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
        GameManager.Instance.spotLight = deathLight;

        stunBirdMaterial.SetFloat(Alpha, 0.0f);

        mainCamera = Camera.main;
    }

    private void Update()
    {

        stunGracePeriod -= Time.deltaTime;
        
        MovePlayer();
        
        // Ground interaction
        if (onGround && !Controller.isGrounded)
            Fall();
        if (!onGround && Controller.isGrounded)
            Land();

        // Camera movement
        float cameraX = Input.GetAxis("Mouse X") * playerMouseSensitivity;
        if(alive) transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y+ cameraX, transform.eulerAngles.z);
        cameraPivot.rotation = Quaternion.Euler(cameraPivot.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z);
        

    }

    public void Die()
    {
        Animator.SetBool(IsDead, true);
        AudioManager.Instance.PlayAudio(peeweeDie);
        GameManager.playerAttacker.enabled = false;
        CanMove = false;
        alive = false;
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

        // Multiple velocity based on current speed percentage
        velocity *= playerSpeedPercentage;

        // Increae gravity s[eed
        if (!onGround)
            ySpeed -= playerGravity * Time.deltaTime;
        // Apply gravity
        velocity.y = GetGravity();

        // Apply any external velocity from other
        // scripts
        velocity += externalVelocity;
        
        Controller.Move(velocity * Time.deltaTime);

        // Initialize external velocity
        externalVelocity = Vector3.zero;
    }
    
    // Return vector based on player input
    private Vector3 GetPlayerMovement()
    {
        // If the player is stunned, there is no movement
        if (PlayerState == PlayerStates.Stunned)
            return Vector3.zero;
        
        // Grab current input direction
        Vector3 moveDirection = GetMovementDirection();
        
        // If they're sliding, force a forward movement
        if (IsSprinting)
            moveDirection = transform.forward.normalized;

        if(moveDirection != Vector3.zero && PlayerState == PlayerStates.Idle)
            StartWalking(); 
        if(moveDirection == Vector3.zero && (PlayerState == PlayerStates.Walking || PlayerState == PlayerStates.Idle))
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

    private float GetGravity()
    {
        if (PlayerState is PlayerStates.Jumping or PlayerStates.Stunned)
            return 0.0f;

        return ySpeed;
    }


    // Returns multiplier based on the players use of sprinting
    private float GetSprintSpeed()
    {
        // If the player is currently charging dont sprint
        if (PlayerState is PlayerStates.Charging or PlayerStates.Dodging)
            return 1;
        
        float cooldown = sprintCooldown / playerSprintDelay;
        
        // Start sprinting if the key is down and theres no cooldown
        if (!IsSprinting && Input.GetKey(KeyCode.LeftShift) && cooldown <= 0)
            StartSprinting();
        if(IsSprinting && sprintProgress >= 1 && !Input.GetKey(KeyCode.LeftShift))
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
        PlayerCamera.ChangeFOV(75,1);
    }

    private void StopSprinting()
    {
        PlayerState = PlayerStates.Idle;
        IsSprinting = false;
        Animator.SetBool(isSprinting, false);
        
        currentSprintDuration = 0;
        sprintCooldown = playerSprintDelay;
        
        PlayerCamera.ChangeFOV(-1,0.25f);

    }
    
    public void StartStun(Vector3 stunDirection, float stunDistance, float overwriteDuration = 0.0f)
    {
        // Dont stun if immune mode
        if (immune)
            return;
        
        // Dont stun again if already stunned
        if (PlayerState == PlayerStates.Stunned)
            return;
        
        if(PlayerState == PlayerStates.Charging)
            GameManager.playerAttacker.CancelCharge();
        
        if(PlayerState == PlayerStates.Jumping)
            GameManager.playerAttacker.CancelJumpCharge();

        PlayerState = PlayerStates.Stunned;
        
        PlayerCamera.ShakeCamera(10,10,0.5f);

        Animator.SetBool(isStunned, true);
        
        OnStun?.Invoke();
        OnStunTime?.Invoke(overwriteDuration > 0.0 ? overwriteDuration : defaultStunDuration);
        
        // Spawn birds
        DOVirtual.Float(0.0f, 1.0f, 0.25f, value => stunBirdMaterial.SetFloat(Alpha, value));

        
        StartCoroutine(Stunned(stunDirection, stunDistance, overwriteDuration > 0.0 ? overwriteDuration : defaultStunDuration));


    }

    private void StopStun()
    {
        PlayerState = PlayerStates.Idle;
        Animator.SetBool(isStunned, false);

        stunGracePeriod = stunGracePeriodLength;
    }

    private IEnumerator Stunned(Vector3 stunDirection, float stunDistance, float stunDuration)
    {
        bool birds = false;
        stunTime = 0.0f;
        while (stunTime / stunDuration < 1)
        {
            float knockbackT = stunTime / stunKnockbackDuration;
            float currentSpeed = Mathf.Lerp(stunDistance, 0.0f, stunCurve.Evaluate(knockbackT));
            AddExternalVelocity(stunDirection * currentSpeed);
            
            stunTime += Time.deltaTime;
            
            // Start fading out birds
            if (stunTime / (stunDuration - stunBirdFadeOutTime) >= 1 && !birds)
            {
                DOVirtual.Float(1.0f, 0.0f, stunBirdFadeOutTime, value => stunBirdMaterial.SetFloat(Alpha, value)).SetEase(Ease.InSine);
                birds = true;
            }

            yield return null;
        }

        StopStun();
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

    public bool CanBeAttacked()
    {
        if (PlayerState == PlayerStates.Stunned)
            return false;

        if (IsJumping)
            return false;
        

        if (IsCharging)
            return false;

        if (stunGracePeriod > 0)
            return false;

        return true;
    }


    #region  JUMP_LOGIC
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
        ySpeed = -playerGravity * Time.deltaTime;
        
        OnLand?.Invoke();
    }
    #endregion
    
}
