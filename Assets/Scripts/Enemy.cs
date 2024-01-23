using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Rigidbody rb;
    private CapsuleCollider cc;
    public bool Alive { get;private set; }

    public enum EnemyTypes
    {
        Default,
        Weapon
    }

    [SerializeField] private EnemyTypes enemyType;
    [SerializeField] private float timeReward;



    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cc = GetComponent<CapsuleCollider>();

        Alive = true;
    }

    public void Kill(Vector3 hitDirection, float hitPower, float hitHeight, int comboStage)
    {
        rb.AddExplosionForce(hitPower,(transform.position-hitDirection),20, hitHeight, ForceMode.Impulse);
        cc.isTrigger = true;

        Alive = false;

        GameManager.timeManager.AddTime(timeReward);

    }
}
