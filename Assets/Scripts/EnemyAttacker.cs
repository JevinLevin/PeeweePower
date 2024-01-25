using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAttacker : GenericAttacker<PlayerController>
{

    [SerializeField] private float attackCooldownMax = 1;
    [SerializeField] private float attackLength = 1;
    [Range(0,1)][SerializeField] private float attackImpactPercent = 0.5f;
    private Enemy enemyScript;

    private float attackTime;
    private float attackCooldown;

    private bool attacking;
    private bool attacked;
    private static readonly int IsAttacking = Animator.StringToHash("isAttacking");

    private void Awake()
    {
        enemyScript = GetComponentInParent<Enemy>();
    }


    // Update is called once per frame
    void Update()
    {
        attackCooldown -= Time.deltaTime;
        
        if (!attacking && attackCooldown < 0 && enemyScript.InRange)
            StartCoroutine(StartAttack());

    }
    
    private void Attack()
    {
        attacked = true;
        
        // Hit all alive enemies in the hit range
        foreach (PlayerController player in targetsInRange.Where(stun => stun.PlayerState != PlayerController.PlayerStates.Stunned))
            player.StartStun(transform.forward);
    }

    private IEnumerator StartAttack()
    {
        attacking = true;
        enemyScript.Animator.SetBool(IsAttacking, true);

        attackTime = 0.0f;
        float t;
        
        while ((t = attackTime / attackLength) < 1)
        {
            if(!attacked &&  t > attackImpactPercent)
                Attack();

            attackTime += Time.deltaTime;

            yield return null;
        }
        
        StopAttack();
    }

    private void StopAttack()
    {
        attacking = false;
        attacked = false;
        enemyScript.Animator.SetBool(IsAttacking, false);

        attackCooldown = attackCooldownMax;
    }
    
}
