using System;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
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
    private GameObject player;
    private Material wallMaterial;
    public BossRoom(Vector2Int bottomLeftAreaCorner, Vector2Int topRightAreaCorner, GameObject bossPrefab, Transform dungeonTransform, GameObject player)
    {
        this.bottomLeftAreaCorner = bottomLeftAreaCorner;
        this.topRightAreaCorner = topRightAreaCorner;
        this.bossPrefab = bossPrefab;
        this.parentTransform = dungeonTransform;
        this.player = player; // pass this down to PlayerInteraction
    }
    public void SetUpBossRoom()
    {
        Debug.Log("Bottom left corner boss: " + bottomLeftAreaCorner);
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
        wallMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        wallMaterial.color = Color.yellow;
        BlockCorridors();
    }

    private void BlockCorridors()
    {
        GameObject wallParent = new GameObject("CorridorBlocker");
        wallParent.transform.parent = parentTransform;

        float wallSpacing = 2.0f;

        CreateWall(new Vector3((bottomLeftAreaCorner.x + topRightAreaCorner.x) / 2, 0, topRightAreaCorner.y + wallSpacing), wallParent, true, topRightAreaCorner.x - bottomLeftAreaCorner.x);
        CreateWall(new Vector3((bottomLeftAreaCorner.x + topRightAreaCorner.x) / 2, 0, bottomLeftAreaCorner.y - wallSpacing), wallParent, true, topRightAreaCorner.x - bottomLeftAreaCorner.x);
        CreateWall(new Vector3(topRightAreaCorner.x + wallSpacing, 0, (bottomLeftAreaCorner.y + topRightAreaCorner.y) / 2), wallParent, false, topRightAreaCorner.y - bottomLeftAreaCorner.y);
        CreateWall(new Vector3(bottomLeftAreaCorner.x - wallSpacing, 0, (bottomLeftAreaCorner.y + topRightAreaCorner.y) / 2), wallParent, false, topRightAreaCorner.y - bottomLeftAreaCorner.y);
    }
    private void CreateWall(Vector3 position, GameObject wallParent, bool isVertical, float wallLength)
    {
        GameObject wall = new GameObject("CorridorBlockerWall", typeof(MeshRenderer), typeof(BoxCollider));
        MeshFilter meshFilter = wall.AddComponent<MeshFilter>();
        wall.transform.position = position;
        wall.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        wall.transform.parent = wallParent.transform;

        MeshRenderer wallRenderer = wall.GetComponent<MeshRenderer>();
        Mesh mesh = new Mesh();

        float width = 1.0f;
        float height = 1.0f;
        float depth = 1.0f;

        Vector3[] vertices = new Vector3[]
        {
        // Vertices for the front face
        new Vector3(-width / 2, 0, -depth / 2),
        new Vector3(width / 2, 0, -depth / 2),
        new Vector3(-width / 2, height, -depth / 2),
        new Vector3(width / 2, height, -depth / 2),
           
        // Vertices for the back face
        new Vector3(-width / 2, 0, depth / 2),
        new Vector3(width / 2, 0, depth / 2),
        new Vector3(-width / 2, height, depth / 2),
        new Vector3(width / 2, height, depth / 2),
        };

        int[] triangles = new int[]
        {
        // Front face
        0, 2, 1, 1, 2, 3,
        // Back face
        4, 5, 6, 6, 5, 7,
        // Top face
        2, 6, 3, 3, 6, 7,
        // Bottom face
        0, 1, 4, 4, 1, 5,
        // Left face
        0, 4, 2, 2, 4, 6,
        // Right face
        1, 3, 5, 5, 3, 7,
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        meshFilter.mesh = mesh;
        if (wallRenderer != null)
        {
            wallRenderer.material = wallMaterial;
        }


        wall.AddComponent<PlayerInteraction>().SetReferences(bossInstance, player);

        BoxCollider boxCollider = wall.GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            boxCollider.center = new Vector3(0, height / 2, 0);
        }
        if (isVertical)
        {
            wall.transform.localScale = new Vector3(wallLength, wallHeight, wallDepth);
        }
        else
        {
            wall.transform.localScale = new Vector3(wallDepth, wallHeight, wallLength);
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