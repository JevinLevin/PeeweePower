using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAttacker : GenericAttacker<PlayerController>
{
    private BoxCollider bc;

    private float attackCooldownMax = 1;
    private float attackLength = 1;
    private float playerStunDuration;
    private float[] attackImpacts;
    private Enemy enemyScript;

    private float attackTime;
    private float attackCooldown;

    private bool attacking;
    private bool attacked;
    private static readonly int IsAttacking = Animator.StringToHash("isAttacking");

    private void Awake()
    {
        enemyScript = GetComponentInParent<Enemy>();
        bc = GetComponent<BoxCollider>();
    }

    private void Start()
    {
        attackCooldownMax = enemyScript.Stats.attackCooldownMax;
        attackLength = enemyScript.Stats.attackLength;
        attackImpacts = enemyScript.Stats.attackImpacts;
        bc.size = enemyScript.Stats.bc.size;
        bc.center = enemyScript.Stats.bc.center;
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
        foreach (PlayerController player in targetsInRange.Where(stun => stun.CanBeAttacked() && stun.PlayerState != PlayerController.PlayerStates.Dodging))
        {
            player.StartStun(transform.forward);
            enemyScript.ChaseCooldown(playerStunDuration);
        }
    }

    private IEnumerator StartAttack()
    {
        attacking = true;
        enemyScript.Animator.SetBool(IsAttacking, true);
        int attackIndex = 0;
        enemyScript.SetSpeed(0);

        attackTime = 0.0f;
        float t;
        
        while ((t = attackTime / attackLength) < 1)
        {
            if (!attacked && attackIndex < attackImpacts.Length && t > attackImpacts[attackIndex])
            {
                Attack();
                attackIndex++;
            }

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
        enemyScript.SetSpeed(-1);


        attackCooldown = attackCooldownMax;
    }
    
    
}
