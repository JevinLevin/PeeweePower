using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    [SerializeField] private float speed;

    private int currentWayPoint;
    private Transform targetWayPoint;

    private CarPath carPath;
    
    private void Start()
    {
        carPath = FindObjectOfType<CarPath>();
        
        float minDistance = float.MaxValue;
        int index = -1;

        for (int i = 0; i < carPath.points.Length; i++) {
            float distance = Vector3.Distance(carPath.points[i].position, transform.position);
    
            if (distance < minDistance) {
                minDistance = distance;
                index = i;
            }
        }

        currentWayPoint = index;
    }

    void Update ()
    {
        targetWayPoint = carPath.points[currentWayPoint];
        Move();
        
    }

    void Move(){
        // rotate towards the target
        transform.forward = Vector3.RotateTowards(transform.forward, targetWayPoint.position - transform.position, speed*Time.deltaTime / 4, 0.0f);

        // move towards the target
        transform.position = Vector3.MoveTowards(transform.position, targetWayPoint.position,   speed*Time.deltaTime);

        if(transform.position == targetWayPoint.position)
        {
            currentWayPoint ++ ;
            if (currentWayPoint >= carPath.points.Length)
                currentWayPoint = 0;
            targetWayPoint = carPath.points[currentWayPoint];
        }
    } 
    
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.TryGetComponent(out PlayerController player))
        {
            player.StartStun(-other.transform.forward);

        }
    }
}
