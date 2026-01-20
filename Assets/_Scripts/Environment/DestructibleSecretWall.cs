using System.Numerics;
using UnityEngine;

public class DestructibleSecretWall : MonoBehaviour
{
    [Header("Wall Settings")]
    public int health = 4;
    public string playerBulletTag = "PlayerBullet";

    [Header("Secret Room Reference")]
    public RectInt associatedRoom;

    private int currentHealth;
    private SpriteRenderer spriteRenderer;
    private LevelGenerator levelGen;
    private int corridorSize;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    void Start()
    {
        currentHealth = health;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        levelGen = LevelGenerator.Instance;
        if (levelGen == null)
        {
            Debug.LogError("DestructibleWall tidak bisa menemukan LevelGenerator!");
            return;
        }

        this.corridorSize = levelGen.minLeafSize;
        BiomeTheme theme = levelGen.GetCurrentBiomeTheme();
        Vector3Int myGridPos = levelGen.wallTilemap.WorldToCell(transform.position);
        Sprite chosenSprite = GetSpriteBasedOnNeighbors(myGridPos, theme.destructibleSprites);
        spriteRenderer.sprite = chosenSprite;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null && col.isTrigger)
        {
            Debug.LogWarning($"Collider di {gameObject.name} masih trigger.", this);
            col.isTrigger = false;
        }
    }

    private Sprite GetSpriteBasedOnNeighbors(Vector3Int myPos, DestructibleSpriteSet spriteSet)
    {
        Vector3Int tempPos = myPos;
        Sprite trueSprite;

        for (int i = 0; i < corridorSize; i++)
        {
            tempPos = myPos;
            tempPos.x += i;
            trueSprite = GetTrueSprite(tempPos, spriteSet);
            if (trueSprite != null) return trueSprite;

            tempPos = myPos;
            tempPos.x -= i;
            trueSprite = GetTrueSprite(tempPos, spriteSet);
            if (trueSprite != null) return trueSprite;

            tempPos = myPos;
            tempPos.y += i;
            trueSprite = GetTrueSprite(tempPos, spriteSet);
            if (trueSprite != null) return trueSprite;

            tempPos = myPos;
            tempPos.y -= i;
            trueSprite = GetTrueSprite(tempPos, spriteSet);
            if (trueSprite != null) return trueSprite;
        }

        return spriteSet.center;
    }

    private Sprite GetTrueSprite(Vector3Int myPos, DestructibleSpriteSet spriteSet)
    {

        bool up = IsWall(myPos.x, myPos.y + 1);
        bool down = IsWall(myPos.x, myPos.y - 1);
        bool left = IsWall(myPos.x - 1, myPos.y);
        bool right = IsWall(myPos.x + 1, myPos.y);

        bool upLeft = IsWall(myPos.x - 1, myPos.y + 1);
        bool upRight = IsWall(myPos.x + 1, myPos.y + 1);
        bool downLeft = IsWall(myPos.x - 1, myPos.y - 1);
        bool downRight = IsWall(myPos.x + 1, myPos.y - 1);

        if ((up && upRight && !upLeft && !left && !right) || (down && downRight && !downLeft && !left && !right))
        {
            return spriteSet.right;
        }
        if ((up && !upRight && upLeft && !left && !right) || (down && !downRight && downLeft && !left && !right))
        {
            return spriteSet.left;
        }
        if ((!up && !down && upLeft && left && !downLeft) || (!up && !down && upRight && right && !downRight))
        {
            return spriteSet.up;
        }
        if ((!up && !down && !upLeft && left && downLeft) || (!up && !down && !upRight && right && downRight))
        {
            return spriteSet.down;
        }

        Debug.Log("Destructible Wall Sprite is null");

        return null;
    }

    private bool IsWall(int x, int y)
    {
        if (x < 0 || x >= levelGen.mapWidth || y < 0 || y >= levelGen.mapHeight)
        {
            return false;
        }

        bool isSecret = levelGen.secretFloorAndCorridorTiles.Contains(new Vector2Int(x, y));
        return levelGen.mapGrid[x, y] == 0 && !isSecret;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerBulletTag))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
    }

    void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Dinding rahasia {associatedRoom.position} sisa HP: {currentHealth}/{health}");

        if (currentHealth <= 0)
        {
            DestroyWallAndReveal();
        }
        else
        {

        }
    }

    void DestroyWallAndReveal()
    {
        Debug.Log($"Dinding rahasia {associatedRoom.position} hancur. Membuka ruangan...");

        if (LevelGenerator.Instance != null)
        {
            LevelGenerator.Instance.RevealSecretRoom(associatedRoom);
        }

        if (PlayerDataTracker.Instance != null)
        {
            PlayerDataTracker.Instance.RecordSecretRoomFound();
        }

        Destroy(gameObject);
    }

    public void SetSprite(Sprite newSprite)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = newSprite;
        }
        else
        {
            Debug.LogError($"DestructibleWall di {transform.position} tidak punya SpriteRenderer!");
        }
    }
}