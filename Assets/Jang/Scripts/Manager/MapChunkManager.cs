using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapChunkManager : MonoBehaviour
{
    #region Variables
    public static MapChunkManager Instance { get; private set; }

    [Header("Tile")]
    [SerializeField] private GameObject[] tilePrefabs;
    [SerializeField] private int chunkSize = 20;
    [SerializeField] private float activationRange = 50f;

    // 필요하면 켜세요(매니저 유지). 켜더라도 씬 로드마다 클리어하므로 진행은 초기화됨.
    [SerializeField] private bool dontDestroyOnLoad = false;

    private readonly Dictionary<Vector2Int, GameObject> tilePool = new();

    public int GetChunkSize() => chunkSize;
    public float GetActivationRange() => activationRange;
    #endregion

    #region Unity Event Method
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 들어올 때마다 기존 타일을 깨끗이 정리
        ClearAllTiles();
    }
    #endregion

    #region Custom Method
    public GameObject GetOrCreateTileAt(Vector2Int coord)
    {
        if (tilePool.TryGetValue(coord, out GameObject tile))
        {
            // 이미 파괴되었으면 풀에서 제거하고 새로 만든다
            if (tile == null)
            {
                tilePool.Remove(coord);
            }
            else
            {
                tile.SetActive(true);
                return tile;
            }
        }

        // 새 타일 생성
        GameObject prefab = tilePrefabs[Random.Range(0, tilePrefabs.Length)];
        Vector3 position = new Vector3(coord.x * chunkSize, 0f, coord.y * chunkSize);
        GameObject newTile = Instantiate(prefab, position, Quaternion.identity);
        newTile.name = $"Tile_{coord.x}_{coord.y}";

        tilePool[coord] = newTile;
        return newTile;
    }

    public void DeactivateTile(Vector2Int coord)
    {
        if (tilePool.TryGetValue(coord, out GameObject tile))
        {
            if (tile != null) tile.SetActive(false);
        }
    }

    public void ClearAllTiles()
    {
        foreach (var tile in tilePool.Values)
        {
            if (tile != null) Destroy(tile);
        }
        tilePool.Clear();
    }
    #endregion
}
