using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Granny : MonoBehaviour
{
    [SerializeField] private float playerStunDuration = 5.0f;
    [SerializeField] private float grannyStunDuration = 2.0f;

    [SerializeField] private float reboundLength = 0.25f;
    [SerializeField] private AnimationCurve reboundCurve;
    [SerializeField] private float reboundPower = 25;

    [SerializeField] private AudioClip grannyLaugh;
    [SerializeField] private AudioClip grannyStun;
    

    private EnemyAI ai;
    private Rigidbody rb;
    private PlayerController player;
    private Animator animator;
    private static readonly int IsStunned = Animator.StringToHash("isStunned");
    private static readonly int StunSpeed = Animator.StringToHash("stunSpeed");

    private bool stunned;


    private void Awake()
    {
        ai = GetComponent<EnemyAI>();
        rb= GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
    }

    public void Spawn(Vector3 position)
    {
        transform.position = position;
        ai.Agent.Warp(position);
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.TryGetComponent(out PlayerController player))
        {
            if (stunned)
                return;
            
            this.player = player;

            if(player.IsCharging)
            {
                GameManager.playerAttacker.EndMidCharge();

                StartCoroutine(GrannyStun());
                return;
            }

            AudioManager.Instance.PlayAudio(grannyLaugh,transform.position);
            player.StartStun(transform.forward, playerStunDuration);
            StartCoroutine(ai.ChaseCooldown(playerStunDuration));

        }
    }

    private IEnumerator GrannyStun()
    {
        ai.enabled = false;
        rb.isKinematic = true;

        stunned = true;

        float stunTime = 0.0f;
        float t;
        Vector3 direction = transform.forward;

        animator.SetBool(IsStunned, true);
        
        AudioManager.Instance.PlayAudio(grannyStun,transform.position);

        while (stunTime < grannyStunDuration) 
        {
            if ((t = stunTime / reboundLength) < 1)
            {
                player.AddExternalVelocity(direction * Mathf.Lerp(reboundPower,0, reboundCurve.Evaluate(t)));
            }

            float stunSpeed = Mathf.Lerp(2, 0.25f, stunTime / grannyStunDuration);
            animator.SetFloat(StunSpeed, stunSpeed);

            stunTime += Time.deltaTime;

            yield return null;
        }

        ai.enabled = true;
        rb.isKinematic = false;
        animator.SetBool(IsStunned, false);
        stunned = false;
    }

    public void Freeze()
    {
        ai.enabled = false;
    }

}
