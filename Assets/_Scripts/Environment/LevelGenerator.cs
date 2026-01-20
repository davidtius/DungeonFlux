using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using Unity.Cinemachine;

public class Leaf
{
    public RectInt rect;
    public Leaf leftChild;
    public Leaf rightChild;
    public RectInt? room = null;

    private readonly int MIN_LEAF_SIZE;

    public Leaf(RectInt rect, int minLeafSize)
    {
        this.rect = rect;
        this.MIN_LEAF_SIZE = minLeafSize;
    }

    public bool Split()
    {
        if (leftChild != null || rightChild != null) return false;

        bool splitHorizontally = (Random.Range(0, 2) == 0);
        if (rect.width > rect.height && (float)rect.width / rect.height >= 1.25f) splitHorizontally = false;
        else if (rect.height > rect.width && (float)rect.height / rect.width >= 1.25f) splitHorizontally = true;

        int minSplit = MIN_LEAF_SIZE + 1;
        int maxSplit = (splitHorizontally ? rect.height : rect.width) - MIN_LEAF_SIZE - 1;

        if (minSplit >= maxSplit) return false;

        int splitPoint = Random.Range(minSplit, maxSplit);

        if (splitHorizontally)
        {
            leftChild = new Leaf(new RectInt(rect.x, rect.y, rect.width, splitPoint), MIN_LEAF_SIZE);
            rightChild = new Leaf(new RectInt(rect.x, rect.y + splitPoint, rect.width, rect.height - splitPoint), MIN_LEAF_SIZE);
        }
        else
        {
            leftChild = new Leaf(new RectInt(rect.x, rect.y, splitPoint, rect.height), MIN_LEAF_SIZE);
            rightChild = new Leaf(new RectInt(rect.x + splitPoint, rect.y, rect.width - splitPoint, rect.height), MIN_LEAF_SIZE);
        }
        return true;
    }

    public void CreateRooms(LevelGenerator levelGenerator)
    {
        if (leftChild != null || rightChild != null)
        {
            leftChild?.CreateRooms(levelGenerator);
            rightChild?.CreateRooms(levelGenerator);

            if (leftChild != null && rightChild != null)
            {

                RectInt? roomA = leftChild.GetRoom();
                RectInt? roomB = rightChild.GetRoom();

                if (roomA.HasValue && roomB.HasValue)
                {

                    RoomType typeA = levelGenerator.GetRoomType(roomA.Value);
                    RoomType typeB = levelGenerator.GetRoomType(roomB.Value);

                    if (typeA != RoomType.Secret && typeB != RoomType.Secret)
                    {
                        levelGenerator.CreateCorridor(roomA, roomB);
                    }
                }
            }
        }
        else
        {
            int roomWidth = Random.Range(rect.width / 2, rect.width - 1);
            int roomHeight = Random.Range(rect.height / 2, rect.height - 1);
            int roomX = rect.x + Random.Range(1, rect.width - roomWidth);
            int roomY = rect.y + Random.Range(1, rect.height - roomHeight);
            room = new RectInt(roomX, roomY, roomWidth, roomHeight);
            levelGenerator.CarveRoom((RectInt)room);
        }
    }

    public RectInt? GetRoom()
    {
        if (room != null) return room;
        else
        {
            RectInt? leftRoom = leftChild?.GetRoom();
            RectInt? rightRoom = rightChild?.GetRoom();
            if (leftRoom == null && rightRoom == null) return null;
            else if (rightRoom == null) return leftRoom;
            else if (leftRoom == null) return rightRoom;
            else return Random.Range(0, 2) == 0 ? leftRoom : rightRoom;
        }
    }
}

[System.Serializable]
public class BiomeTheme
{
    public string biomeName;
    public TileBase floorTile;
    public RuleTile wallBaseRuleTile;
    public DestructibleSpriteSet destructibleSprites;

    [Header("Music")]
    public AudioClip biomeBGM;
    public AudioClip bossBGM;
}

[System.Serializable]
public struct DestructibleSpriteSet
{
    public Sprite up;
    public Sprite down;
    public Sprite left;
    public Sprite right;

    [Header("Default")]
    public Sprite center;
}

[System.Serializable]
public struct PCGProfileSettings
{
    public PlayerProfile profile;

    [Header("Map Dimensions")]
    public int mapWidth;
    public int mapHeight;

    [Header("BSP Parameters (Layout Complexity)")]
    public int minLeafSize;
    public int maxLeafSize;

}

public class LevelGenerator : MonoBehaviour
{
    [Header("Loot Table")]
    public List<PlayerController.LootData> possibleLoot;
    [Header("Object Reference")]
    public GameObject playerPrefab;
    public GameObject[] enemyPrefabs;
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public BoxCollider2D cameraBoundsCollider;
    public CinemachineConfiner2D cinemachineConfiner;

    [Header("Biome Settings")]
    public List<BiomeTheme> biomeThemes;

    [Header("Map Size")]
    public int mapWidth = 80;
    public int mapHeight = 60;

    [Header("BSP/Room Settings")]
    public int minLeafSize = 10;
    public int maxLeafSize = 20;
    public int corridorSize = 1;

    [Header("DDA-PCG Parameters")]
    public List<PCGProfileSettings> pcgProfiles;

    [Header("Gameplay Settings")]
    public int enemiesPerRoom = 2;
    public int floorPerBiome = 5;
    public GameObject[] bossPrefabs;

    [Header("Room Type Probabilities")]
    [Range(0f, 1f)] public float restStopChance = 0.15f;
    [Range(0f, 1f)] public float combatRoomChance = 0.2f;
    [Range(0f, 1f)] public float secretRoomChance = 0.1f;
    [Range(0f, 1f)] public float trapRoomChance = 0.25f;

    [Header("Room Specific Objects")]
    public GameObject shrinePrefab;
    public GameObject treasureChestPrefab;
    public GameObject trapPrefab;
    public GameObject destructibleWallPrefab;
    public GameObject lockingWallPrefab;
    public GameObject lockingRoomTriggerPrefab;

    [Header("Game Loop / Pindah Floor")]
    public GameObject exitPortalPrefab;
    public GameObject sanctuaryPortalPrefab;

    [Header("UI References")]
    public LevelFader levelFader;

    [Header("Room Specific Objects")]
    public GameObject roomVisitTriggerPrefab;

    private GameObject playerInstance;
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private int currentFloor = 1;
    private bool objectiveCompleted = false;

    [HideInInspector] public int[,] mapGrid;
    private List<RectInt> rooms = new List<RectInt>();
    private Dictionary<RectInt, RoomType> roomTypes = new Dictionary<RectInt, RoomType>();

    private RectInt playerSpawnRoom;
    private RectInt exitSpawnRoom;
    private BiomeTheme currentActiveTheme;
    private List<int> currentBiomeOrder = new List<int>();

    private PlayerProfile nextFloorProfile = PlayerProfile.Passive;

    private Dictionary<RectInt, List<(Vector3Int position, TileBase tile, bool isWall)>> hiddenTiles = new Dictionary<RectInt, List<(Vector3Int position, TileBase tile, bool isWall)>>();
    [HideInInspector]
    public HashSet<Vector2Int> secretFloorAndCorridorTiles = new HashSet<Vector2Int>();

    private Dictionary<RectInt, List<GameObject>> hiddenObjects = new Dictionary<RectInt, List<GameObject>>();

    private Dictionary<PlayerProfile, Dictionary<string, float>> ddaWeightDatabase;

    public static LevelGenerator Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }

        SetupDDAWeights();
    }

    void Start()
    {
        if (DDAManager.Instance != null)
        {
            DDAManager.Instance.RegisterLevelGenerator(this);
        }
        else
        {
            Debug.LogWarning("DDAManager tidak ditemukan (Testing Mode?). Generate New Run Default.");
            GenerateNewRun();
        }
    }

    public BiomeTheme GetCurrentBiomeTheme()
    {
        return currentActiveTheme;
    }

    public void SetNextFloorProfile(PlayerProfile profile)
    {
        this.nextFloorProfile = profile;
        Debug.Log($"<color=cyan>LevelGenerator:</color> Lantai berikutnya akan di-generate untuk Player {profile}.");
    }

    public void GenerateNewRun()
    {
        currentFloor = 1;
        GenerateLevel();
    }

    public void GenerateLevel()
    {

        PCGProfileSettings settings = pcgProfiles.FirstOrDefault(p => p.profile == nextFloorProfile);

        if (settings.profile != nextFloorProfile)
        {
            settings = pcgProfiles.FirstOrDefault(p => p.profile == PlayerProfile.Passive);
        }

        this.mapWidth = settings.mapWidth;
        this.mapHeight = settings.mapHeight;
        this.minLeafSize = settings.minLeafSize;
        this.maxLeafSize = settings.maxLeafSize;

        Debug.Log($"<color=cyan>DDA-PCG:</color> Menerapkan profil {settings.profile}. MapSize={mapWidth}x{mapHeight}, LeafSize={minLeafSize}-{maxLeafSize}");

        if (cameraBoundsCollider != null && cinemachineConfiner != null)
        {

            cameraBoundsCollider.transform.position = new Vector3(mapWidth / 2f, mapHeight / 2f, 0);

            cameraBoundsCollider.size = new Vector2(mapWidth, mapHeight);

            cinemachineConfiner.InvalidateBoundingShapeCache();
        }
        else
        {
            Debug.LogError("CameraBoundsCollider atau CinemachineConfiner belum di-set di Inspector!");
        }

        Debug.Log($"--- Generating Floor {currentFloor} ---");

        bool isBossFloor = (currentFloor % floorPerBiome == 0);

        if (isBossFloor)
        {
            Debug.Log($"<color=red>WARNING: Ini Lantai BOSS (Lantai {currentFloor})</color>");
        }

        ClearLevel();

        mapGrid = new int[mapWidth, mapHeight];
        rooms.Clear();
        roomTypes.Clear();
        secretFloorAndCorridorTiles.Clear();
        objectiveCompleted = false;

        List<Leaf> leaves = new List<Leaf>();
        Leaf root = new Leaf(new RectInt(0, 0, mapWidth, mapHeight), minLeafSize);
        leaves.Add(root);

        bool didSplit = true;

        while (didSplit)
        {
            didSplit = false;
            List<Leaf> tempLeaves = new List<Leaf>(leaves);
            foreach (var leaf in tempLeaves)
            {
                if (leaf.leftChild == null && leaf.rightChild == null)
                {
                    List<Leaf> newLeavesToAdd = new List<Leaf>();
                    if (leaf.rect.width > maxLeafSize || leaf.rect.height > maxLeafSize || Random.Range(0, 100) > 25)
                    {
                        if (leaf.Split())
                        {
                            newLeavesToAdd.Add(leaf.leftChild);
                            newLeavesToAdd.Add(leaf.rightChild);
                            didSplit = true;
                        }
                    }
                    if (newLeavesToAdd.Count > 0)
                    {
                        leaves.Remove(leaf);
                        leaves.AddRange(newLeavesToAdd);
                    }
                }
            }
        }

        root.CreateRooms(this);
        rooms.Sort((a, b) => Vector2.Distance(a.center, Vector2.zero).CompareTo(Vector2.Distance(b.center, Vector2.zero)));
        DetermineSpawnRooms(isBossFloor);
        ConnectCorridorsRecursively(root);

        MarkAllSecretAreaTiles();

        DrawMap();

        SpawnPlayer();
        SpawnExitPortal();
        SpawnRoomObjects();
        int totalEnemies = SpawnEnemies(isBossFloor);
        SpawnRoomVisitTriggers();

        int totalRealRooms = rooms.Count;

        int totalLootCount = 0;

        foreach (var kvp in roomTypes)
        {
            if (kvp.Value == RoomType.Secret)
            {
                totalLootCount++;
            }
        }

        if (PlayerDataTracker.Instance != null)
        {
            PlayerDataTracker.Instance.StartNewFloor(totalRealRooms, totalLootCount, totalEnemies);
            PlayerDataTracker.Instance.SetTimerPaused(false);
            PlayerDataTracker.Instance.SetFloorTotals(totalRealRooms, totalLootCount);
        }

        Debug.Log($"Floor {currentFloor} generated.");

        if (GameHUDManager.Instance != null)
        {

        int biomeLevel = ((currentFloor - 1) / floorPerBiome) + 1;

        int rawIndex = (biomeLevel - 1) % biomeThemes.Count;

        if (currentBiomeOrder == null || currentBiomeOrder.Count != biomeThemes.Count)
        {
            RandomizeBiomeOrder();
        }

        int shuffledIndex = currentBiomeOrder[rawIndex];

        BiomeTheme theme = biomeThemes[shuffledIndex];

        GameHUDManager.Instance.UpdateLevelInfo(theme.biomeName, biomeLevel, currentFloor);
        }

        if (MusicManager.Instance != null && currentActiveTheme.biomeBGM != null)
        {
            MusicManager.Instance.PlayMusic(currentActiveTheme.biomeBGM);
        }
    }

    public RoomType GetRoomType(RectInt room)
    {

        if (roomTypes.ContainsKey(room))
        {
            return roomTypes[room];
        }

        return RoomType.Normal;
    }

    private void ConnectCorridorsRecursively(Leaf leaf)
    {
        if (leaf == null || (leaf.leftChild == null && leaf.rightChild == null))
        {
            return;
        }

        ConnectCorridorsRecursively(leaf.leftChild);
        ConnectCorridorsRecursively(leaf.rightChild);

        if (leaf.leftChild != null && leaf.rightChild != null)
        {
            RectInt? roomA = leaf.leftChild.GetRoom();
            RectInt? roomB = leaf.rightChild.GetRoom();

            if (roomA.HasValue && roomB.HasValue)
            {

                RoomType typeA = GetRoomType(roomA.Value);
                RoomType typeB = GetRoomType(roomB.Value);

                if (typeA != RoomType.Secret && typeB != RoomType.Secret)
                {
                    CreateCorridor(roomA, roomB);
                }

            }
        }
    }

    public void CarveRoom(RectInt room)
    {
        rooms.Add(room);

        RoomType currentRoomType = RoomType.Normal;
        if (rooms.Count == 1)
        {
            currentRoomType = RoomType.Start;
            playerSpawnRoom = room;
        }

        if (!roomTypes.ContainsKey(room))
        {
            roomTypes.Add(room, currentRoomType);
        }

        for (int x = room.x + 1; x < room.xMax - 1; x++)
        {
            for (int y = room.y + 1; y < room.yMax - 1; y++)
            {
                mapGrid[x, y] = 1;
            }
        }
    }

    public void CreateCorridor(RectInt? roomA, RectInt? roomB)
    {
        if (roomA == null || roomB == null) return;
        Vector2Int pointA = Vector2Int.RoundToInt(roomA.Value.center);
        Vector2Int pointB = Vector2Int.RoundToInt(roomB.Value.center);

        int H_corridorExtender = Random.Range(0, 2) == 0 ? -1 : 1;
        int V_corridorExtender = Random.Range(0, 2) == 0 ? -1 : 1;

        for (int x = Mathf.Min(pointA.x, pointB.x); x <= Mathf.Max(pointA.x, pointB.x); x++)
        {
            for (int i = 0; i < corridorSize; i++) mapGrid[x, pointA.y + H_corridorExtender * i] = 1;
        }
        for (int y = Mathf.Min(pointA.y, pointB.y); y <= Mathf.Max(pointA.y, pointB.y); y++)
        {
            for (int i = 0; i < corridorSize; i++) mapGrid[pointB.x + V_corridorExtender * i, y] = 1;
        }
    }

    private void DrawMap()
    {

        if (biomeThemes.Count == 0)
        {
            Debug.LogError("List 'biomeThemes' di LevelGenerator masih kosong! Harap isi di Inspector.");
            return;
        }

        if (currentBiomeOrder == null || currentBiomeOrder.Count == 0) RandomizeBiomeOrder();

        int biomeLevel = ((currentFloor - 1) / floorPerBiome) + 1;

        int rawIndex = (biomeLevel - 1) % biomeThemes.Count;

        int shuffledIndex = currentBiomeOrder[rawIndex];

        BiomeTheme currentActiveTheme = biomeThemes[shuffledIndex];

        this.currentActiveTheme = currentActiveTheme;
        TileBase currentFloorTile = currentActiveTheme.floorTile;
        RuleTile currentWallRuleTile = currentActiveTheme.wallBaseRuleTile;

        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        hiddenTiles.Clear();

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int currentPosInt = new Vector3Int(x, y, 0);
                Vector2Int currentPos2D = new Vector2Int(x, y);
                bool isSecretAreaTile = secretFloorAndCorridorTiles.Contains(currentPos2D);

                if (mapGrid[x, y] == 1)
                {
                    if (isSecretAreaTile)
                    {

                        RectInt? ownerRoom = FindOwnerRoom(currentPos2D);
                        if (!ownerRoom.HasValue) ownerRoom = FindClosestSecretRoom(currentPos2D);
                        if (ownerRoom.HasValue)
                        {
                            if (!hiddenTiles.ContainsKey(ownerRoom.Value)) hiddenTiles[ownerRoom.Value] = new List<(Vector3Int, TileBase, bool)>();
                            hiddenTiles[ownerRoom.Value].Add((currentPosInt, currentFloorTile, false));
                        }
                        wallTilemap.SetTile(currentPosInt, currentWallRuleTile);
                    }
                    else
                    {

                        floorTilemap.SetTile(currentPosInt, currentFloorTile);
                    }

                    bool isAdjacentToNormalFloor = false;
                    bool isAdjacentToSecretFloor = false;
                    Vector2Int[] neighbors4Dir = { currentPos2D + Vector2Int.up, currentPos2D + Vector2Int.down, currentPos2D + Vector2Int.left, currentPos2D + Vector2Int.right };
                    foreach (Vector2Int neighborPos in neighbors4Dir)
                    {
                        if (neighborPos.x < 0 || neighborPos.x >= mapWidth || neighborPos.y < 0 || neighborPos.y >= mapHeight) continue;
                        if (mapGrid[neighborPos.x, neighborPos.y] == 1)
                        {
                            if (secretFloorAndCorridorTiles.Contains(neighborPos)) isAdjacentToSecretFloor = true;
                            else isAdjacentToNormalFloor = true;
                        }
                    }
                    if (isAdjacentToNormalFloor && isAdjacentToSecretFloor && isSecretAreaTile)
                    {
                        floorTilemap.SetTile(currentPosInt, currentFloorTile);

                        if (destructibleWallPrefab != null)
                        {
                            RectInt? adjacentSecretRectOwner = FindClosestSecretRoom(currentPos2D);
                            Vector3 wallSpawnPos = currentPosInt + new Vector3(0.5f, 0.5f, 0);
                            GameObject wall = Instantiate(destructibleWallPrefab, wallSpawnPos, Quaternion.identity);

                            DestructibleSecretWall wallScript = wall.GetComponent<DestructibleSecretWall>();
                            if (wallScript != null && adjacentSecretRectOwner.HasValue) wallScript.associatedRoom = adjacentSecretRectOwner.Value;
                            spawnedObjects.Add(wall);
                        }
                    }
                }
                else if (mapGrid[x, y] == 0)
                {

                    if (isSecretAreaTile)
                    {

                        RectInt? ownerRoom = FindOwnerRoom(currentPos2D);
                        if (!ownerRoom.HasValue) ownerRoom = FindClosestSecretRoom(currentPos2D);
                        if (ownerRoom.HasValue)
                        {
                            if (!hiddenTiles.ContainsKey(ownerRoom.Value)) hiddenTiles[ownerRoom.Value] = new List<(Vector3Int, TileBase, bool)>();
                            hiddenTiles[ownerRoom.Value].Add((currentPosInt, currentFloorTile, false));

                            hiddenTiles[ownerRoom.Value].Add((currentPosInt, currentWallRuleTile, true));
                        }
                        wallTilemap.SetTile(currentPosInt, currentWallRuleTile);
                    }
                    else
                    {

                        floorTilemap.SetTile(currentPosInt, currentFloorTile);

                        wallTilemap.SetTile(currentPosInt, currentWallRuleTile);
                    }
                }
            }
        }
    }

    private RectInt? FindClosestSecretRoom(Vector2Int tilePos)
    {
        RectInt? closestRoom = null;
        float minDistance = Mathf.Infinity;

        foreach (var kvp in roomTypes)
        {
            if (kvp.Value == RoomType.Secret)
            {
                float dist = Vector2.Distance(tilePos, kvp.Key.center);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestRoom = kvp.Key;
                }
            }
        }
        return closestRoom;
    }

    private void MarkAllSecretAreaTiles()
    {

        secretFloorAndCorridorTiles.Clear();

        Queue<Vector2Int> tilesToExplore = new Queue<Vector2Int>();

        foreach (var kvp in roomTypes)
        {
            if (kvp.Value == RoomType.Secret)
            {
                RectInt secretRect = kvp.Key;
                for (int x = secretRect.xMin + 1; x < secretRect.xMax - 1; x++)
                {
                    for (int y = secretRect.yMin + 1; y < secretRect.yMax - 1; y++)
                    {
                        Vector2Int pos = new Vector2Int(x, y);

                        if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight && mapGrid[x, y] == 1)
                        {
                            if (!secretFloorAndCorridorTiles.Contains(pos))
                            {
                                secretFloorAndCorridorTiles.Add(pos);
                                tilesToExplore.Enqueue(pos);
                            }
                        }
                    }
                }
            }
        }

        Vector2Int[] neighbors4Dir = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (tilesToExplore.Count > 0)
        {
            Vector2Int current = tilesToExplore.Dequeue();
            foreach (Vector2Int offset in neighbors4Dir)
            {
                Vector2Int neighborPos = current + offset;

                if (neighborPos.x < 0 || neighborPos.x >= mapWidth || neighborPos.y < 0 || neighborPos.y >= mapHeight)
                    continue;

                if (mapGrid[neighborPos.x, neighborPos.y] == 1 && !secretFloorAndCorridorTiles.Contains(neighborPos))
                {

                    RectInt? owner = FindOwnerRoom(neighborPos);

                    if (!owner.HasValue)
                    {

                        secretFloorAndCorridorTiles.Add(neighborPos);
                        tilesToExplore.Enqueue(neighborPos);
                    }

                }
            }
        }

        Debug.Log($"<color=yellow>MarkAllSecretAreaTiles: Menandai {secretFloorAndCorridorTiles.Count} tile rahasia.</color>");
    }

    public RectInt? FindOwnerRoom(Vector2Int tilePos)
    {
        foreach (RectInt room in rooms)
        {

            if (tilePos.x > room.x && tilePos.x < room.xMax - 1 &&
                tilePos.y > room.y && tilePos.y < room.yMax - 1)
            {

                if (mapGrid[tilePos.x, tilePos.y] == 1)
                {
                    return room;
                }
            }
        }
        return null;
    }

    private RectInt? GetContainingSecretRoom(Vector3Int tilePos)
    {
        foreach (var kvp in roomTypes)
        {
            if (kvp.Value == RoomType.Secret)
            {
                RectInt secretRect = kvp.Key;

                RectInt expandedRect = new RectInt(secretRect.x - corridorSize, secretRect.y - corridorSize,
                                                  secretRect.width + corridorSize * 2, secretRect.height + corridorSize * 2);

                if (expandedRect.Contains(new Vector2Int(tilePos.x, tilePos.y)))
                {

                    if (mapGrid[tilePos.x, tilePos.y] == 1)
                    {
                        return secretRect;
                    }
                }
            }
        }
        return null;
    }

    void SpawnPlayer()
    {
        if (rooms.Count > 0)
        {

            int tileX = playerSpawnRoom.x + (playerSpawnRoom.width / 2);
            int tileY = playerSpawnRoom.y + (playerSpawnRoom.height / 2);
            Vector3 spawnPosition = new Vector3(tileX + 0.5f, tileY + 0.5f, 0);

            if (playerInstance == null)
            {

                PlayerController existingPlayer = FindObjectOfType<PlayerController>();

                if (existingPlayer != null)
                {
                    playerInstance = existingPlayer.gameObject;
                }
            }

            if (playerInstance == null)
            {

                playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
                DontDestroyOnLoad(playerInstance);
            }
            else
            {

                Rigidbody2D rb = playerInstance.GetComponent<Rigidbody2D>();
                if (rb != null) rb.simulated = false;

                playerInstance.transform.position = spawnPosition;

                if (rb != null)
                {
                    rb.simulated = true;
                    rb.linearVelocity = Vector2.zero;
                }
            }

            Debug.Log($"Player diposisikan di Start Room: {spawnPosition}");
        }
        else
        {
            Debug.LogError("Error: Tidak ada ruangan untuk spawn player.");
        }
    }

    int SpawnEnemies(bool isBossFloor = false)
    {
        int totalEnemiesSpawnedCount = 0;

        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return 0;

        float targetDamageInterval = 1.0f;

        int biomeLevel = ((currentFloor - 1) / floorPerBiome) + 1;

        PlayerProfile profile = this.nextFloorProfile;

        float enemyMultiplier = 1.0f;
        bool favorRanged = false;

        switch (profile)
        {
            case PlayerProfile.Aggressive:
                enemyMultiplier = 1.5f;
                targetDamageInterval = 0.5f;
                Debug.Log("DDA: Aggressive Profile -> Spawn Lebih Banyak Musuh");
                break;

            case PlayerProfile.Passive:
                enemyMultiplier = 1.5f;
                favorRanged = true;
                Debug.Log("DDA: Passive Profile -> Spawn Banyak Musuh Ranged");
                break;

            case PlayerProfile.Explorer:
                enemyMultiplier = 1.0f;
                Debug.Log("DDA: Explorer Profile -> Spawn Normal");
                break;

            case PlayerProfile.Speedrunner:
                enemyMultiplier = 1.8f;
                Debug.Log("DDA: Speedrunner Profile -> Spawn 'Traffic Jam' (Sangat Banyak)");
                break;
        }

        Dictionary<string, float> weightsToSend = ddaWeightDatabase[this.nextFloorProfile];

        if (this.nextFloorProfile == PlayerProfile.Aggressive)
        {
            targetDamageInterval = 0.5f;
            Debug.Log("<color=red>DDA: Musuh menjadi lebih ganas (Attack Speed UP)!</color>");
        }

        foreach (RectInt currentRoom in rooms)
        {

            RoomType type = RoomType.Normal;
            if (roomTypes.ContainsKey(currentRoom))
            {
                type = roomTypes[currentRoom];
            }

            if (type == RoomType.Boss)
            {

                if (bossPrefabs != null && bossPrefabs.Length > 0)
                {
                    GameObject selectedBossPrefab = bossPrefabs[Random.Range(0, bossPrefabs.Length)];
                    Vector3 spawnCenter = new Vector3(currentRoom.center.x + 0.5f, currentRoom.center.y + 0.5f, 0);
                    GameObject boss = Instantiate(selectedBossPrefab, spawnCenter, Quaternion.identity);
                    spawnedObjects.Add(boss);

                    Health bossHealth = boss.GetComponentInChildren<Health>();
                    if (bossHealth != null)
                    {
                        bossHealth.ApplyBiomeScaling(biomeLevel);
                    }

                    EnemyRoomKeeper bossKeeper = boss.GetComponent<EnemyRoomKeeper>();
                    if (bossKeeper != null) bossKeeper.Initialize(currentRoom);
                }
                else { Debug.LogError("Array 'bossPrefabs' di LevelGenerator belum diisi!"); }

                GameObject trigger = SpawnLockingTrigger(currentRoom, lockingRoomTriggerPrefab);
                if (trigger != null)
                {
                    LockingRoomTrigger lockScript = trigger.GetComponent<LockingRoomTrigger>();
                    if (lockScript != null)
                    {

                        lockScript.overrideMusic = currentActiveTheme.bossBGM;
                    }
                }
                continue;
            }

            if (type == RoomType.Start || type == RoomType.Exit || type == RoomType.RestStop || type == RoomType.Secret)
            {
                continue;
            }

            int enemiesToSpawn = enemiesPerRoom;

            switch (type)
            {
                case RoomType.Combat:
                    enemiesToSpawn = Mathf.CeilToInt(enemiesPerRoom * 2f);
                    Debug.Log($"Combat Room: Spawning {enemiesToSpawn} musuh.");

                    break;

                case RoomType.Trap:
                    enemiesToSpawn = Mathf.FloorToInt(enemiesPerRoom * 0.5f);

                    GameObject trapObj = SpawnLockingTrigger(currentRoom, trapPrefab);

                    if (trapObj != null)
                    {
                        TrapRoomController trapScript = trapObj.GetComponent<TrapRoomController>();
                        if (trapScript != null)
                        {

                            int randomTrap = Random.Range(1, 3);
                            trapScript.trapType = (TrapRoomController.TrapType)randomTrap;

                            Debug.Log($"Trap Room dibuat: {(TrapRoomController.TrapType)randomTrap}");
                        }
                    }
                    break;

                case RoomType.Treasure:
                    enemiesToSpawn = Mathf.CeilToInt(enemiesPerRoom * 1.5f);
                    Debug.Log($"Treasure Room: Spawning {enemiesToSpawn} musuh.");
                    break;

                case RoomType.Normal:
                    enemiesToSpawn = enemiesPerRoom;
                    break;

                default:
                    enemiesToSpawn = enemiesPerRoom;
                    break;
            }

            enemiesToSpawn = Mathf.CeilToInt(enemiesToSpawn * enemyMultiplier);

            for (int j = 0; j < enemiesToSpawn; j++)
            {
                Vector3 spawnPosition = new Vector3(
                    Random.Range(currentRoom.x + 1, currentRoom.xMax - 1) + 0.5f,
                    Random.Range(currentRoom.y + 1, currentRoom.yMax - 1) + 0.5f,
                    0);

                GameObject randomEnemyPrefab;

                if (favorRanged && Random.value < 0.6f)
                {

                    randomEnemyPrefab = System.Array.Find(enemyPrefabs, prefab => prefab.name.Contains("Ranged"));
                    if (randomEnemyPrefab == null)
                    {
                        randomEnemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                    }
                }
                else
                {

                    randomEnemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                }

                GameObject enemy = Instantiate(randomEnemyPrefab, spawnPosition, Quaternion.identity);
                spawnedObjects.Add(enemy);

                totalEnemiesSpawnedCount++;

                EnemyDamage dmgScript = enemy.GetComponent<EnemyDamage>();
                if (dmgScript != null)
                {
                    dmgScript.damageInterval = targetDamageInterval;
                }

                Health enemyHealth = enemy.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.ApplyBiomeScaling(biomeLevel);
                }

                BehaviourTreeRunner btRunner = enemy.GetComponent<BehaviourTreeRunner>();

                if (btRunner != null)
                {

                    btRunner.UpdateDDAWeights(weightsToSend);
                }
                else
                {
                    Debug.LogWarning($"Musuh {enemy.name} di-spawn tanpa BehaviourTreeRunner!");
                }

                EnemyRoomKeeper roomKeeper = enemy.GetComponent<EnemyRoomKeeper>();
                if (roomKeeper != null)
                {
                    roomKeeper.Initialize(currentRoom);
                }
            }
        }

        return totalEnemiesSpawnedCount;
    }

    public void GoToNextFloor()
    {
        StartCoroutine(TransitionToNextFloorSequence());
    }

    private IEnumerator TransitionToNextFloorSequence()
    {

        if (levelFader != null)
        {
            yield return levelFader.FadeOut();
        }

        if (!objectiveCompleted)
        {
            Debug.LogWarning("Objektif belum selesai!");
            yield break;
        }

        currentFloor++;
        GenerateLevel();

        yield return new WaitForSeconds(0.5f);

        if (levelFader != null)
        {
            yield return levelFader.FadeIn();
        }
    }

    private void ClearLevel()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();
    }

    public void MarkObjectiveComplete()
    {
        objectiveCompleted = true;
        Debug.Log($"Objektif Lantai {currentFloor - 1} Selesai. Exit Portal Aktif.");

        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                ExitPortal portalScript = obj.GetComponent<ExitPortal>();
                if (portalScript != null)
                {
                    portalScript.UnlockPortal();
                }
            }
        }
    }

    private void DetermineSpawnRooms(bool isBossFloor = false)
    {
        if (rooms.Count == 0) return;

        playerSpawnRoom = rooms[0];

        if (roomTypes.ContainsKey(playerSpawnRoom))
            roomTypes[playerSpawnRoom] = RoomType.Start;
        else
            roomTypes.Add(playerSpawnRoom, RoomType.Start);

        exitSpawnRoom = playerSpawnRoom;
        int secretRoomCount = 0;
        int treasureRoomCount = 0;
        int restStopCount = 0;
        if (rooms.Count > 1)
        {
            float maxDistance = 0f;
            Vector2 spawnCenter = playerSpawnRoom.center;

            foreach (RectInt room in rooms)
            {
                if (room == playerSpawnRoom) continue;
                float distance = Vector2.Distance(spawnCenter, room.center);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    exitSpawnRoom = room;
                }
            }

            if (isBossFloor)
            {

                roomTypes[exitSpawnRoom] = RoomType.Boss;
                Debug.Log($"<color=red>Ruangan {exitSpawnRoom.position} menjadi BOSS ROOM.</color>");
                objectiveCompleted = false;
            }
            else
            {

                if (roomTypes.ContainsKey(exitSpawnRoom))
                {
                    roomTypes[exitSpawnRoom] = RoomType.Exit;
                }
                else
                {
                    roomTypes.Add(exitSpawnRoom, RoomType.Exit);
                }
            }

            PlayerProfile profile = this.nextFloorProfile;
            Debug.Log($"<color=cyan>LevelGenerator:</color> Menyesuaikan probabilitas ruangan untuk tipe {profile}...");

            float localRestStopChance = restStopChance;
            float localCombatRoomChance = combatRoomChance;
            float localSecretRoomChance = secretRoomChance;
            float localTrapRoomChance = trapRoomChance;

            switch (profile)
            {
                case PlayerProfile.Explorer:

                    localSecretRoomChance = 0.4f;
                    localCombatRoomChance *= 0.5f;
                    break;

                case PlayerProfile.Speedrunner:

                    localSecretRoomChance = 0.05f;
                    localTrapRoomChance *= 2.0f;
                    break;

                case PlayerProfile.Aggressive:

                    localCombatRoomChance *= 2.0f;
                    break;

                case PlayerProfile.Passive:

                    localRestStopChance = 0.2f;
                    break;
            }

            int randomTreasureRoomIndex = Random.Range(1, rooms.Count-1);

            for (int i = 0; i < rooms.Count; i++)
            {
                RectInt currentRoom = rooms[i];

                if (currentRoom == playerSpawnRoom || currentRoom == exitSpawnRoom) continue;

                if (!roomTypes.ContainsKey(currentRoom) || roomTypes[currentRoom] == RoomType.Normal)
                {

                    if (!isBossFloor && treasureRoomCount < 1 && i == randomTreasureRoomIndex)
                    {
                        roomTypes[currentRoom] = RoomType.Treasure;
                        Debug.Log($"Ruangan {i} menjadi Treasure Room.");
                        treasureRoomCount++;
                        continue;
                    }

                    if (Random.value < localSecretRoomChance && secretRoomCount < 1)
                    {
                        roomTypes[currentRoom] = RoomType.Secret;
                        Debug.Log($"Ruangan {i} menjadi Secret Room.");
                        secretRoomCount++;
                        continue;
                    }

                    if (Random.value < localTrapRoomChance)
                    {
                        roomTypes[currentRoom] = RoomType.Trap;
                        Debug.Log($"Ruangan {i} menjadi Trap Room.");
                        continue;
                    }

                    if (Random.value < localRestStopChance && restStopCount < 3)
                    {
                        roomTypes[currentRoom] = RoomType.RestStop;
                        Debug.Log($"Ruangan {i} menjadi RestStop.");
                        restStopCount++;
                        continue;
                    }

                    if (Random.value < localCombatRoomChance)
                    {
                        roomTypes[currentRoom] = RoomType.Combat;
                        Debug.Log($"Ruangan {i} menjadi Combat Room.");
                        continue;
                    }

                    if (!roomTypes.ContainsKey(currentRoom))
                    {
                        roomTypes.Add(currentRoom, RoomType.Normal);
                    }

                    Debug.Log($"Ternyata Ruangan {i} menjadi {roomTypes[currentRoom]} Room.");
                }
            }
        }
    }

    void SpawnExitPortal()
    {
        bool isBossFloor = (currentFloor % floorPerBiome == 0);

        if (!isBossFloor && rooms.Count > 1 && exitPortalPrefab != null)
        {
            Vector2 portalPosition = exitSpawnRoom.center;
            GameObject portal = Instantiate(exitPortalPrefab, new Vector3(portalPosition.x + 0.5f, portalPosition.y + 0.5f, 0), Quaternion.identity);
            spawnedObjects.Add(portal);

            Collider2D roomCol = Physics2D.OverlapPoint(exitSpawnRoom.center, LayerMask.GetMask("Default"));
            if (roomCol != null)
            {
                RoomVisitTrigger rc = roomCol.GetComponent<RoomVisitTrigger>();
                if (rc != null) rc.isExitRoom = true;
            }
        }
        else if (isBossFloor)
        {
            Debug.Log("Ini Lantai Boss, Exit Portal tidak di-spawn.");
        }

    }

    private void SpawnRoomObjects()
    {
        hiddenObjects.Clear();

        foreach (var kvp in roomTypes)
        {
            RectInt room = kvp.Key;
            RoomType type = kvp.Value;
            Vector3 roomCenter = new Vector3(room.center.x + 0.5f, room.center.y + 0.5f, 0);

            switch (type)
            {
                case RoomType.RestStop:
                    if (shrinePrefab != null)
                    {
                        GameObject shrine = Instantiate(shrinePrefab, roomCenter, Quaternion.identity);
                        spawnedObjects.Add(shrine);
                        Debug.Log("Shrine spawned in RestStop.");
                    }
                    break;

                case RoomType.Trap:
                    if (trapPrefab != null)
                    {
                        GameObject trap = Instantiate(trapPrefab, roomCenter, Quaternion.identity);
                        spawnedObjects.Add(trap);
                        Debug.Log("Trap built in Trap Room.");
                    }
                    break;

                case RoomType.Treasure:
                    if (treasureChestPrefab != null)
                    {
                        GameObject chestObj = Instantiate(treasureChestPrefab, roomCenter, Quaternion.identity);
                        TreasureChest chestScript = chestObj.GetComponent<TreasureChest>();

                        if (chestScript != null)
                        {
                            chestScript.isObjectiveChest = true;
                            chestScript.contentLoot = null;
                        }

                        spawnedObjects.Add(chestObj);
                        Debug.Log("Objective Chest spawned.");
                    }
                    break;

                case RoomType.Secret:
                    if (treasureChestPrefab != null)
                    {
                        GameObject chestObj = Instantiate(treasureChestPrefab, roomCenter, Quaternion.identity);
                        TreasureChest chestScript = chestObj.GetComponent<TreasureChest>();

                        if (chestScript != null)
                        {

                            chestScript.isObjectiveChest = false;

                            if (possibleLoot != null && possibleLoot.Count > 0)
                            {
                                var loot = GetLootByDDAProfile(nextFloorProfile);
                                chestScript.SetContent(loot);
                            }
                        }

                        chestObj.SetActive(false);
                        if (!hiddenObjects.ContainsKey(room)) hiddenObjects[room] = new List<GameObject>();
                        hiddenObjects[room].Add(chestObj);
                        spawnedObjects.Add(chestObj);
                    }
                    break;
            }
        }
    }

    public void RevealSecretRoom(RectInt roomRect)
    {
        if (PlayerDataTracker.Instance != null)
        {
            PlayerDataTracker.Instance.RecordSecretRoomFound();
        }

        Debug.Log($"Revealing Secret Room at {roomRect.position}...");

        if (hiddenTiles.ContainsKey(roomRect))
        {
            foreach (var tileData in hiddenTiles[roomRect])
            {

                wallTilemap.SetTile(tileData.position, null);

                if (tileData.isWall)
                {
                    wallTilemap.SetTile(tileData.position, tileData.tile);
                }
                else
                {
                    floorTilemap.SetTile(tileData.position, tileData.tile);
                }
            }
            hiddenTiles.Remove(roomRect);
        }

        if (hiddenObjects.ContainsKey(roomRect))
        {
            foreach (GameObject obj in hiddenObjects[roomRect])
            {
                if (obj != null) obj.SetActive(true);
            }
            hiddenObjects.Remove(roomRect);
        }
    }

    private void SetupDDAWeights()
    {
        ddaWeightDatabase = new Dictionary<PlayerProfile, Dictionary<string, float>>();

        ddaWeightDatabase.Add(PlayerProfile.Aggressive, new Dictionary<string, float>
        {
            { "Aggressive", 0.2f },
            { "JagaJarak", 1.9f },
            { "Evading", 1.5f },
            { "SkillOriented", 1.2f }
        });

        ddaWeightDatabase.Add(PlayerProfile.Passive, new Dictionary<string, float>
        {
            { "Aggressive", 1.8f },
            { "JagaJarak", 0.3f },
            { "Evading", 1.0f },
            { "SkillOriented", 1.4f }
        });

        ddaWeightDatabase.Add(PlayerProfile.Explorer, new Dictionary<string, float>
        {
            { "Aggressive", 1.5f },
            { "JagaJarak", 1.3f },
            { "Evading", 0.8f },
            { "SkillOriented", 1.5f }
        });

        ddaWeightDatabase.Add(PlayerProfile.Speedrunner, new Dictionary<string, float>
        {
            { "Aggressive", 1.5f },
            { "JagaJarak", 1.5f },
            { "Evading", 1.8f },
            { "SkillOriented", 1.2f }
        });
    }

    public void SpawnPortalAfterBoss()
    {
        Health[] allCharacters = FindObjectsByType<Health>(FindObjectsSortMode.None);

        int livingBossCount = 0;

        foreach (Health h in allCharacters)
        {

            if (h.characterType == Health.CharacterType.Boss && h.GetCurrentHealth() > 0)
            {
                livingBossCount++;
            }
        }

        if (livingBossCount > 0)
        {
            Debug.Log($"Satu Boss mati, masih ada {livingBossCount} Boss lain tersisa.");
            return;
        }

        Debug.Log("<color=red>Boss dikalahkan! Portal Spawn!</color>");

        if (sanctuaryPortalPrefab != null && exitSpawnRoom != null)
        {
            Vector2 portalPosition = exitSpawnRoom.center;
            GameObject portal = Instantiate(sanctuaryPortalPrefab, new Vector3(portalPosition.x + 0.5f, portalPosition.y + 0.5f, 0), Quaternion.identity);
            spawnedObjects.Add(portal);

            SceneTransferPortal portalScript = portal.GetComponentInChildren<SceneTransferPortal>();
            if (portalScript != null)
            {
                portalScript.targetSceneName = "Sanctuary";
                portalScript.advanceFloor = true;

                Debug.Log("Portal Boss dikonfigurasi: Target Sanctuary & Advance Floor ON.");
            }

            MarkObjectiveComplete();

            if (MusicManager.Instance != null && currentActiveTheme != null)
            {
                if (currentActiveTheme.biomeBGM != null)
                {
                    Debug.Log("Boss Mati: Kembali ke Musik Biome.");
                    MusicManager.Instance.PlayMusic(currentActiveTheme.biomeBGM);
                }
            }
        }
        else
        {
            Debug.LogError("Gagal spawn portal setelah boss! (Prefab/exitSpawnRoom null?)");
        }

        if (PlayerDataTracker.Instance != null)
        {
            PlayerDataTracker.Instance.RecordExitRoomFound();
            PlayerDataTracker.Instance.SetObjectiveComplete();
        }
    }

    private GameObject SpawnLockingTrigger(RectInt room, GameObject prefabToSpawn)
    {
        if (lockingRoomTriggerPrefab == null || lockingWallPrefab == null)
        {
            Debug.LogError("Prefab 'LockingRoomTrigger' atau 'LockingWall' belum di-set di Inspector!");
            return null;
        }

        Vector3 roomCenter = new Vector3(room.center.x + 0.5f, room.center.y + 0.5f, 0);
        GameObject triggerObj = Instantiate(prefabToSpawn, roomCenter, Quaternion.identity);

        triggerObj.GetComponent<BoxCollider2D>().size = new Vector2(room.width - 2, room.height - 2);

        LockingRoomTrigger triggerScript = triggerObj.GetComponent<LockingRoomTrigger>();
        if (triggerScript != null)
        {
            triggerScript.roomBounds = room;
            triggerScript.doorPrefab = this.lockingWallPrefab;
        }

        spawnedObjects.Add(triggerObj);
        return triggerObj;
    }

    public void RandomizeBiomeOrder()
    {
        currentBiomeOrder.Clear();

        for (int i = 0; i < biomeThemes.Count; i++)
        {
            currentBiomeOrder.Add(i);
        }

        for (int i = 0; i < currentBiomeOrder.Count; i++)
        {
            int temp = currentBiomeOrder[i];
            int randomIndex = Random.Range(i, currentBiomeOrder.Count);
            currentBiomeOrder[i] = currentBiomeOrder[randomIndex];
            currentBiomeOrder[randomIndex] = temp;
        }

        Debug.Log("Biome Order Diacak: " + string.Join(", ", currentBiomeOrder));
    }

    public void SetBiomeOrder(List<int> savedOrder)
    {

        if (savedOrder == null || savedOrder.Count != biomeThemes.Count)
        {
            Debug.LogWarning("Saved Biome Order tidak valid/cocok, generate baru.");
            RandomizeBiomeOrder();
        }
        else
        {
            currentBiomeOrder = new List<int>(savedOrder);
            Debug.Log("Biome Order Diload: " + string.Join(", ", currentBiomeOrder));
        }
    }

    public List<int> GetBiomeOrder()
    {
        if (currentBiomeOrder == null || currentBiomeOrder.Count == 0)
            RandomizeBiomeOrder();

        return currentBiomeOrder;
    }

    public int GetCurrentFloor()
    {
        return currentFloor;
    }

    public void SetCurrentFloor(int floor)
    {
        currentFloor = floor;
        Debug.Log($"LevelGenerator: Floor diset ke {currentFloor} dari Save Data.");
    }

    public int GetCurrentBiomeLevel()
    {

        return Mathf.Max(1, (currentFloor - 1) / floorPerBiome) + 1;
    }

    private PlayerController.LootData GetLootByDDAProfile(PlayerProfile profile)
    {

        List<PlayerController.LootData> filteredList = new List<PlayerController.LootData>(possibleLoot);

        switch (profile)
        {
            case PlayerProfile.Aggressive:

                if (possibleLoot.Exists(x => x.itemType == PlayerController.LootData.ItemType.Weapon))
                {
                    filteredList = possibleLoot.FindAll(x => x.itemType == PlayerController.LootData.ItemType.Weapon);
                }
                break;

            case PlayerProfile.Passive:
            case PlayerProfile.Speedrunner:

                if (possibleLoot.Exists(x => x.itemType == PlayerController.LootData.ItemType.Skill))
                {
                    filteredList = possibleLoot.FindAll(x => x.itemType == PlayerController.LootData.ItemType.Skill);
                }
                break;

            case PlayerProfile.Explorer:

                filteredList = possibleLoot;
                break;
        }

        if (filteredList.Count == 0) return possibleLoot[Random.Range(0, possibleLoot.Count)];

        return filteredList[Random.Range(0, filteredList.Count)];
    }

    void SpawnRoomVisitTriggers()
    {
        if (roomVisitTriggerPrefab == null) return;

        foreach (RectInt room in rooms)
        {

            Vector3 roomCenter = new Vector3(room.center.x + 0.5f, room.center.y + 0.5f, 0);

            GameObject trigger = Instantiate(roomVisitTriggerPrefab, roomCenter, Quaternion.identity);
            spawnedObjects.Add(trigger);

            BoxCollider2D col = trigger.GetComponent<BoxCollider2D>();
            if (col != null)
            {

                col.size = new Vector2(room.width - 2f, room.height - 2f);
            }
        }
    }
}

public enum RoomType
{
    Normal,
    Start,
    Exit,
    RestStop,
    Combat,
    Treasure,
    Boss,
    Trap,
    Secret
}