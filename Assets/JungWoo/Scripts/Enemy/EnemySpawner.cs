/*using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace DiceSurvivor.Test
{
    /// <summary>
    /// 적 스포너 - 적을 생성하고 관리
    /// </summary>
    public class WJEnemySpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject enemyPrefab;              // 적 프리팹
        [SerializeField] private float spawnInterval = 2f;            // 스폰 간격
        [SerializeField] private float spawnRadius = 10f;             // 스폰 반경
        [SerializeField] private int maxEnemies = 20;                 // 최대 적 수
        [SerializeField] private bool autoSpawn = true;               // 자동 스폰 여부

        [Header("Spawn Points")]
        [SerializeField] private Transform[] spawnPoints;             // 고정 스폰 포인트 (옵션)
        [SerializeField] private bool useRandomPosition = true;       // 랜덤 위치 사용

        [Header("Enemy Pool")]
        [SerializeField] private List<GameObject> activeEnemies;      // 활성 적 목록

        [Header("Player Reference")]
        [SerializeField] private Transform player;                    // 플레이어 위치

        private float lastSpawnTime;                                  // 마지막 스폰 시간

        /// <summary>
        /// 초기화
        /// </summary>
        void Start()
        {
            // 플레이어 찾기
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }

            // 리스트 초기화
            activeEnemies = new List<GameObject>();

            // 적 프리팹이 없으면 기본 생성
            if (enemyPrefab == null)
            {
                CreateDefaultEnemyPrefab();
            }

            // 자동 스폰 시작
            if (autoSpawn)
            {
                StartCoroutine(AutoSpawnRoutine());
            }
        }

        /// <summary>
        /// 기본 적 프리팹 생성
        /// </summary>
        private void CreateDefaultEnemyPrefab()
        {
            // 기본 적 생성
            enemyPrefab = new GameObject("EnemyPrefab");

            // 큐브 메시 추가
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.transform.SetParent(enemyPrefab.transform);
            visual.transform.localPosition = Vector3.zero;

            // 크기 조정
            visual.transform.localScale = new Vector3(1f, 2f, 1f);

            // 빨간색 머티리얼
            Renderer renderer = visual.GetComponent<Renderer>();
            renderer.material.color = Color.red;

            // Enemy 스크립트 추가
            enemyPrefab.AddComponent<WJEnemy>();

            // Rigidbody 추가
            Rigidbody rb = enemyPrefab.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            // Collider 추가
            BoxCollider collider = enemyPrefab.AddComponent<BoxCollider>();
            collider.size = new Vector3(1f, 2f, 1f);

            // Enemy 레이어 설정
            enemyPrefab.layer = LayerMask.NameToLayer("Enemy");

            // 프리팹 비활성화
            enemyPrefab.SetActive(false);
        }

        /// <summary>
        /// 자동 스폰 루틴
        /// </summary>
        private IEnumerator AutoSpawnRoutine()
        {
            while (autoSpawn)
            {
                // 최대 적 수 체크
                CleanupDeadEnemies();

                if (activeEnemies.Count < maxEnemies)
                {
                    SpawnEnemy();
                }

                // 스폰 간격 대기
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        /// <summary>
        /// 적 스폰
        /// </summary>
        public void SpawnEnemy()
        {
            if (enemyPrefab == null) return;

            // 스폰 위치 결정
            Vector3 spawnPosition = GetSpawnPosition();

            // 적 생성
            GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            newEnemy.SetActive(true);
            newEnemy.name = $"Enemy_{activeEnemies.Count + 1}";

            // 리스트에 추가
            activeEnemies.Add(newEnemy);

            //Debug.Log($"적 스폰됨: {newEnemy.name} at {spawnPosition}");
        }

        /// <summary>
        /// 스폰 위치 결정
        /// </summary>
        private Vector3 GetSpawnPosition()
        {
            Vector3 spawnPos = Vector3.zero;

            if (useRandomPosition && player != null)
            {
                // 플레이어 주변 랜덤 위치
                Vector2 randomCircle = Random.insideUnitCircle.normalized * spawnRadius;
                spawnPos = player.position + new Vector3(randomCircle.x, 0, randomCircle.y);
            }
            else if (spawnPoints != null && spawnPoints.Length > 0)
            {
                // 고정 스폰 포인트 중 랜덤 선택
                Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                spawnPos = randomPoint.position;
            }
            else
            {
                // 기본 위치
                spawnPos = transform.position + Random.insideUnitSphere * spawnRadius;
                spawnPos.y = 0;
            }

            return spawnPos;
        }

        /// <summary>
        /// 여러 적 스폰
        /// </summary>
        public void SpawnMultipleEnemies(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (activeEnemies.Count >= maxEnemies) break;
                SpawnEnemy();
            }
        }

        /// <summary>
        /// 죽은 적 정리
        /// </summary>
        private void CleanupDeadEnemies()
        {
            activeEnemies.RemoveAll(enemy => enemy == null);
        }

        /// <summary>
        /// 모든 적 제거
        /// </summary>
        public void RemoveAllEnemies()
        {
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }
            activeEnemies.Clear();
        }

        /// <summary>
        /// 스폰 시작/중지
        /// </summary>
        public void SetAutoSpawn(bool enable)
        {
            autoSpawn = enable;

            if (enable)
            {
                StartCoroutine(AutoSpawnRoutine());
            }
        }

        /// <summary>
        /// 활성 적 수 반환
        /// </summary>
        public int GetActiveEnemyCount()
        {
            CleanupDeadEnemies();
            return activeEnemies.Count;
        }

        /// <summary>
        /// Gizmo 그리기
        /// </summary>
        void OnDrawGizmosSelected()
        {
            // 스폰 반경 표시
            Gizmos.color = Color.cyan;

            if (player != null)
            {
                Gizmos.DrawWireSphere(player.position, spawnRadius);
            }
            else
            {
                Gizmos.DrawWireSphere(transform.position, spawnRadius);
            }

            // 스폰 포인트 표시
            if (spawnPoints != null)
            {
                Gizmos.color = Color.green;
                foreach (var point in spawnPoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawSphere(point.position, 0.5f);
                    }
                }
            }
        }
    }
}*/