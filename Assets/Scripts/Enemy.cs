using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    private Rigidbody rb;
    private CapsuleCollider cc;
    private EnemyAI ai;
    public Animator Animator { get; private set; }
    public bool Alive { get;private set; }
    public bool InRange { get; private set; }
    
    private PlayerController player;
    public Transform PlayerTransform { get; private set; }
    private bool isPlayerStunned = false;

    private Action<Enemy> OnKill;
    private Func<float> GetTimeReward;

    [SerializeField] private GameObject[] enemyTypes;
    [SerializeField] private float deathDespawnDelay = 2f; // Time before the enemy is despawned after death
    [SerializeField] private float targetRange = 25f; // Distance to the player before the enemy starts tracking them
    [SerializeField] private float inRangeRadius = 2;

    public EnemyStats Stats { get; private set; }

    public enum EnemyTypes
    {
        Default,
        Weapon
    }

    [SerializeField] private EnemyTypes enemyType;
    [SerializeField] private float timeReward;
    private readonly int animSpeed = Animator.StringToHash("speed");


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cc = GetComponent<CapsuleCollider>();
        ai = GetComponentInChildren<EnemyAI>();
        // Set type
        EnemyVariants enemyType = Instantiate(enemyTypes[Random.Range(0,enemyTypes.Length)], transform).GetComponent<EnemyVariants>();
        enemyType.SetTexture();

        Stats = enemyType.gameObject.GetComponent<EnemyStats>();
        
        Animator = GetComponentInChildren<Animator>();
        Alive = true;
    }

    private void Start()
    { 
        // the player tag will have to be created 
        player = GameManager.playerController;
        PlayerTransform = player.transform;
        
        ai.Agent.speed = Stats.speed;

    }

    private void Update()
    {
        if (Alive)
        {
            FollowPlayer();
        }
    }

    private void FollowPlayer()
    {
        if(!ai.enabled)
            return;
        
        float distance = Vector3.Distance(transform.position, PlayerTransform.position);

        // If the enemy is close enough and the player isnt stunned
        InRange = (distance < inRangeRadius && player.CanBeAttacked());
        
        if (Alive && !isPlayerStunned && distance < targetRange &&  distance >= 1)
        {
            Vector3 tempPos = transform.position;

            // Move position
            transform.position = ai.transform.position;
            // Dont ask why this is necessary okay, for some reason it needs to be slightly clipped into the ground for ragdoll physics to work properly
            transform.position += Vector3.up/1.1f;

            // Rotate enemy
            transform.rotation = ai.transform.rotation;

            // Change the enemys walk speed depending on how fast theyre going
            float currentSpeed = Mathf.Lerp(0,1,Vector3.Distance(transform.position,tempPos)*250);
            Animator.SetFloat(animSpeed, currentSpeed);

            ai.ResetTransform();
        }
    }

    public void Kill(Vector3 hitDirection, float hitPower, float hitHeight, int comboStage)
    {
        Alive = false;

        // Remove collision
        cc.isTrigger = true;
        // Allow ragdoll rotation
        rb.constraints = RigidbodyConstraints.None;
        rb.isKinematic = false;
        ai.enabled = false;

        // Apply velocity to enemy based on where theyre hit from
        rb.AddExplosionForce(hitPower, (transform.position - hitDirection), 20, hitHeight, ForceMode.Impulse);



        // Add time to main timer
        GameManager.timeManager.AddTime(GetTimeReward.Invoke(), transform.position);

        // Despawn
        StartCoroutine(DespawnAfterDelay(deathDespawnDelay));


    }

    public void Spawn(Vector3 position, Action<Enemy> onKill, Func<float> getTimeReward)
    {
        OnKill = onKill;
        GetTimeReward = getTimeReward;
        
        //Debug.Log("Random Position: " + position);

        transform.position = position;
        ai.ResetTransform();
        //ai.Agent.Warp(position);
        
    }

    private IEnumerator DespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        OnKill?.Invoke(this);
        
        // despawn the enemy
        Destroy(gameObject);
    }

    public void ChaseCooldown(float duration)
    {
        StartCoroutine(ai.ChaseCooldown(duration));
    }

    public void SetSpeed(float speed)
    {
        if (speed == -1)
        {
            ai.Agent.speed = Stats.speed;
            return;
        }

        ai.Agent.speed = speed;
    }

    public void Freeze()
    {
        ai.enabled = false;
        Animator.SetFloat(animSpeed, 0);
    }
}
