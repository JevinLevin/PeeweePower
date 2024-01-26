using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [Header("Assets")] 
    [SerializeField] private GameObject enemyObject;
    [SerializeField] private GameObject grannyObject;

    [Header("References")] 
    [SerializeField] private BoxCollider spawnArea;
    [SerializeField] private GrannyTimer grannyTimer;

    [Header("Config")] 
    [SerializeField] private Vector2 spawnDelayMin = new Vector2(1.0f,0.25f);
    [SerializeField] private Vector2 spawnDelayMax = new Vector2(2.0f,0.5f);
    [SerializeField] private Vector2 timeRewardRange = new Vector2(1f,0.2f);
    [SerializeField] private float spawnScalingLength = 180.0f;
    [SerializeField] private Vector2 spawnCount;
    [SerializeField] private int maxEnemyCount = 100;
    [SerializeField] private float minPlayerDistance = 15;
    [SerializeField] private float maxPlayerDistance = 50;
    [SerializeField] private float grannySpawnTime = 60.0f;
    private List<Granny> grannies = new();
    private List<Enemy> enemies = new();
    private Coroutine spawningCoroutine;

    public bool Pause { get; set; }

    private Camera mainCamera;
    private NavMeshTriangulation triangulation;
    private Mesh navMesh;

    private float time;
    private float grannyTime;

    

    private void Start()
    {
        spawningCoroutine = StartCoroutine(EnemySpawning());

        GameManager.enemySpawner = this;
        
        mainCamera = Camera.main;
        triangulation = NavMesh.CalculateTriangulation();
        navMesh = new Mesh
        {
            vertices = triangulation.vertices
        };
    }

    private void Update()
    {
        time += Time.deltaTime;
        
        grannyTime += Time.deltaTime;
        
        grannyTimer.SetTimeProgress(grannyTime/grannySpawnTime);

        if(grannyTime >= grannySpawnTime)
            SpawnGranny();
    }

    /// <summary>
    /// Loops throughout the whole game spawning enemies at random intervals
    /// </summary>
    private IEnumerator EnemySpawning()
    {
        while (true)
        {
            float t = time / spawnScalingLength;
            float delay = Random.Range(Mathf.Lerp(spawnDelayMin.x,spawnDelayMin.y,t), Mathf.Lerp(spawnDelayMax.x,spawnDelayMax.y,t));

            yield return new WaitForSeconds(delay);

            if (Pause) yield return null;

            SpawnEnemies();

            // Prevent spawning more enemies than the max count
            while (enemies.Count >= maxEnemyCount)
                yield return null;
        }
        // ReSharper disable once IteratorNeverReturns
    }

    private void SpawnGranny()
    {
        grannyTime = 0.0f;
        
        Vector3 randomPosition = CalculateSpawnPosition();
        Granny newGranny = Instantiate(grannyObject, transform).GetComponent<Granny>();
        newGranny.Spawn(randomPosition);
        grannies.Add(newGranny);

    }

    /// <summary>
    /// Spawns a horde of enemies at a random position
    /// </summary>
    private void SpawnEnemies()
    {
        int randomCount = Mathf.RoundToInt(Random.Range(spawnCount.x, spawnCount.y));
        Vector3 randomPosition = CalculateSpawnPosition();
        for(int i = 0; i < randomCount; i++)
        {
            // Create enemy
            Enemy newEnemy = Instantiate(enemyObject, transform).GetComponent<Enemy>();
            newEnemy.Spawn(randomPosition, KillEnemy, GetTimeReward);
            enemies.Add(newEnemy);
        }
    }

    /// <summary>
    /// Create a random position to spawn a enemy
    /// </summary>
    /// <returns>Random position that is out of sight of the player.</returns>

    private Vector3 CalculateSpawnPosition()
    {
        Vector3 position;
        float distance;
        do
        {
            position = GetRandomPosition();
            distance = Vector3.Distance(position, GameManager.playerController.transform.position);

            // Repeatedly generate positions until its out of camera view and far enough from the player
        } while (distance > maxPlayerDistance || distance < minPlayerDistance || IsVisible(position));
        
        return position;


    }
    
    /// <summary>
    /// Generate a random Vector3 on the maps navmesh
    /// </summary>
    /// <returns>Random position on navmesh.</returns>

    private Vector3 GetRandomPosition()
    {
        //Vector3 randomDirection = Random.insideUnitSphere * maxPlayerDistance;
        //randomDirection += transform.position;
        //NavMeshHit hit;
        //Vector3 finalPosition = Vector3.zero;
        //if (NavMesh.SamplePosition(randomDirection, out hit, maxPlayerDistance, 1)) {
        //    finalPosition = hit.position;            
        //}
        
        Vector3 colliderSize = spawnArea.size;
            
        float randomX = Random.Range(transform.position.x - colliderSize.x / 2, transform.position.x + colliderSize.x / 2);
        float randomZ = Random.Range(transform.position.z - colliderSize.z / 2, transform.position.z + colliderSize.z / 2);
        
        Vector3 finalPosition = new Vector3(randomX, GameManager.playerController.transform.position.y,randomZ);

        
        return finalPosition;
    }
    
    /// <summary>
    /// Calculates if position is out of camera view or obstructed by walls
    /// </summary>
    /// <returns>A bool stating if the position is visible or not.</returns>
    /// <param name="position">The point to check the visibility of.</param>
    private bool IsVisible(Vector3 position)
    {
        Vector3 cameraPosition = mainCamera.WorldToViewportPoint(position);
        // Uses -0.1 and 1.1 to add extra padding to prevent enemy spawning while half onscreen
        if (!(cameraPosition.x >= -0.1f) || !(cameraPosition.x <= 1.1f) || !(cameraPosition.y >= -0.1f) || !(cameraPosition.y <= 1.1f) || !(cameraPosition.z > -0.1f)) 
            return false;
        
        // Check if position is obstructed by walls
        // Linecast rather than raycast lets me determine the start and end position
        if (Physics.Linecast(position + Vector3.up, mainCamera.transform.position, out RaycastHit hit) && hit.collider.gameObject.layer == 8)
                return false;

        return true;

    }

    /// <summary>
    /// Handles logic for when a enemy is killed in the map
    /// </summary>
    /// <param name="enemy">The script of the enemy thats being killed.</param>
    private void KillEnemy(Enemy enemy)
    {
        enemies.Remove(enemy);
    }

    public void Freeze()
    {
        StopCoroutine(spawningCoroutine);
        
        foreach (Enemy enemy in enemies)
            enemy.Freeze();
        foreach (Granny granny in grannies)
            granny.Freeze();


    }

    private float GetTimeReward()
    {
        return Mathf.Lerp(timeRewardRange.x, timeRewardRange.y, time / spawnScalingLength);
    }
}
