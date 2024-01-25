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

    private EnemyAI ai;
    private Rigidbody rb;
    private PlayerController player;
    private Animator animator;
    private static readonly int IsStunned = Animator.StringToHash("isStunned");


    private void Awake()
    {
        ai = GetComponent<EnemyAI>();
        rb= GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerController player))
        {
            this.player = player;

            if(player.PlayerState == PlayerController.PlayerStates.Charging)
            {
                GameManager.playerAttacker.EndMidCharge();
                
                StartCoroutine(GrannyStun());
                return;
            }

            player.StartStun(transform.forward, playerStunDuration);
            StartCoroutine(ChaseCooldown());

        }
    }

    private IEnumerator ChaseCooldown()
    {
        ai.enabled = false;

        yield return new WaitForSeconds(playerStunDuration);

        ai.enabled = true;
    }

    private IEnumerator GrannyStun()
    {
        ai.enabled = false;
        rb.isKinematic = true;

        float stunTime = 0.0f;
        float t;
        Vector3 direction = transform.forward;

        animator.SetBool(IsStunned, true);

        while (stunTime < grannyStunDuration) 
        {
            if ((t = stunTime / reboundLength) < 1)
            {
                player.AddExternalVelocity(direction * Mathf.Lerp(reboundPower,0, reboundCurve.Evaluate(t)));
                print(direction * reboundCurve.Evaluate(t));
            }

            stunTime += Time.deltaTime;

            yield return null;
        }

        ai.enabled = true;
        rb.isKinematic = false;
        animator.SetBool(IsStunned, false);
    }

}
