using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Boss : MonoBehaviour
{
    enum BossState
    {
        Moving,
        Casting
    }

    public int maxHealth = 20;
    public float moveDuration = 3f;
    public float castDuration = 4f;
    public float warningDuration = 2f;
    public float moveSpeed = 3f;
    public Vector2 movePadding = new Vector2(0.75f, 0.75f);
    public int castPointCount = 5;
    public Vector2 areaSize = new Vector2(1.5f, 1.5f);
    public Vector2 healthBlockSize = new Vector2(30f, 30f);
    public float healthBlockSpacing = 8f;
    public float healthBarTopOffset = 0.7f;
    public Color healthFilledColor = new Color(0.85f, 0.15f, 0.15f, 1f);
    public Color healthEmptyColor = new Color(0.18f, 0.18f, 0.18f, 0.95f);
    public Color healthFlashColor = new Color(1f, 1f, 0.85f, 1f);
    public GameObject hitVfxPrefab;
    public GameObject warningPrefab;
    public GameObject damagePrefab;

    static Sprite runtimeMarkerSprite;

    Camera cachedCamera;
    BossState currentState;
    int currentHealth;
    Vector2 minBounds;
    Vector2 maxBounds;
    Vector3 currentMoveTarget;
    Transform healthBarRoot;
    Coroutine[] healthFlashCoroutines;
    readonly List<GameObject> activeWarnings = new List<GameObject>();
    readonly List<GameObject> activeDamageAreas = new List<GameObject>();
    readonly List<SpriteRenderer> healthBlockRenderers = new List<SpriteRenderer>();

    void Awake()
    {
        currentHealth = maxHealth;
        EnsureTriggerCollider();
        CacheCameraBounds();
    }

    void Start()
    {
        currentState = BossState.Moving;
        currentMoveTarget = GetRandomPointInBounds();
        EnsureHealthBar();
        StartCoroutine(BossLoop());
    }

    void OnEnable()
    {
        CacheCameraBounds();
        EnsureHealthBar();
        UpdateHealthBarPosition();
    }

    void LateUpdate()
    {
        if (healthBarRoot == null || cachedCamera == null)
        {
            CacheCameraBounds();
            EnsureHealthBar();
        }

        UpdateHealthBarPosition();
    }

    void OnDestroy()
    {
        ClearSpawnedObjects(activeWarnings);
        ClearSpawnedObjects(activeDamageAreas);
        if (healthBarRoot != null)
        {
            Destroy(healthBarRoot.gameObject);
        }
    }

    IEnumerator BossLoop()
    {
        while (true)
        {
            yield return MoveState();
            yield return CastState();
        }
    }

    IEnumerator MoveState()
    {
        currentState = BossState.Moving;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            if (Vector2.Distance(transform.position, currentMoveTarget) <= 0.1f)
            {
                currentMoveTarget = GetRandomPointInBounds();
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                currentMoveTarget,
                moveSpeed * Time.deltaTime);

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator CastState()
    {
        currentState = BossState.Casting;
        List<Vector3> castPoints = new List<Vector3>(castPointCount);
        for (int i = 0; i < castPointCount; i++)
        {
            castPoints.Add(GetRandomPointInBounds());
        }

        SpawnMarkers(castPoints, warningPrefab, activeWarnings, false, new Color(1f, 0.85f, 0.2f, 0.45f));
        yield return new WaitForSeconds(warningDuration);

        ClearSpawnedObjects(activeWarnings);

        SpawnMarkers(castPoints, damagePrefab, activeDamageAreas, true, new Color(1f, 0.2f, 0.2f, 0.65f));
        yield return new WaitForSeconds(Mathf.Max(0f, castDuration - warningDuration));

        ClearSpawnedObjects(activeDamageAreas);
        currentMoveTarget = GetRandomPointInBounds();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("DarkBullet"))
        {
            return;
        }

        if (hitVfxPrefab != null)
        {
            Instantiate(hitVfxPrefab, transform.position, Quaternion.identity);
        }

        int previousHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - 1);
        UpdateHealthBar();
        FlashLostHealth(previousHealth, currentHealth);
        Destroy(other.gameObject);

        if (currentHealth == 0)
        {
            StopAllCoroutines();
            ClearSpawnedObjects(activeWarnings);
            ClearSpawnedObjects(activeDamageAreas);
            Destroy(gameObject);
        }
    }

    void SpawnMarkers(
        List<Vector3> positions,
        GameObject prefab,
        List<GameObject> cache,
        bool enableDamage,
        Color fallbackColor)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            GameObject marker = prefab != null
                ? Instantiate(prefab, positions[i], Quaternion.identity)
                : CreateFallbackMarker(positions[i], fallbackColor);

            if (enableDamage)
            {
                ConfigureDamageMarker(marker);
            }

            cache.Add(marker);
        }
    }

    GameObject CreateFallbackMarker(Vector3 position, Color color)
    {
        GameObject marker = new GameObject("BossMarker");
        marker.transform.position = new Vector3(position.x, position.y, 0f);
        marker.transform.localScale = new Vector3(areaSize.x, areaSize.y, 1f);

        SpriteRenderer renderer = marker.AddComponent<SpriteRenderer>();
        renderer.sprite = GetRuntimeMarkerSprite();
        renderer.color = color;
        renderer.sortingOrder = 20;

        return marker;
    }

    void ConfigureDamageMarker(GameObject marker)
    {
        Collider2D markerCollider = marker.GetComponent<Collider2D>();
        if (markerCollider == null)
        {
            BoxCollider2D collider = marker.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
            collider.isTrigger = true;
            markerCollider = collider;
        }
        else
        {
            markerCollider.isTrigger = true;
        }

        if (marker.GetComponent<BossDamageArea>() == null)
        {
            marker.AddComponent<BossDamageArea>();
        }
    }

    void ClearSpawnedObjects(List<GameObject> spawnedObjects)
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] != null)
            {
                Destroy(spawnedObjects[i]);
            }
        }

        spawnedObjects.Clear();
    }

    void CacheCameraBounds()
    {
        cachedCamera = Camera.main;

        if (cachedCamera == null)
        {
            Vector3 bossPosition = transform.position;
            minBounds = new Vector2(bossPosition.x - 4f, bossPosition.y - 4f);
            maxBounds = new Vector2(bossPosition.x + 4f, bossPosition.y + 4f);
            return;
        }

        float halfHeight = Mathf.Max(0f, cachedCamera.orthographicSize - movePadding.y);
        float halfWidth = Mathf.Max(0f, (cachedCamera.orthographicSize * cachedCamera.aspect) - movePadding.x);
        Vector3 cameraPosition = cachedCamera.transform.position;

        minBounds = new Vector2(cameraPosition.x - halfWidth, cameraPosition.y - halfHeight);
        maxBounds = new Vector2(cameraPosition.x + halfWidth, cameraPosition.y + halfHeight);
    }

    Vector3 GetRandomPointInBounds()
    {
        float x = Random.Range(minBounds.x, maxBounds.x);
        float y = Random.Range(minBounds.y, maxBounds.y);
        return new Vector3(x, y, transform.position.z);
    }

    void EnsureTriggerCollider()
    {
        Collider2D bossCollider = GetComponent<Collider2D>();
        if (bossCollider != null)
        {
            bossCollider.isTrigger = true;
            return;
        }

        BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null && renderer.sprite != null)
        {
            collider.size = renderer.sprite.bounds.size;
        }
    }

    void CreateHealthBar()
    {
        if (maxHealth <= 0 || healthBarRoot != null)
        {
            return;
        }

        healthBarRoot = new GameObject("BossHealthBar").transform;
        healthFlashCoroutines = new Coroutine[maxHealth];
        float spriteUnitWidth = GetRuntimeMarkerSprite().bounds.size.x;
        float blockWidth = spriteUnitWidth * healthBlockSize.x;
        float spacingWidth = spriteUnitWidth * healthBlockSpacing;
        float totalWidth = (maxHealth * blockWidth) + ((maxHealth - 1) * spacingWidth);
        float startX = (-totalWidth * 0.5f) + (blockWidth * 0.5f);

        for (int i = 0; i < maxHealth; i++)
        {
            GameObject healthBlock = new GameObject("HealthBlock_" + i);
            healthBlock.transform.SetParent(healthBarRoot, false);
            healthBlock.transform.localPosition = new Vector3(
                startX + (i * (blockWidth + spacingWidth)),
                0f,
                0f);
            healthBlock.transform.localScale = new Vector3(healthBlockSize.x, healthBlockSize.y, 1f);

            SpriteRenderer renderer = healthBlock.AddComponent<SpriteRenderer>();
            renderer.sprite = GetRuntimeMarkerSprite();
            renderer.color = i < currentHealth ? healthFilledColor : healthEmptyColor;
            renderer.sortingOrder = 900;
            healthBlockRenderers.Add(renderer);
        }

        UpdateHealthBarPosition();
    }

    void UpdateHealthBar()
    {
        for (int i = 0; i < healthBlockRenderers.Count; i++)
        {
            if (healthBlockRenderers[i] != null)
            {
                healthBlockRenderers[i].color = i < currentHealth ? healthFilledColor : healthEmptyColor;
            }
        }
    }

    void EnsureHealthBar()
    {
        if (healthBarRoot == null)
        {
            CreateHealthBar();
        }
    }

    void UpdateHealthBarPosition()
    {
        if (healthBarRoot == null)
        {
            return;
        }

        if (cachedCamera == null)
        {
            cachedCamera = Camera.main;
        }

        if (cachedCamera == null)
        {
            healthBarRoot.position = new Vector3(0f, 4f, 0f);
            return;
        }

        Vector3 topCenter = cachedCamera.ViewportToWorldPoint(new Vector3(0.5f, 1f, 0f));
        healthBarRoot.position = new Vector3(topCenter.x, topCenter.y - healthBarTopOffset, 0f);
    }

    void FlashLostHealth(int previousHealth, int newHealth)
    {
        for (int i = newHealth; i < previousHealth; i++)
        {
            if (i < 0 || i >= healthBlockRenderers.Count)
            {
                continue;
            }

            if (healthFlashCoroutines[i] != null)
            {
                StopCoroutine(healthFlashCoroutines[i]);
            }

            healthFlashCoroutines[i] = StartCoroutine(FlashHealthBlock(i));
        }
    }

    IEnumerator FlashHealthBlock(int index)
    {
        SpriteRenderer renderer = healthBlockRenderers[index];
        if (renderer == null)
        {
            yield break;
        }

        for (int i = 0; i < 4; i++)
        {
            renderer.color = healthFlashColor;
            yield return new WaitForSeconds(0.08f);
            renderer.color = healthEmptyColor;
            yield return new WaitForSeconds(0.08f);
        }

        renderer.color = healthEmptyColor;
        healthFlashCoroutines[index] = null;
    }

    Sprite GetRuntimeMarkerSprite()
    {
        if (runtimeMarkerSprite == null)
        {
            runtimeMarkerSprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f));
        }

        return runtimeMarkerSprite;
    }
}

public class BossDamageArea : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.root.CompareTag("Player"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
