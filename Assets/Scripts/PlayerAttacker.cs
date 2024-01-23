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
    private float chargeUpTime;
    private float chargeTime;
    private bool charging;
    private bool charged;
    private bool chargeup;
    public Action<Vector3> OnAddVelocity;
    public Action<float> OnChangeSpeed;


    private void OnTriggerEnter(Collider other)
    {
        // Dont collider with parent

        if (other.TryGetComponent(out Enemy enemyScript))
            enemiesInRange.Add(enemyScript);

    }

    private void OnTriggerExit(Collider other)
    {

        if (other.TryGetComponent(out Enemy enemyScript))
            enemiesInRange.Remove(enemyScript);

    }


    private void Update()
    {
        comboTime -= Time.deltaTime;
        if (comboTime <= 0)
            comboStage = 0;

        if (Input.GetMouseButtonUp(0) && !charged)
                Attack();
        if(Input.GetMouseButtonUp(0) && charged)
            StartCoroutine(PlayChargeAttack());

        if (charging)
            ChargeAttack();


        if (Input.GetMouseButton(0) && !charging)
        {
            chargeUpTime += Time.deltaTime;
            chargeup = true;
            OnChangeSpeed?.Invoke(Mathf.Clamp(1 - chargeUpTime / chargeUpDuration, 0.25f, 1));
        }
        if(chargeup & !Input.GetMouseButton(0) && !charging)
        {
            chargeUpTime = 0;
            charged = false;
            chargeup = false;
            OnChangeSpeed?.Invoke(1);
        }

        if (chargeUpTime >= chargeUpDuration)
            charged = true;
    }

    private void Attack()
    {
        if (enemiesInRange.Count <= 0)
            return;

        PlayerCamera.ShakeCamera();

        comboTime = comboMaxWait;

        comboStage++;


        // Calculate hit power
        float currentHitPower = hitPower;
        float currentHitHeight = hitHeight;
        Vector3 hitDirection = Vector3.zero;
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

        foreach (Enemy enemy in enemiesInRange.Where(alive => alive.Alive))
            enemy.Kill(hitDirection, currentHitPower, currentHitHeight, comboStage);

        if (comboStage >= 3)
            comboStage = 0;
    }

    private void ChargeAttack()
    {
        if (enemiesInRange.Count <= 0)
            return;

        PlayerCamera.ShakeCamera();

        foreach (Enemy enemy in enemiesInRange.Where(alive => alive.Alive))
        {
            Vector3 randomDirection = Random.Range(0, 2) == 0 ? -transform.right : transform.right;
            float randomMultiplier = Random.Range(1.5f, 3.0f);
            randomDirection /= randomMultiplier * Random.Range(0,2) == 1 ? -1 : 1;
            enemy.Kill(transform.forward*2 + randomDirection, chargeHitPower, chargeHitHeight, comboStage);
        }

    }

    private IEnumerator PlayChargeAttack()
    {
        charging = true;
        OnChangeSpeed?.Invoke(1);

        Vector3 startingChargeVelocity = new Vector3(transform.forward.x,0,transform.forward.z) * chargePower;
        Vector3 chargeVelocity = startingChargeVelocity;
        chargeTime = 0.0f;
        float t = 0.0f;

        while ((t = chargeTime / chargeDuration) < 1)
        {
            chargeTime += Time.deltaTime;

            chargeVelocity = Vector3.Lerp(startingChargeVelocity, Vector3.zero, chargeCurve.Evaluate(t));
            OnAddVelocity(chargeVelocity);

            yield return null;
        }

        charging = false;
        charged = false;
        chargeup = false;
        chargeUpTime = 0.0f;
        
    }
}
