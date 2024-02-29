using System;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UIElements;

public class BossRoom : MonoBehaviour
{
    private Vector2Int bottomLeftAreaCorner;
    private Vector2Int topRightAreaCorner;
    private GameObject bossPrefab;
    private Transform parentTransform;
    private GameObject bossInstance;
    private float wallHeight = 4.4f;
    private float wallDepth = 0.1f;
    public BossRoom(Vector2Int bottomLeftAreaCorner, Vector2Int topRightAreaCorner, GameObject bossPrefab, Transform dungeonTransform)
    {
        this.bottomLeftAreaCorner = bottomLeftAreaCorner;
        this.topRightAreaCorner = topRightAreaCorner;
        this.bossPrefab = bossPrefab;
        this.parentTransform = dungeonTransform;
    }
    public void SpawnBoss()
    {
        // Spawns the boss in the middle of the room
        Vector3 spawnPosition = new Vector3(
            (bottomLeftAreaCorner.x + topRightAreaCorner.x) / 2f,
            0,
            (bottomLeftAreaCorner.y + topRightAreaCorner.y) / 2f
        );
        bossInstance = GameObject.Instantiate(bossPrefab, spawnPosition, Quaternion.identity, parentTransform);
        Health bossHealth = bossInstance.GetComponent<Health>();
        if (bossHealth != null)
        {
            bossHealth.OnDie += OnBossDeath;
        }

        BlockCorridors();
    }

    private void BlockCorridors()
    {
        GameObject wallParent = new GameObject("CorridorBlocker");
        wallParent.transform.parent = parentTransform;

        CreateWall(new Vector3((bottomLeftAreaCorner.x + topRightAreaCorner.x) / 2, 0, topRightAreaCorner.y), wallParent, true, topRightAreaCorner.x - bottomLeftAreaCorner.x);
        CreateWall(new Vector3((bottomLeftAreaCorner.x + topRightAreaCorner.x) / 2, 0, bottomLeftAreaCorner.y), wallParent, true, topRightAreaCorner.x - bottomLeftAreaCorner.x);
        CreateWall(new Vector3(topRightAreaCorner.x, 0, (bottomLeftAreaCorner.y + topRightAreaCorner.y) / 2), wallParent, false, topRightAreaCorner.y - bottomLeftAreaCorner.y);
        CreateWall(new Vector3(bottomLeftAreaCorner.x, 0, (bottomLeftAreaCorner.y + topRightAreaCorner.y) / 2), wallParent, false, topRightAreaCorner.y - bottomLeftAreaCorner.y);
    }

    private void CreateWall(Vector3 position, GameObject wallParent, bool isVertical, float wallLength)
    {
        GameObject wall = new GameObject("CorridorBlockerWall", typeof(BoxCollider));
        wall.transform.position = position;
        wall.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        wall.transform.parent = wallParent.transform;

        BoxCollider wallCollider = wall.GetComponent<BoxCollider>();
        if (wallCollider != null) // Box collider adjustments
        {
            wallCollider.center = new Vector3(0, 2.2f, 0);
            if (isVertical)
            {
                wallCollider.size = new Vector3(wallLength, wallHeight, wallDepth);
            }
            else
            {
                wallCollider.size = new Vector3(wallDepth, wallHeight, wallLength);
            }
        }
    }

    private void OnBossDeath()
    {
        UnblockCorridors();
    }

    private void UnblockCorridors()
    {
        Destroy(GameObject.Find("CorridorBlocker"));
    }
}