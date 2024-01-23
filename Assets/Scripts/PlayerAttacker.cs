using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.Image;
using Random = UnityEngine.Random;

public class PlayerAttacker : MonoBehaviour
{
    private List<Enemy> enemiesInRange = new();

    [Header("Config")]
    [SerializeField] private float hitPower;
    [SerializeField] private float hitHeight;

    [Header("Combo")]
    private int comboStage;
    private float comboTime;
    [SerializeField] private float comboMaxWait;

    [Header("Charge")]
    [SerializeField] private float chargeUpDuration;
    [SerializeField] private float chargeDuration;
    [SerializeField] private float chargePower;
    [SerializeField] private AnimationCurve chargeCurve;
    [SerializeField] private float chargeHitPower;
    [SerializeField] private float chargeHitHeight;
    [SerializeField] private float chargePlayerSpeedMin = 0.25f;
    private float chargeUpTime;
    private float chargeTime;
    private bool charging;
    private bool charged;
    private bool chargeup;
    public Action<Vector3> OnAddVelocity;
    public Action<float> OnChangeSpeed;


    private void OnTriggerEnter(Collider other)
    {
        // If enemy enters attack range, add to list
        if (other.TryGetComponent(out Enemy enemyScript))
            enemiesInRange.Add(enemyScript);

    }

    private void OnTriggerExit(Collider other)
    {
        // Remove enemy from list if they leave range
        if (other.TryGetComponent(out Enemy enemyScript))
            enemiesInRange.Remove(enemyScript);

    }


    private void Update()
    {
        // Track if the player is currently in a combo
        comboTime -= Time.deltaTime;
        // Reset combo if timer runs out
        if (comboTime <= 0)
            comboStage = 0;

        // If the player lets go of attack without charging
        if (Input.GetMouseButtonUp(0) && !charged)
                Attack();
        // If the player lets go of attack after charging
        if(Input.GetMouseButtonUp(0) && charged)
            StartCoroutine(PlayChargeAttack());

        // Attack anything in range while charging
        if (charging)
            ChargeAttack();

        // If the player is holdijng down the mouse
        if (Input.GetMouseButton(0) && !charging)
        {
            // Count duration
            chargeUpTime += Time.deltaTime;
            chargeup = true;
            // Reduce player speed over time
            OnChangeSpeed?.Invoke(Mathf.Clamp(1 - chargeUpTime / chargeUpDuration, chargePlayerSpeedMin, 1));
        }
        // If the player lets go of the attack while charging up
        if(chargeup & !Input.GetMouseButton(0) && !charging)
        {
            // Reset
            chargeUpTime = 0;
            charged = false;
            chargeup = false;
            OnChangeSpeed?.Invoke(1);
        }

        // If the player fully charges
        if (chargeUpTime >= chargeUpDuration)
            charged = true;
    }

    private void Attack()
    {
        if (CanHitEnemy())
            return;

        // Doesnt currently work
        PlayerCamera.ShakeCamera();

        // Combo tracker
        comboTime = comboMaxWait;
        comboStage++;


        // Calculate hit power
        float currentHitPower = hitPower;
        float currentHitHeight = hitHeight;
        Vector3 hitDirection = Vector3.zero;
        // Calculate hit direcdtion based on combo
        switch (comboStage)
        {
            case 1:
                hitDirection = transform.right;
                break;
            case 2:
                hitDirection = -transform.right;
                break;
            case 3:
                hitDirection = -transform.up + transform.forward / 2;
                currentHitPower *= 1.25f;
                currentHitHeight *= 2f;
                break;
        }

        // Hit all alive enemies in the hit range
        foreach (Enemy enemy in enemiesInRange.Where(alive => alive.Alive))
            enemy.Kill(hitDirection, currentHitPower, currentHitHeight, comboStage);

        // Reset combo after max
        if (comboStage >= 3)
            comboStage = 0;
    }

    private void ChargeAttack()
    {
        if (CanHitEnemy())
            return;

        PlayerCamera.ShakeCamera();

        // Hit all alive enemies in the hit range
        foreach (Enemy enemy in enemiesInRange.Where(alive => alive.Alive))
        {
            // Randomise the hit direction (dont try to comprehend this clusterfuck of code)
            Vector3 randomDirection = Random.Range(0, 2) == 0 ? -transform.right : transform.right;
            float randomMultiplier = Random.Range(1.5f, 3.0f);
            randomDirection /= randomMultiplier * Random.Range(0,2) == 1 ? -1 : 1;

            // Kill enemies
            enemy.Kill(transform.forward*2 + randomDirection, chargeHitPower, chargeHitHeight, comboStage);
        }

    }

    // Checks if there are any enemies in range
    private bool CanHitEnemy()
    {
        return enemiesInRange.Count <= 0;
    }

    private IEnumerator PlayChargeAttack()
    {
        // Initialise
        charging = true;
        OnChangeSpeed?.Invoke(1);
        Vector3 startingChargeVelocity = new Vector3(transform.forward.x,0,transform.forward.z) * chargePower;
        Vector3 chargeVelocity = startingChargeVelocity;
        chargeTime = 0.0f;
        float t = 0.0f;

        // Charge logic
        while ((t = chargeTime / chargeDuration) < 1)
        {
            chargeTime += Time.deltaTime;

            // Increase forward velocity based on time
            chargeVelocity = Vector3.Lerp(startingChargeVelocity, Vector3.zero, chargeCurve.Evaluate(t));
            OnAddVelocity(chargeVelocity);

            yield return null;
        }

        // Reset
        charging = false;
        charged = false;
        chargeup = false;
        chargeUpTime = 0.0f;
        
    }
}
