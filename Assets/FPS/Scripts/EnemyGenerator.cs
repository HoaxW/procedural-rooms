using System.Collections;
using System.Collections.Generic;
using Unity.FPS.AI;
using UnityEngine;
using UnityEngine.AI;
public class EnemyGenerator : MonoBehaviour
{
    private GameObject enemyPrefab;
    private GameObject patrolPrefab;
    private float minRoomSize = 15f;
    private DungeonCreator dungeonCreator;
    public EnemyGenerator(GameObject enemyPrefab, GameObject patrolPrefab, DungeonCreator dungeonCreator)
    {
        this.enemyPrefab = enemyPrefab;
        this.patrolPrefab = patrolPrefab;
        this.dungeonCreator = dungeonCreator; // Only used for parenting the enemies to the dungeon GameObject
    }
    public void GenerateEnemies(List<Node> listOfRooms)
    {
        foreach (Node room in listOfRooms)
        {
            Vector2Int bottomLeftCorner = room.BottomLeftAreaCorner;
            Vector2Int topRightCorner = room.TopRightAreaCorner;

            // Check if the room is large enough
            if (IsRoomLargeEnough(bottomLeftCorner, topRightCorner) && bottomLeftCorner != Vector2Int.zero)
            {
                if (Random.Range(0f, 1f) > 0.5f) // 50% chance to spawn an enemy in this room
                {
                    SpawnEnemy(DetermineMiddleOfRoom(bottomLeftCorner, topRightCorner), bottomLeftCorner, topRightCorner);
                }
            }
        }
    }
    private void SpawnEnemy(Vector3 position, Vector2Int bottomLeftCorner, Vector2Int topRightCorner)
    {
        if (enemyPrefab != null)
        {
            GameObject enemyInstance = Instantiate(enemyPrefab, position, Quaternion.identity, dungeonCreator.transform);

            if (patrolPrefab != null)
            {
                GameObject patrolInstance = Instantiate(patrolPrefab, position, Quaternion.identity, dungeonCreator.transform);

                PatrolPath patrolPath = patrolInstance.GetComponent<PatrolPath>();
                EnemyController enemyController = enemyInstance.GetComponent<EnemyController>();

                if (patrolPath != null && enemyController != null)
                {
                    RandomizePathNodes(patrolPath, bottomLeftCorner, topRightCorner);
                    patrolPath.EnemiesToAssign.Add(enemyController);
                }
            }
        }
    }

    private void RandomizePathNodes(PatrolPath patrolPath, Vector2Int bottomLeftCorner, Vector2Int topRightCorner)
    {
        // Iterate through each path node and randomize its position
        foreach (Transform pathNode in patrolPath.PathNodes)
        {
            Vector3 randomizedPosition = GetRandomizedNodePosition(pathNode.position, bottomLeftCorner, topRightCorner);
            pathNode.position = randomizedPosition;
        }
    }
    private Vector3 GetRandomizedNodePosition(Vector3 originalPosition, Vector2Int bottomLeftCorner, Vector2Int topRightCorner)
    {
        float maxRandomOffset = 10f;
        int maxAttempts = 20;

        Vector3 randomizedPosition;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float randomX = Random.Range(-maxRandomOffset, maxRandomOffset);
            float randomZ = Random.Range(-maxRandomOffset, maxRandomOffset);

            randomizedPosition = originalPosition + new Vector3(randomX, 0f, randomZ);

            // Check if the randomized position is within the room bounds
            if (IsPositionWithinBounds(randomizedPosition, bottomLeftCorner, topRightCorner))
            {
                return randomizedPosition;
            }
        }

        // If max attempts are reached, return the original position
        Debug.LogWarning("Max attempts reached. Unable to find a suitable randomized position within bounds.");
        return originalPosition;
    }
    private bool IsPositionWithinBounds(Vector3 position, Vector2Int bottomLeftCorner, Vector2Int topRightCorner)
    {
        if (position.x < bottomLeftCorner.x || position.x > topRightCorner.x ||
        position.z < bottomLeftCorner.y || position.z > topRightCorner.y)
        {
            return false;
        }
        NavMeshHit hit;
        if (NavMesh.SamplePosition(position, out hit, 0.1f, NavMesh.AllAreas))
        {
            return true;
        }
        return false;
    }
    private Vector3 DetermineMiddleOfRoom(Vector2Int bottomLeftCorner, Vector2Int topRightCorner)
    {
        float x = (bottomLeftCorner.x + topRightCorner.x) / 2f;
        float z = (bottomLeftCorner.y + topRightCorner.y) / 2f;
        return new Vector3(x, 0, z);
    }

    private bool IsRoomLargeEnough(Vector2Int bottomLeftCorner, Vector2Int topRightCorner)
    {
        float roomWidth = topRightCorner.x - bottomLeftCorner.x;
        float roomHeight = topRightCorner.y - bottomLeftCorner.y;
        return roomWidth >= minRoomSize && roomHeight >= minRoomSize;
    }
}
