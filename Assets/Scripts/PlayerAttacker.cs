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
        if (Input.GetMouseButtonDown(0))
            foreach(Enemy enemy in enemiesInRange)
                enemy.Kill(transform, hitPower, hitHeight);
    }
}
