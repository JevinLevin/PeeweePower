using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerAttacker : GenericAttacker<Enemy>
{
    private PlayerController player;

    [SerializeField] private AudioSource audioSource;

    
    [Header("Attack")]
    [SerializeField] private float hitPower;
    [SerializeField] private float hitHeight;
    [SerializeField] private float hitLength;
    [Range(0,1)]
    [SerializeField] private float hitImpactPercent;
    [SerializeField] private float hitCooldown = 0.25f;
    [SerializeField] private float hitBufferMax = 0.3f;
    private float hitBuffer;
    private bool attacking;
    private bool hasAttacked;
    private float attackingDuration;
    private float attackingCooldown;

    [Header("Combo")]
    [SerializeField] private float comboMaxWait;
    [SerializeField] private ComboDisplay comboDisplay;
    private int comboReady;
    private int totalComboStage;
    private int currentComboStage;
    private float comboTime;

    [Header("Charge")] 
    [SerializeField] private float chargeUpAnimationSpeed = 1.35f;
    [SerializeField] private float chargeUpDuration;
    [SerializeField] private float chargeDuration;
    [SerializeField] private float chargePower;
    [SerializeField] private AnimationCurve chargeCurve;
    [SerializeField] private float chargeAnimationSpeed = 1.5f;
    [SerializeField] private float chargeHitPower;
    [SerializeField] private float chargeHitHeight;
    [SerializeField] private float chargePlayerSpeedMin = 0.25f;
    [SerializeField] private float chargeCooldownMax = 0.25f;
    [SerializeField] private CanvasGroup chargeReadyTint;
    [SerializeField] private AudioClip peeweeChargeStart;
    [SerializeField] private AudioClip peeweeChargeEnd;
    private float chargeCooldown;
    private float chargeUpTime;
    private float chargeTime;
    private bool chargeCharging;
    private bool chargeReady;
    private bool chargeActive;
    private Coroutine chargeCoroutine;
    private readonly int isAttacking = Animator.StringToHash("isAttacking");
    private readonly int comboStage = Animator.StringToHash("comboStage");
    private readonly int isCharging = Animator.StringToHash("isCharging");
    private readonly int chargeSpeed = Animator.StringToHash("chargeSpeed");
    private readonly int isDodging = Animator.StringToHash("isDodging");
    private readonly int walkSpeed = Animator.StringToHash("walkSpeed");

    [Header("Jump")] 
    [SerializeField] private int jumpComboRequirement = 10;
    [SerializeField] private float jumpChargeUpDuration = 0.5f;
    [SerializeField] private float jumpChargeUpSpeedReduction = 0.1f;
    [SerializeField] private float jumpChargeUpSpeedReductionLength = 0.25f;
    [SerializeField] private float jumpUpPower = 50;
    [SerializeField] private float jumpUpDuration = 1.5f;
    [SerializeField] private AnimationCurve jumpUpCurve;
    [SerializeField] private float jumpDownPower = 25;
    [SerializeField] private float jumpDownAcceleration = 0.25f;
    [SerializeField] private float jumpSlamDelay = 0.5f;
    [SerializeField] private float jumpCooldownMax = 3;
    [SerializeField] private float jumpSlamRange = 10;
    [SerializeField] private float jumpSlamPower = 25;
    [SerializeField] private float jumpSlamHeight = 2;
    [SerializeField] private LayerMask slamLayerMask;

    private bool canSlam;
    private bool jumpCharging;
    private bool jumpCharged;
    private float jumpChargeUpTime;
    private float jumpCooldown;
    private Coroutine jumpUpCoroutine;
    private Coroutine jumpDownCoroutine;
    
    [Header("Dodge")] 
    [SerializeField] private float dodgeBufferMax = 0.25f;
    [SerializeField] private float dodgeDurationMax = 0.25f;
    [SerializeField] private float dodgePlayerSpeed = 0.5f;
    [SerializeField] private float dodgeCooldownMax = 0.5f;
    private float dodgeBuffer;
    private float dodgeDuration;
    private float dodgeCooldown;

    private bool inputAttack;
    private bool inputCharge;
    private bool inputDodge;
    private bool inputJump;
    private bool inputSlam;
    private static readonly int IsJumping = Animator.StringToHash("isJumping");


    private void Awake()
    {
        player = GetComponentInParent<PlayerController>();
        player.OnStun += ResetCombo;
        chargeReadyTint.alpha = 0.0f;
    }

    private void OnEnable()
    {
        GameManager.playerAttacker = this;
    }

    private void Update()
    {
        // Check inputs
        inputAttack = Input.GetMouseButtonDown(0);
        inputCharge = Input.GetMouseButton(1);
        inputDodge = Input.GetKeyDown(KeyCode.LeftControl);
        inputJump = Input.GetKey(KeyCode.Space);
        inputSlam = Input.GetMouseButtonDown(0);
        
        
        // Track if the player is currently in a combo
        if(player.PlayerState is not PlayerController.PlayerStates.Charging and PlayerController.PlayerStates.Jumping && !jumpCharging && !jumpCharged) comboTime -= Time.deltaTime;
        // Reset combo if timer runs out
        if (comboTime <= 0)
            ResetCombo();
        
        // Hit buffer
        hitBuffer -= Time.deltaTime;
        if (inputAttack && !chargeActive)
            hitBuffer = hitBufferMax;
        
        CheckAttack();
        
        CheckCharge();
        
        CheckDodge();

        CheckJump();
    }

    private void CheckJump()
    {
        jumpCooldown -= Time.deltaTime;
        
        
        if(inputJump && CanAttack() && CanJump())
            StartJumpCharge();
        if(!inputJump && !player.IsJumping && jumpCharging)
            CancelJumpCharge();

        if (jumpCharging)
        {
            jumpChargeUpTime += Time.deltaTime;
            player.AdjustPlayerSpeed(Mathf.Lerp(1.0f,jumpChargeUpSpeedReduction, jumpChargeUpTime/jumpChargeUpSpeedReductionLength));
        }
        
        if(jumpChargeUpTime >= jumpChargeUpDuration && !jumpCharged)
            JumpReady();
        
        if (jumpCharged && !inputJump && jumpChargeUpTime >= jumpChargeUpDuration)
            StartJump();

        
        // If they attack while in the air slam
        if(inputSlam && canSlam)
            JumpStartSlam();
    }


    private void StartJumpCharge()
    {
        jumpCharging = true;
        player.PlayerState = PlayerController.PlayerStates.Jumping;
        player.Animator.SetBool(IsJumping, true);
        PlayerCamera.ChangeFOV(40, 1f);
        PlayerCamera.ShakeCameraIn(3,1,jumpChargeUpDuration);
    }

    public void CancelJumpCharge()
    {
        jumpCharging = false;
        jumpCharged = false;
        jumpChargeUpTime = 0.0f;
        chargeReadyTint.DOFade(0.0f, 1.0f);
        GameManager.Instance.actionTextDisplay.StopDisplay();
        PlayerCamera.ShakeCameraInStop();
        EndJump();
    }

    private void JumpReady()
    {
        chargeReadyTint.DOFade(1.0f, 0.1f);
        GameManager.Instance.actionTextDisplay.StartDisplay("RELEASE" ,0.25f);
        jumpCharging = false;
        jumpCharged = true;

    }

    private void EndJump()
    {
        player.PlayerState = PlayerController.PlayerStates.Idle;
        player.Animator.SetBool(IsJumping, false);

        jumpDownCoroutine = null;
        jumpUpCoroutine = null;
        
        player.IsJumping = false;
        player.OnLand -= JumpSlam;

        player.AdjustPlayerSpeed(1.0f);


        jumpCooldown = jumpCooldownMax;
        
        player.Controller.excludeLayers = 0;
        
        PlayerCamera.ChangeFOV(-1, 0.25f);

    }
    private void StartJump()
    {
        canSlam = false;
        player.IsJumping = true;
        jumpCharging = false;
        jumpCharged = false;
        jumpChargeUpTime = 0.0f;

        jumpUpCoroutine = StartCoroutine(JumpUpVelocity());
        
        chargeReadyTint.DOFade(0.0f, 1.0f);
        GameManager.Instance.actionTextDisplay.StopDisplay();
        
        UseCombo();

        player.OnLand += JumpSlam;
        
        PlayerCamera.ChangeFOV(100, 0.25f);
        PlayerCamera.ShakeCameraInStop();
    }

    private IEnumerator JumpUpVelocity()
    {
        float jumpUpTime = 0.0f;
        float t;
        while ( (t = jumpUpTime / jumpUpDuration) < 1)
        {
            jumpUpTime += Time.deltaTime;

            if (jumpUpTime > jumpSlamDelay && !canSlam)
            {
                canSlam = true;
                GameManager.Instance.actionTextDisplay.StartDisplay("ATTACK", 0.15f);
            }

            player.AddExternalVelocity(player.transform.up * Mathf.Lerp(jumpUpPower,0,jumpUpCurve.Evaluate(t)));
            
            yield return null;
        }
        
        JumpStartSlam();
    }

    private void JumpStartSlam()
    {
        if(jumpUpCoroutine != null) StopCoroutine(jumpUpCoroutine);
        jumpDownCoroutine = StartCoroutine(JumpSlamVelocity());
        GameManager.Instance.actionTextDisplay.StopDisplay();
        canSlam = false;
        player.Controller.excludeLayers = slamLayerMask;
    }
    
    private IEnumerator JumpSlamVelocity()
    {
        float jumpDownTime = 0.0f;
        while (true)
        {
            jumpDownTime += Time.deltaTime;
            float fallSpeed = Easing.easeInQuad(jumpDownTime / jumpDownAcceleration);
            player.AddExternalVelocity(-player.transform.up * Mathf.LerpUnclamped(0, jumpDownPower,  fallSpeed));
            
            yield return null;
        }
        // ReSharper disable once IteratorNeverReturns
    }

    private void JumpSlam()
    {
        StopCoroutine(jumpDownCoroutine);

        List<Enemy> targets = GameManager.enemySpawner.GetEnemiesInRange(transform.position, jumpSlamRange);
        foreach (Enemy target in targets)
            target.Kill((target.transform.position-transform.position).normalized, jumpSlamPower, jumpSlamHeight);
        
        List<Granny> grannies = GameManager.enemySpawner.GetGrandmasInRange(transform.position, jumpSlamRange);
        foreach (Granny granny in grannies)
            granny.StartStun(false); 

        PlayerCamera.ShakeCamera(25,10,0.5f);

        EndJump();

    }
    
    private void CheckDodge()
    {
        dodgeBuffer -= Time.deltaTime;
        if (inputDodge)
            dodgeBuffer = dodgeBufferMax;
        
        // Dodge cooldown
        dodgeCooldown -= Time.deltaTime;

        // Dont check for dodge during the cooldown
        if (dodgeCooldown > 0)
            return;

        // If the player clicks and is idle/walking
        if (dodgeBuffer > 0 && (player.PlayerState == PlayerController.PlayerStates.Idle || player.PlayerState == PlayerController.PlayerStates.Walking))
            StartDodge();

        if (player.PlayerState == PlayerController.PlayerStates.Dodging)
            Dodging();
    }

    private void Dodging()
    {
        dodgeDuration += Time.deltaTime;
        
        if(dodgeDuration / dodgeDurationMax > 1)
            EndDodge();
    }

    private void StartDodge()
    {
        player.PlayerState = PlayerController.PlayerStates.Dodging;
        player.Animator.SetBool(isDodging, true);
        player.Animator.SetFloat(walkSpeed, dodgePlayerSpeed);
        player.AdjustPlayerSpeed(dodgePlayerSpeed);
        
        dodgeBuffer = 0.0f;
    }

    private void EndDodge()
    {
        player.PlayerState = PlayerController.PlayerStates.Idle;
        player.Animator.SetBool(isDodging, false);
        player.Animator.SetFloat(walkSpeed, 1f);
        player.AdjustPlayerSpeed(1f);

        dodgeDuration = 0.0f;
        dodgeCooldown = dodgeCooldownMax;
    }

    private void CheckAttack()
    {
        // Attack cooldown
        attackingCooldown -= Time.deltaTime;

        // Dont check for attacks during the cooldown
        if (attackingCooldown > 0)
            return;

        // If the player attacks without charging
        if ( !attacking && CanAttack() && hitBuffer > 0 && !chargeReady)
            StartAttack();
        if (attacking)
            Attacking();
    }

    private void StartAttack()
    {
        player.Animator.SetBool(isAttacking, true);
        attacking = true;
        hasAttacked = false;
        attackingDuration = 0.0f;
        hitBuffer = 0.0f;
        
    }

    private void EndAttack()
    {
        player.Animator.SetBool(isAttacking, false);
        attacking = false;
        attackingDuration = 0.0f;
        
        if(!hasAttacked)
            ResetCombo();

        attackingCooldown = hitCooldown;
    }

    private void Attacking()
    {
        attackingDuration += Time.deltaTime;

        float attackProgress = attackingDuration / hitLength;
        // Allow the attack to actually impact at the set percent
        if(!hasAttacked && attackProgress > hitImpactPercent)
            Attack();
        
        if(attackProgress >= 1)
            EndAttack();
    }

    private void Attack()
    {
        if (CanHitEnemy())
            return;

        hasAttacked = true;
        
        PlayerCamera.ShakeCamera(3,5,0.2f);

        player.IsAttacking = true;

        AddCombo();


        // Calculate hit power
        float currentHitPower = hitPower;
        float currentHitHeight = hitHeight;
        Vector3 hitDirection = Vector3.zero;
        // Calculate hit direcdtion based on combo
        switch (totalComboStage % 3)
        {
            case 1:
                hitDirection = transform.right;
                break;
            case 2:
                hitDirection = -transform.right;
                break;
            case 0:
                hitDirection = -transform.up + transform.forward / 2;
                currentHitPower *= 1.25f;
                currentHitHeight *= 2f;
                break;
        }

        // Hit all alive enemies in the hit range
        foreach (Enemy enemy in targetsInRange.Where(alive => alive.Alive))
            enemy.Kill(hitDirection, currentHitPower, currentHitHeight);

        // Reset combo after max
        //if (currentComboStage >= 3)
        //    currentComboStage = 0;
    }

    private void CheckCharge()
    {
        // Attack cooldown
        chargeCooldown -= Time.deltaTime;
        
        // Dont check for chare during the cooldown
        if (chargeCooldown > 0)
            return;

        // Dont check for charge mid charge
        if (chargeCharging)
            return;
        
        // If the player attacks after charging
        if(chargeReady && !inputCharge)
            chargeCoroutine = StartCoroutine(PlayChargeAttack());


        // If the player is holding down the mouse and can attack/charge
        if (inputCharge && CanAttack() && CanCharge())
        {
            if(!chargeActive)
                StartCharge();
            
            // Count duration
            chargeUpTime += Time.deltaTime;
            // Reduce player speed over time
            // Also clamps the value so it cant go below the min
            player.AdjustPlayerSpeed(Mathf.Clamp(1 - chargeUpTime / chargeUpDuration, chargePlayerSpeedMin, 1));
        }
        
        // If the player lets go of the attack while charging up
        if(chargeActive && ((!inputCharge && !chargeCharging) || !CanAttack()))
            CancelCharge();

        // If the player fully charges
        if (chargeUpTime >= chargeUpDuration && !chargeReady)
            ReadyCharge();
    }

    private void ChargeAttack()
    {
        if (CanHitEnemy())
            return;

        PlayerCamera.ShakeCamera(5,2,0.2f);
        
        comboTime = comboMaxWait;

        // Hit all alive enemies in the hit range
        foreach (Enemy enemy in targetsInRange.Where(alive => alive.Alive))
        {
            // Randomise the hit direction (dont try to comprehend this clusterfuck of code)
            Vector3 randomDirection = Random.Range(0, 2) == 0 ? -transform.right : transform.right;
            float randomMultiplier = Random.Range(1.5f, 3.0f);
            randomDirection /= randomMultiplier * Random.Range(0,2) == 1 ? -1 : 1;

            // Kill enemies
            enemy.Kill(transform.forward*2 + randomDirection, chargeHitPower, chargeHitHeight);
        }

    }

    private void StartCharge()
    {
        chargeActive = true;
        audioSource.PlayOneShot(peeweeChargeStart);
        player.PlayerState = PlayerController.PlayerStates.Charging;
        player.Animator.SetBool(isCharging,true);
        PlayerCamera.ChangeFOV(40, 2f);
        player.Animator.SetFloat(chargeSpeed, chargeUpAnimationSpeed);
        PlayerCamera.ShakeCameraIn(3,1,chargeUpDuration);
    }

    public void CancelCharge()
    {
        audioSource.Stop();
        player.AdjustPlayerSpeed(1);
        chargeReadyTint.DOFade(0.0f, 1.0f);
        PlayerCamera.ChangeFOV(-1, 0.25f);
        PlayerCamera.ShakeCameraInStop();
        EndCharge();
    }

    private void ReadyCharge()
    {
        chargeReadyTint.DOFade(1.0f, 0.1f);
        chargeReady = true;
        player.Animator.SetFloat(chargeSpeed, 0);
    }

    private void EndCharge()
    {
        player.PlayerState = PlayerController.PlayerStates.Idle;
        player.Animator.SetBool(isCharging,false);
        player.Animator.SetFloat(chargeSpeed, 1);
        chargeCharging = false;
        player.IsCharging = false;
        chargeReady = false;
        chargeActive = false;
        chargeUpTime = 0.0f;
        chargeCooldown = chargeCooldownMax;
    }

    public void EndMidCharge()
    {
        StopCoroutine(chargeCoroutine);
        EndCharge();
    }

    private IEnumerator PlayChargeAttack()
    {
        player.Animator.SetFloat(chargeSpeed, chargeAnimationSpeed);

        // Initialise
        chargeCharging = true;
        player.IsCharging = true;
        
        chargeReadyTint.DOFade(0.0f, 1.0f);
        PlayerCamera.ChangeFOV(-1, 0.25f);
        PlayerCamera.ShakeCameraInStop();

        UseCombo();

        player.AdjustPlayerSpeed(1);
        
        audioSource.PlayOneShot(peeweeChargeEnd);
        
        Vector3 startingChargeVelocity = new Vector3(transform.forward.x,0,transform.forward.z) * chargePower;
        chargeTime = 0.0f;
        float t = 0.0f;

        // Charge logic
        while ((t = chargeTime / chargeDuration) < 1)
        {
            chargeTime += Time.deltaTime;

            // Increase forward velocity based on time
            var chargeVelocity = Vector3.Lerp(startingChargeVelocity, Vector3.zero, chargeCurve.Evaluate(t));
            player.AddExternalVelocity(chargeVelocity);
            
            
            // Attack anything in range
            ChargeAttack();

            yield return null;
        }

        EndCharge();
        
    }

    private void ResetCombo()
    {
        totalComboStage = 0;
        currentComboStage = 0;
        comboDisplay.LoseCombo();
    }

    private void AddCombo()
    {
        
        totalComboStage++;
        currentComboStage++;
        comboTime = comboMaxWait;
        
        // Start combo
        if(totalComboStage == 1)
            comboDisplay.StartCombo();
        
        // Ready charge
        if(currentComboStage == 3)
            comboDisplay.ChargeComboReady();
        
        // Ready jump
        if(currentComboStage == jumpComboRequirement)
            comboDisplay.JumpComboReady();
        
        player.Animator.SetInteger(comboStage, totalComboStage%3+1);
        
        comboDisplay.ChangeCombo(totalComboStage);
    }

    private void UseCombo()
    {
        currentComboStage = 0;
        comboDisplay.ComboUnready();
    }
    
    // Checks if there are any enemies in range
    private bool CanHitEnemy()
    {
        return targetsInRange.Count <= 0;
    }

    private bool CanAttack()
    {
        if (player.PlayerState == PlayerController.PlayerStates.Sprinting)
            return false;
        
        if (player.PlayerState == PlayerController.PlayerStates.Stunned)
            return false;
        
        if (player.PlayerState == PlayerController.PlayerStates.Dodging)
            return false;
        
        if (player.PlayerState == PlayerController.PlayerStates.Jumping)
            return false;

        if (attacking)
            return false;
        
        return true;
    }

    private bool CanCharge()
    {
        if (currentComboStage < 3)
            return false;

        if (chargeCharging)
            return false;

        return true;
    }
    
    private bool CanJump()
    {
        if (currentComboStage < jumpComboRequirement)
            return false;

        if (player.IsJumping || jumpCharging)
            return false;

        if (jumpCooldown > 0)
            return false;
        
        return true;
    }

    public int GetCombo()
    {
        return totalComboStage;
    }
}
