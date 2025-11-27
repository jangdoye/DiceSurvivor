using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomBoxSpawner : MonoBehaviour
{
    #region Variables
    public List<GameObject> boxPrefabs; // 여러 종류의 박스
    public float minSpawnDelay = 10f;   // 최소 대기 시간
    public float maxSpawnDelay = 30f;   // 최대 대기 시간
    public List<Transform> spawnPoints; // 상자 스폰 가능한 위치들

    public Transform player;

    private GameObject currentBox;
    #endregion

    #region Custom Method
    void Start()
    {
        StartCoroutine(SpawnBoxRoutine());
    }
    #endregion

    #region Custom Method
    IEnumerator SpawnBoxRoutine()
    {
        while (true)
        {
            // 일정 시간 랜덤 대기
            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(delay);

            SpawnBox();
        }
    }

    public  void SpawnBox()
    {
        if (boxPrefabs.Count == 0) return;

        // 랜덤 프리팹 선택
        GameObject selectedBox = boxPrefabs[Random.Range(0, boxPrefabs.Count)];

        // 플레이어 주변 랜덤 위치 계산
        Vector3 playerPos = player.position;
        float radius = 30f;
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * radius;
        Vector3 spawnPos = new Vector3(playerPos.x + randomCircle.x, playerPos.y, playerPos.z + randomCircle.y);

        currentBox = Instantiate(selectedBox, spawnPos, Quaternion.identity);

        Box boxScript = currentBox.GetComponent<Box>();
        if (boxScript != null)
            boxScript.onBoxDestroyed += OnBoxDestroyed;
    }

    public void OnBoxDestroyed()
    {
        currentBox = null;
    }
    #endregion
}
