using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        if (Input.GetMouseButtonDown(0))
            Attack();
    }

    private void Attack()
    {
        if (enemiesInRange.Count <= 0)
            return;

        PlayerCamera.ShakeCamera();

        comboTime = comboMaxWait;

        comboStage++;

        foreach (Enemy enemy in enemiesInRange)
            enemy.Kill(transform, hitPower, hitHeight, comboStage);

        if (comboStage >= 3)
            comboStage = 0;
    }
}
