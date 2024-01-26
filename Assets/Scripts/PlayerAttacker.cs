using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.Image;
using Random = UnityEngine.Random;

public class PlayerAttacker : GenericAttacker<Enemy>
{
    private PlayerController player;

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
    [SerializeField] private float chargeUpDuration;
    [SerializeField] private float chargeDuration;
    [SerializeField] private float chargePower;
    [SerializeField] private AnimationCurve chargeCurve;
    [SerializeField] private float chargeHitPower;
    [SerializeField] private float chargeHitHeight;
    [SerializeField] private float chargePlayerSpeedMin = 0.25f;
    [SerializeField] private float chargeCooldownMax = 0.25f;
    [SerializeField] private float chargeStartThreshold = 0.25f;
    private float chargeStartTimer;
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

    
    [Header("Dodge")] 
    [SerializeField] private float dodgeBufferMax = 0.25f;
    [SerializeField] private float dodgeDurationMax = 0.25f;
    [SerializeField] private float dodgePlayerSpeed = 0.5f;
    [SerializeField] private float dodgeCooldownMax;
    private float dodgeBuffer;
    private float dodgeDuration;
    private float dodgeCooldown;
    

    private void Awake()
    {
        player = GetComponentInParent<PlayerController>();
        player.OnStun += ResetCombo;
    }

    private void OnEnable()
    {
        GameManager.playerAttacker = this;
    }

    private void Update()
    {
        // Track if the player is currently in a combo
        if(player.PlayerState != PlayerController.PlayerStates.Charging) comboTime -= Time.deltaTime;
        // Reset combo if timer runs out
        if (comboTime <= 0)
            ResetCombo();
        
        // Hit buffer
        hitBuffer -= Time.deltaTime;
        if (Input.GetMouseButtonUp(0) && !chargeActive)
            hitBuffer = hitBufferMax;
        
        CheckAttack();
        
        CheckCharge();
        
        CheckDodge();
    }
    
    private void CheckDodge()
    {
        dodgeBuffer -= Time.deltaTime;
        if (Input.GetMouseButtonDown(1))
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
        
        // Doesnt currently work
        PlayerCamera.ShakeCamera();

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
            enemy.Kill(hitDirection, currentHitPower, currentHitHeight, totalComboStage);

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
        if(Input.GetMouseButtonUp(0) && chargeReady)
            chargeCoroutine = StartCoroutine(PlayChargeAttack());

        if (Input.GetMouseButton(0) && CanAttack() && CanCharge())
            chargeStartTimer += Time.deltaTime;
        else
            chargeStartTimer = 0.0f;

        // If the player is holding down the mouse and can attack
        if (chargeStartTimer >= chargeStartThreshold)
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
        if(chargeActive && ((!Input.GetMouseButton(0) && !chargeCharging) || !CanAttack()))
            CancelCharge();

        // If the player fully charges
        if (chargeUpTime >= chargeUpDuration && !chargeReady)
            ReadyCharge();
    }

    private void ChargeAttack()
    {
        if (CanHitEnemy())
            return;

        PlayerCamera.ShakeCamera();

        // Hit all alive enemies in the hit range
        foreach (Enemy enemy in targetsInRange.Where(alive => alive.Alive))
        {
            // Randomise the hit direction (dont try to comprehend this clusterfuck of code)
            Vector3 randomDirection = Random.Range(0, 2) == 0 ? -transform.right : transform.right;
            float randomMultiplier = Random.Range(1.5f, 3.0f);
            randomDirection /= randomMultiplier * Random.Range(0,2) == 1 ? -1 : 1;

            // Kill enemies
            enemy.Kill(transform.forward*2 + randomDirection, chargeHitPower, chargeHitHeight, totalComboStage);
        }

    }

    private void StartCharge()
    {
        chargeActive = true;
        player.PlayerState = PlayerController.PlayerStates.Charging;
        player.Animator.SetBool(isCharging,true);
    }

    public void CancelCharge()
    {
        player.AdjustPlayerSpeed(1);
        EndCharge();
    }

    private void ReadyCharge()
    {
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
        player.Animator.SetFloat(chargeSpeed, 1.5f);

        // Initialise
        chargeCharging = true;
        player.IsCharging = true;
        
        UseCombo();

        player.AdjustPlayerSpeed(1);
        
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
            comboDisplay.ComboReady();
        
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
}
