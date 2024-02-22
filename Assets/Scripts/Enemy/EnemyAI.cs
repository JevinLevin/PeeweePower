using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent Agent { get; private set; }

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (!Agent.isOnNavMesh)
            return;
        
        if(Agent.enabled && Agent.isOnNavMesh) Agent.destination = GameManager.playerController.transform.position;
    }

    public void ResetTransform()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
    
    public IEnumerator ChaseCooldown(float duration)
    {
        Agent.enabled = false;

        yield return new WaitForSeconds(duration);

        Agent.enabled = true;
    }
}
