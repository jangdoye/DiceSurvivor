using UnityEngine;
using System.Collections.Generic;
using DiceSurvivor.Enemy;

namespace DiceSurvivor.Manager {
    /// <summary>
    /// EnemySpawnManager (SingletonManager 상속, EnemyData 활용, 불필요한 자료구조 제거)
    /// </summary>
    public class EnemySpawnManager : SingletonManager<EnemySpawnManager>
    {
        #region Variables
        //오브젝트 풀 싱글톤 참조
        EnemyPoolManager epManager;
        // 플레이어 주변 위치에서 적 스폰해야 하므로 플레이어 위치 참조
        public Transform playerPos;
        [Header("적 스폰 관련 변수")]

        //엘리트 적 출현 조건 : 일정 수 이상 적 사망 or 일정 시간 경과
        public int deadCounter = 0;
        public int eliteSpawnCounter = 200;
        public float eliteTimer = 0f;
        public float eliteCooldown = 120f;
        //적이 생성되는 거리
        public float spawnDistance = 25f;

        public float spawnInterval = 0.2f; // 적 스폰 간격 (초)
        public float spawnTimer = 0f; // 스폰 타이머
        // 3. 웨이브 및 시간/레벨 관리 변수
        public int currentWaveIndex = 0; // 현재 웨이브 인덱스
        public float waveDuration = 60f; // 웨이브 지속 시간 (초)
        public float timeSinceLastWave = 0f; // 마지막 웨이브 이후 경과 시간

        // 스폰된 적들을 관리할 부모 오브젝트들 (PoolManager와 다른 이름 사용)
        public GameObject spawnedEnemyPar;
        public GameObject spawnedEliteEnemyPar;
        public GameObject spawnedBossEnemyPar;

        #endregion

        #region Unity Event Methods
        
        public void ResetSpawn()
        {
            timeSinceLastWave = 0f;
            currentWaveIndex = 0;
            spawnTimer = 0f;
            eliteTimer = 0f;
            deadCounter = 0;
        }

        protected override void Awake()
        {
            base.Awake();
            // 초기 웨이브/적 구성 세팅
            // 적 스폰 스케쥴링 시작
            epManager = EnemyPoolManager.Instance;
            //플레이어는 단 하나만 존재하므로
            playerPos = GameObject.FindGameObjectWithTag("Player").transform;
        }

        void Update()
        {
            //누락 체크
            CheckMissing();
            // 게임 시간/웨이브 진행 체크
            timeSinceLastWave += Time.deltaTime;
            eliteTimer += Time.deltaTime;
            if (timeSinceLastWave >= waveDuration)
            {
                // 웨이브 교체 로직 실행
                ChangeWave(currentWaveIndex + 1);
                timeSinceLastWave = 0f;
            }
            // 주기적 적 스폰 관리
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval && epManager.enemyPool.Count > 0)
            {
                //Debug.Log("적 소환");
                //일반 적 스폰 로직 실행
                SpawnEnemy(epManager.enemyDataArray[currentWaveIndex]);
                spawnTimer = 0f;
                //TODO : 일정 조건을 만족했을 때 엘리트적/보스 적 소환 추가

                //TODO : 엘리트는 일정 이상 적을 처치 or 시간 경과 시 소환
                if (EliteSpawnCond())
                    SpawnEnemy(epManager.eliteEnemyDataArray[currentWaveIndex / 3]);
                //TODO : 보스는 최종 웨이브 시간이 지났을 때 소환
                if (GameTimerManager.Instance.RemainingTime <= 0)
                {
                    SpawnEnemy(epManager.bossEnemyDataArray[0]);
                }
            }
        }
        #endregion
        #region Custom Methods
        
        /// <summary>
        /// 기존 오브젝트를 찾거나 새로 생성하는 메서드
        /// </summary>
        GameObject FindOrCreateObject(string name)
        {
            GameObject existingObj = GameObject.Find(name);
            if (existingObj != null)
            {
                return existingObj;
            }
            else
            {
                return new GameObject(name);
            }
        }
        
        //누락 점검
        void CheckMissing()
        {
            if (epManager == null) epManager = EnemyPoolManager.Instance;
            if (playerPos == null) playerPos = GameObject.FindGameObjectWithTag("Player").transform;
            
            // 스폰된 적들을 관리할 부모 오브젝트들
            if (spawnedEnemyPar == null) spawnedEnemyPar = FindOrCreateObject("SpawnedEnemies");
            if (spawnedEliteEnemyPar == null) spawnedEliteEnemyPar = FindOrCreateObject("SpawnedEliteEnemies");
            if (spawnedBossEnemyPar == null) spawnedBossEnemyPar = FindOrCreateObject("SpawnedBossEnemies");
        }
        
        //소환 가능 여부 점검
        public bool SpawnCondCheck(EnemyData enemyData)
        {
            switch (enemyData.type)
            {
                case EnemyData.EnemyType.Normal:
                    if (epManager.enemyPool.Count > 0)
                    {
                        return true; // 풀에 여유가 있어야 소환 가능
                    }
                    break;
                case EnemyData.EnemyType.Elite:
                    if (epManager.eliteEnemyPool.Count > 0)
                    {
                        return true; // 풀에 여유가 있어야 소환 가능
                    }
                    break;
                case EnemyData.EnemyType.Boss:
                    if (epManager.bossEnemyPool.Count > 0)
                    {
                        return true; // 풀에 여유가 있어야 소환 가능
                    }
                    break;
            }

            return false;
        }

        public bool EliteSpawnCond()
        {
            if (deadCounter >= eliteSpawnCounter || eliteTimer >= eliteCooldown)
            {
                deadCounter = 0;
                eliteTimer = 0f;
                return true;
            }
            return false;
        }

        /// <summary>
        /// EnemyData 기준 적 스폰 (풀에서 꺼내어 위치 지정 및 활성화)
        /// </summary>
        public void SpawnEnemy(EnemyData enemyData)
        {
            if (epManager.poolInitWaiting) return;

            GameObject enemyObj = epManager.SpawnEnemy(enemyData);
            if(enemyObj == null)
            {
                Debug.LogWarning("적 소환 실패: 풀에서 적 오브젝트를 가져올 수 없음.");
                return;
            }
            if (enemyObj != null)
            {
                // 플레이어 위치 기준으로 가장자리 무작위 위치로 소환 위치 결정
                Vector3 spawnPos = GetSpawnPos();
                enemyObj.transform.position = spawnPos;

                // 적 타입에 맞춰서 부모 오브젝트 지정 (스폰된 적들용 부모)
                switch (enemyData.type)
                {
                    case EnemyData.EnemyType.Normal:
                        enemyObj.transform.SetParent(spawnedEnemyPar.transform);
                        break;
                    case EnemyData.EnemyType.Elite:
                        enemyObj.transform.SetParent(spawnedEliteEnemyPar.transform);
                        break;
                    case EnemyData.EnemyType.Boss:
                        enemyObj.transform.SetParent(spawnedBossEnemyPar.transform);
                        break;
                }

                // 적의 enemyfinal 컴포넌트에 데이터 설정 - EnemyData는 EnemyPoolManager의 DataArray의 waveIndex번째 데이터
                if (enemyObj.TryGetComponent<EnemyFinal>(out EnemyFinal enemyFinal))
                {
                    enemyFinal.EnemyData = enemyData;
                }
                else Debug.LogError("EnemyFinal component not found on spawned enemy object.");

                //적 활성화
                enemyObj.SetActive(true);
            }
        }

        Vector3 GetSpawnPos()
        {
            Camera mainCam = Camera.main;

            // 화면 경계
            Vector3 bottomLeft = playerPos.position - new Vector3(spawnDistance, 0, spawnDistance);
            Vector3 topRight = playerPos.position + new Vector3(spawnDistance, 0, spawnDistance);

            // 화면 경계 좌표
            float leftBound = bottomLeft.x;
            float rightBound = topRight.x;
            float topBound = topRight.z;
            float bottomBound = bottomLeft.z;
            
            // 4개 변 중 하나 랜덤 선택
            int side = Random.Range(0, 4);
            Vector3 spawnPos = Vector3.zero;
            
            switch (side)
            {
                case 0: // 아래쪽 변
                    spawnPos = new Vector3(Random.Range(leftBound, rightBound), playerPos.position.y, bottomBound);
                    break;
                case 1: // 위쪽 변
                    spawnPos = new Vector3(Random.Range(leftBound, rightBound), playerPos.position.y, topBound);
                    break;
                case 2: // 왼쪽 변
                    spawnPos = new Vector3(leftBound, playerPos.position.y, Random.Range(bottomBound, topBound));
                    break;
                case 3: // 오른쪽 변
                    spawnPos = new Vector3(rightBound, playerPos.position.y, Random.Range(bottomBound, topBound));
                    break;
            }
            
            return spawnPos;
        }

        /// <summary>
        /// 웨이브 교체
        /// </summary>
        void ChangeWave(int waveIndex)
        {
            // 현재 웨이브 인덱스 갱신
            currentWaveIndex = waveIndex;
            // EnemyData 배열/리스트에서 다음 웨이브 적 구성 선택 및 스폰
            //epManager.ReturnAllEnemies();
            //오브젝트 갱신은 모두 Return에서 개별적으로 수행
        }
        #endregion
    }
}