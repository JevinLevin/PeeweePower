using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

    [SerializeField] private float punchRange = 2f;
    [SerializeField] private float punchCooldown = 3f; // Time between punches
    [SerializeField] private float deathDespawnDelay = 2f; // Time before the enemy is despawned after death
    [SerializeField] private float targetRange = 25f; // Distance to the player before the enemy starts tracking them
    [SerializeField] private float inRangeRadius = 2;

    public enum EnemyTypes
    {
        Default,
        Weapon
    }

    [SerializeField] private EnemyTypes enemyType;
    [SerializeField] private float timeReward;
    private readonly int speed = Animator.StringToHash("speed");


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cc = GetComponent<CapsuleCollider>();
        ai = GetComponentInChildren<EnemyAI>();
        Animator = GetComponentInChildren<Animator>();

        Alive = true;
    }

    private void Start()
    { // the player tag will have to be created 
        player = GameManager.playerController;
        PlayerTransform = player.transform;
        StartCoroutine(PunchPlayerRoutine());
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
        float distance = Vector3.Distance(transform.position, PlayerTransform.position);

        // If the enemy is close enough and the player isnt stunned
        InRange = (distance < inRangeRadius && player.PlayerState != PlayerController.PlayerStates.Stunned);
        
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
            Animator.SetFloat(speed, currentSpeed);

            ai.ResetTransform();
        }
    }

    private IEnumerator PunchPlayerRoutine()
    {
        while (Alive)
        {
            // using a timer to determine when to punch, follow the player, try to land a swing 
            float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);
            if (distanceToPlayer < punchRange)
            {
                //PunchPlayer();
            }

            yield return new WaitForSeconds(punchCooldown);
        }
    }

    private void PunchPlayer()
    {
        // Add logic to make the player stop moving for 3 seconds
        StartCoroutine(StopPlayerMovement(3f));
    }

    private IEnumerator StopPlayerMovement(float duration)
    {
        isPlayerStunned = true;
        //  logic to stop player movement here, the player controller is going to require the custom method "SetCanMove in the Player Controller"
        // set a flag in the player's script to prevent movement
        player.GetComponent<PlayerController>().CanMove = false;

        yield return new WaitForSeconds(duration);

        // resume player movement after the specified duration
        player.GetComponent<PlayerController>().CanMove = true;
        isPlayerStunned = false;
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
        GameManager.timeManager.AddTime(timeReward);

        // Despawn
        StartCoroutine(DespawnAfterDelay(deathDespawnDelay));


    }

    private IEnumerator DespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // despawn the enemy
        Destroy(gameObject);
    }
}
