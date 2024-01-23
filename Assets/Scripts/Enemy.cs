using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Rigidbody rb;
    private CapsuleCollider cc;
    public bool Alive { get;private set; }

    private PlayerController player;
    private Transform playerTransform;
    private bool isPlayerStunned = false;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float punchRange = 2f;
    [SerializeField] private float punchCooldown = 3f; // Time between punches
    [SerializeField] private float deathDespawnDelay = 2f; // Time before the enemy is despawned after death

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

    private void Start()
    { // the player tag will have to be created 
        player = GameManager.playerController;
        playerTransform = player.transform;
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
        if (!isPlayerStunned)
        {
            Vector3 targetPosition = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
    }

    private IEnumerator PunchPlayerRoutine()
    {
        while (Alive)
        {
            // using a timer to determine when to punch, follow the player, try to land a swing 
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer < punchRange)
            {
                PunchPlayer();
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
        cc.isTrigger = true;
        rb.constraints = RigidbodyConstraints.None;

        rb.AddExplosionForce(hitPower, (transform.position - hitDirection), 20, hitHeight, ForceMode.Impulse);

        Alive = false;

        GameManager.timeManager.AddTime(timeReward);

        StartCoroutine(DespawnAfterDelay(deathDespawnDelay));


    }

    private IEnumerator DespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // despawn the enemy
        Destroy(gameObject);
    }
}
