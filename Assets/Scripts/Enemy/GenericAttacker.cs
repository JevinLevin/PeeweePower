using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericAttacker<T> : MonoBehaviour
{
    protected List<T> targetsInRange = new();
    
    private void OnTriggerEnter(Collider other)
    {
        // If enemy enters attack range, add to list
        if (other.TryGetComponent(out T targetScript))
            targetsInRange.Add(targetScript);

    }

    private void OnTriggerExit(Collider other)
    {
        // Remove enemy from list if they leave range
        if (other.TryGetComponent(out T targetScript))
            targetsInRange.Remove(targetScript);

    }

}
