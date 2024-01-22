using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Kill(Transform origin, float hitPower, float hitHeight)
    {
        rb.AddExplosionForce(hitPower,(transform.position-origin.right),2, hitHeight, ForceMode.Impulse);
    }
}
