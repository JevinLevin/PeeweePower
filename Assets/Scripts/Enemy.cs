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

    public void Kill(Transform origin, float hitPower, float hitHeight, int comboStage)
    {

        Vector3 hitDirection = Vector3.zero;
        switch (comboStage)
        {
            case 1:
                hitDirection = origin.right;
                break;
            case 2:
                hitDirection = -origin.right;
                break;
            case 3:
                hitDirection = -origin.up + transform.forward/2;
                hitPower *= 1.25f;
                hitHeight *= 2f;
                break;
        }
                


        rb.AddExplosionForce(hitPower,(transform.position-hitDirection),2, hitHeight, ForceMode.Impulse);
    }
}
