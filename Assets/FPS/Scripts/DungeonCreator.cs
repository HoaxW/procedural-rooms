using System;
using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using Unity.FPS.UI;
using UnityEngine;
using UnityEngine.AI;

public class DungeonCreator : MonoBehaviour
{
    [SerializeField] private int dungeonWidth, dungeonLength;
    [SerializeField] private int roomWidthMin, roomLengthMin;
    [SerializeField] private int maxIterations;
    [SerializeField] private int corridorWidth;
    [SerializeField] private Material material;
    [Range(0.0f, 0.3f)]
    [SerializeField] private float roomBottomCornerModifier;
    [Range(0.7f, 1.0f)]
    [SerializeField] private float roomTopCornerModifier;
    [Range(0, 2)]
    [SerializeField] private int roomOffset;
    public GameObject wallVertical, wallHorizontal;
    List<Vector3Int> possibleDoorVerticalPosition;
    List<Vector3Int> possibleDoorHorizontalPosition;
    List<Vector3Int> possibleWallHorizontalPosition;
    List<Vector3Int> possibleWallVerticalPosition;
    [SerializeField] private List<GameObject> objectPrefabs;
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject patrolPrefab;
    [SerializeField] private GameObject player;
    void Start()
    {
        CreateDungeon();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Reload the scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(3);
        }
    }
    public void CreateDungeon()
    {
        DestroyAllChildren();
        DungeonGenerator generator = new DungeonGenerator(dungeonWidth, dungeonLength);
        var listOfRooms = generator.CalculateDungeon(maxIterations,
            roomWidthMin,
            roomLengthMin,
            roomBottomCornerModifier,
            roomTopCornerModifier,
            roomOffset,
            corridorWidth);
        GameObject wallParent = new GameObject("WallParent");
        wallParent.transform.parent = transform;
        possibleDoorVerticalPosition = new List<Vector3Int>();
        possibleDoorHorizontalPosition = new List<Vector3Int>();
        possibleWallHorizontalPosition = new List<Vector3Int>();
        possibleWallVerticalPosition = new List<Vector3Int>();
        bool playerRoomCreated = false;

        for (int i = 0; i < listOfRooms.Count; i++)
        {
            CreateMesh(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner);
            if (!playerRoomCreated)
            {
                // Make the first room the player room
                PlayerRoom playerRoom = new PlayerRoom(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner, player);
                playerRoom.MovePlayerToCenterOfThisRoom();
                playerRoomCreated = true;
            }
            else if (i == listOfRooms.Count - 10) // Last generated room is the boss room, 10 rooms after that are hallways
            {
                BossRoom bossRoom = new BossRoom(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner, bossPrefab, transform, enemyPrefab, player);
                bossRoom.SetUpBossRoom();
            }
            else
            {
                // Add random objects to non-player and non-boss rooms
                AddRandomObjects(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner);
            }
        }
        CreateWalls(wallParent);
        EnemyGenerator enemyGenerator = new EnemyGenerator(enemyPrefab, patrolPrefab, this);
        enemyGenerator.GenerateEnemies(listOfRooms);
    }
    private void GenerateNavMesh(GameObject room)
    {
        NavMeshSurface navMeshSurface = room.GetComponent<NavMeshSurface>();

        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
        }
        else
        {
            Debug.LogError("NavMeshSurface component not found on the room GameObject.");
        }
    }

    private void AddRandomObjects(Vector2Int bottomLeftAreaCorner, Vector2Int topRightAreaCorner)
    {
        int numberOfObjects = UnityEngine.Random.Range(1, 4);
        int maxAttempts = 10; // Adjust as needed

        for (int i = 0; i < numberOfObjects; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, objectPrefabs.Count);
            GameObject randomObject = objectPrefabs[randomIndex];

            Vector3 position;
            int attempts = 0;

            do
            {
                float x = UnityEngine.Random.Range(bottomLeftAreaCorner.x, topRightAreaCorner.x);
                float z = UnityEngine.Random.Range(bottomLeftAreaCorner.y, topRightAreaCorner.y);
                position = new Vector3(x, 0, z);
                attempts++;

                if (attempts >= maxAttempts)
                {
                    Debug.LogWarning("Max attempts reached. Unable to find a suitable position.");
                    break;
                }
            } while (IsTooCloseToWallOrDoorway(position));

            if (attempts < maxAttempts)
            {
                Instantiate(randomObject, position, Quaternion.identity, transform);
            }
        }
    }

    private bool IsTooCloseToWallOrDoorway(Vector3 position)
    {
        float minDistanceToWall = 6.0f;
        float minDistanceToDoorway = 6.0f;

        foreach (Vector3Int doorwayPosition in possibleDoorVerticalPosition)
        {
            if (Vector3.Distance(position, doorwayPosition) < minDistanceToDoorway)
            {
                return true;
            }
        }

        foreach (Vector3Int doorwayPosition in possibleDoorHorizontalPosition)
        {
            if (Vector3.Distance(position, doorwayPosition) < minDistanceToDoorway)
            {
                return true;
            }
        }

        foreach (Vector3Int wallPosition in possibleWallVerticalPosition)
        {
            if (Vector3.Distance(position, wallPosition) < minDistanceToWall)
            {
                return true;
            }
        }

        foreach (Vector3Int wallPosition in possibleWallHorizontalPosition)
        {
            if (Vector3.Distance(position, wallPosition) < minDistanceToWall)
            {
                return true;
            }
        }

        return false;
    }

    private void CreateWalls(GameObject wallParent)
    {
        foreach (var wallPosition in possibleWallHorizontalPosition)
        {
            CreateWall(wallParent, wallPosition, wallHorizontal);
        }
        foreach (var wallPosition in possibleWallVerticalPosition)
        {
            CreateWall(wallParent, wallPosition, wallVertical);
        }
    }

    private void CreateWall(GameObject wallParent, Vector3Int wallPosition, GameObject wallPrefab)
    {
        GameObject wall = Instantiate(wallPrefab, wallPosition, Quaternion.identity, wallParent.transform);
        GameObject wallChild = wall.transform.GetChild(0).GetChild(0).gameObject;
        AddBoxCollider(wallChild);
    }

    private void CreateMesh(Vector2 bottomLeftCorner, Vector2 topRightCorner)
    {
        Vector3 bottomLeftV = new Vector3(bottomLeftCorner.x, 0, bottomLeftCorner.y);
        Vector3 bottomRightV = new Vector3(topRightCorner.x, 0, bottomLeftCorner.y);
        Vector3 topLeftV = new Vector3(bottomLeftCorner.x, 0, topRightCorner.y);
        Vector3 topRightV = new Vector3(topRightCorner.x, 0, topRightCorner.y);

        Vector3[] vertices = new Vector3[]
        {
            topLeftV,
            topRightV,
            bottomLeftV,
            bottomRightV
        };

        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        }

        int[] triangles = new int[]
        {
            0,
            1,
            2,
            2,
            1,
            3
        };
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        GameObject dungeonFloor = new GameObject("Mesh" + bottomLeftCorner,typeof(MeshFilter),typeof(MeshRenderer), typeof(NavMeshSurface));

        dungeonFloor.transform.position = Vector3.zero;
        dungeonFloor.transform.localScale = Vector3.one;
        dungeonFloor.GetComponent<MeshFilter>().mesh = mesh;
        dungeonFloor.GetComponent<MeshRenderer>().material = material;
        dungeonFloor.transform.parent = transform;
        GenerateNavMesh(dungeonFloor);

        AddBoxCollider(dungeonFloor);
        for (int row = (int)bottomLeftV.x; row < (int)bottomRightV.x; row++)
        {
            var wallPosition = new Vector3(row, 0, bottomLeftV.z);
            AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition);
        }
        for (int row = (int)topLeftV.x; row < (int)topRightCorner.x; row++)
        {
            var wallPosition = new Vector3(row, 0, topRightV.z);
            AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition);
        }
        for (int col = (int)bottomLeftV.z; col < (int)topLeftV.z; col++)
        {
            var wallPosition = new Vector3(bottomLeftV.x, 0, col);
            AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition);
        }
        for (int col = (int)bottomRightV.z; col < (int)topRightV.z; col++)
        {
            var wallPosition = new Vector3(bottomRightV.x, 0, col);
            AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition);
        }
    }

    private void AddBoxCollider(GameObject obj)
    {
        BoxCollider boxCollider = obj.AddComponent<BoxCollider>();
        if (boxCollider != null)
        {
            MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                Bounds bounds = meshFilter.sharedMesh.bounds;
                boxCollider.size = new Vector3(bounds.size.x, bounds.size.y, bounds.size.z);
                boxCollider.center = new Vector3(bounds.center.x, bounds.center.y, bounds.center.z);
            }
        }
    }

    private void AddWallPositionToList(Vector3 wallPosition, List<Vector3Int> wallList, List<Vector3Int> doorList)
    {
        Vector3Int point = Vector3Int.CeilToInt(wallPosition);
        if (wallList.Contains(point))
        {
            doorList.Add(point);
            wallList.Remove(point);
        }
        else
        {
            wallList.Add(point);
        }
    }

    private void DestroyAllChildren()
    {
        while(transform.childCount != 0)
        {
            foreach (Transform item in transform)
            {
                DestroyImmediate(item.gameObject);
            }
        }
    }
}
