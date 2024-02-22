using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyStats : MonoBehaviour
{
    public float speed = 4;
    public float attackCooldownMax = 1;
    public float attackLength = 1.25f;
    public float attackKnockback = 25;
    public float playerStunDuration = 0.0f;
    public float[] attackImpacts;
    public BoxCollider bc;

    private void Awake()
    {
        bc = GetComponent<BoxCollider>();
    }
}
