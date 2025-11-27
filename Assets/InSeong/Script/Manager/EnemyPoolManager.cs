using UnityEngine;
using System.Collections.Generic;
using DiceSurvivor.Enemy;
using System;
using Unity.VisualScripting;

namespace DiceSurvivor.Manager {
    /// <summary>
    /// EnemyData 기반 적 풀링 매니저 (SingletonManager 상속, 불필요한 자료구조 제거)
    /// </summary>
    public class EnemyPoolManager : SingletonManager<EnemyPoolManager>
    {
        #region Variables
        [Header("일반/엘리트/보스 적 정보 배열")]
        public EnemyData[] enemyDataArray;
        public EnemyData[] eliteEnemyDataArray;
        public EnemyData[] bossEnemyDataArray;

        [Header("적 종류별 사용할 풀")]
        public Queue<GameObject> enemyPool = new Queue<GameObject>();
        public Queue<GameObject> eliteEnemyPool = new Queue<GameObject>();
        public Queue<GameObject> bossEnemyPool = new Queue<GameObject>();

        [Header("각 풀 별 최대 동시 존재할 오브젝트의 수")]
        public int maxEnemyCount = 100;
        public int maxEliteEnemyCount = 15;
        public int maxBossEnemyCount = 1;

        [Header("적 스폰 매니저 참조")]
        public EnemySpawnManager esManager;

        //풀에 담긴 오브젝트들을 보관할 하이어라키 상 오브젝트
        public GameObject enemyPar;
        public GameObject eliteEnemyPar;
        public GameObject bossEnemyPar;

        
        //초기 풀 대기
        public bool poolInitWaiting = true;
        //리셋 대기
        bool isResetting = false;
        #endregion

        #region Properties
        #endregion

        #region Unity Event Methods
        protected override void Awake()
        {
            base.Awake();
            if (enemyPar == null) enemyPar = new GameObject("EnemyPar");
            if (eliteEnemyPar == null) eliteEnemyPar = new GameObject("EliteEnemyPar");
            if (bossEnemyPar == null) bossEnemyPar = new GameObject("BossEnemyPar");
            //풀 구조 초기화
            InitializeEnemyPools();
        }

        void Update()
        {
            //누락 요소 점검
            CheckMissing();
        }
        #endregion

        #region Custom Methods
        //씬 로드 시 풀 초기화
        public void ResetPool()
        {
            isResetting = true;
            //기존 풀 비우기
            if (enemyPar != null)
            {
                for (int i = enemyPar.transform.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(enemyPar.transform.GetChild(i).gameObject);
                }
            }
            
            if (eliteEnemyPar != null)
            {
                for (int i = eliteEnemyPar.transform.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(eliteEnemyPar.transform.GetChild(i).gameObject);
                }
            }
            
            if (bossEnemyPar != null)
            {
                for (int i = bossEnemyPar.transform.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(bossEnemyPar.transform.GetChild(i).gameObject);
                }
            }

            while (enemyPool.Count > 0)
            {
                GameObject obj = enemyPool.Dequeue();
                DestroyImmediate(obj);
            }

            while (eliteEnemyPool.Count > 0)
            {
                GameObject obj = eliteEnemyPool.Dequeue();
                DestroyImmediate(obj);
            }

            while (bossEnemyPool.Count > 0)
            {
                GameObject obj = bossEnemyPool.Dequeue();
                DestroyImmediate(obj);
            }

            enemyPool = new Queue<GameObject>();
            eliteEnemyPool = new Queue<GameObject>();
            bossEnemyPool = new Queue<GameObject>();

            poolInitWaiting = true;
            isResetting = false;
            CheckMissing();
            InitializeEnemyPools();
        }
        //누락 요소 점검
        void CheckMissing()
        {
            if (isResetting) return;
            if (esManager == null) esManager = EnemySpawnManager.Instance;
            //하이어라키 오브젝트 생성 후 할당
            if (enemyPar == null) enemyPar = FindObject("EnemyPar");
            if (eliteEnemyPar == null) eliteEnemyPar = FindObject("EliteEnemyPar");
            if (bossEnemyPar == null) bossEnemyPar = FindObject("BossEnemyPar");

            if (enemyPool != null && eliteEnemyPool != null && bossEnemyPool != null)
            {
                if (enemyPar != null && eliteEnemyPar != null && bossEnemyPar != null)
                {
                    poolInitWaiting = false;
                }
            }
        }

        GameObject FindObject(string name)
        {
            GameObject newObj = GameObject.Find(name);
            if (newObj != null)
            {
                return newObj;
            }
            else return new GameObject(name);
        }
        /// <summary>
        /// EnemyData 기준 적 스폰(풀에서 꺼내어 위치/속성 적용)
        /// </summary>
        public GameObject SpawnEnemy(EnemyData data)
        {
            //Debug.Log("적 소환 요청: " + data.type);
            //인덱스 안 맞는 애들은 소환 전에 returnenemy를 실행
            // 해당 EnemyData의 풀에서 오브젝트 꺼내서 spawnmanager에 전달
            switch (data.type)
            {
                case EnemyData.EnemyType.Normal:
                    if (enemyPool.Count == 0) return null; // 풀에 오브젝트가 없으면 null 반환
                    return GetValidEnemy(enemyPool, data, enemyDataArray);
                case EnemyData.EnemyType.Elite:
                    if (eliteEnemyPool.Count == 0) return null; // 풀에 오브젝트가 없으면 null 반환
                    if (esManager.EliteSpawnCond())
                        return GetValidEnemy(eliteEnemyPool, data, eliteEnemyDataArray);
                    else return null;
                case EnemyData.EnemyType.Boss:
                    if (bossEnemyPool.Count == 0) return null; // 풀에 오브젝트가 없으면 null 반환
                    return GetValidEnemy(bossEnemyPool, data, bossEnemyDataArray);
                default:
                    Debug.LogError("Unknown enemy type: " + data.type);
                    return null;
            }

            return null; // 기본적으로 null 반환 (예외 처리)
        }

        //현재 인덱스에 유효한 객체를 반환하기 위한 메서드
        GameObject GetValidEnemy(Queue<GameObject> pool, EnemyData data, EnemyData[] dataArray) {
            //Debug.Log("적 유효성 검사 및 반환");
            int currIndex = esManager.currentWaveIndex;
            if(data.type == EnemyData.EnemyType.Elite)
                currIndex = esManager.currentWaveIndex / 3;
            else if (data.type == EnemyData.EnemyType.Boss)
                currIndex = 0; //보스는 항상 첫 번째 데이터 사용
            if (CheckValid(currIndex, dataArray, data))
            {
                //Debug.Log(pool.Count);
                GameObject result = pool.Dequeue();
                //Debug.Log("적 반환: " + result.name);
                return result;
            }
            else
            {
                //Debug.Log(pool.Count);
                //유효한 객체가 아니라면 풀 내 오브젝트들을 순회하며 불일치하는 객체를 제거하고 새로 채움
                while (pool.Count > 0)
                {
                    GameObject obj = pool.Dequeue();
                    if (CheckValid(currIndex, dataArray, data))
                    {
                        return obj;
                    }
                    else ReturnEnemy(data, obj);
                }
            }

            return null; // 유효한 오브젝트가 없으면 null 반환
        }

        /// <summary>
        /// 풀 오브젝트 초기화
        /// </summary>
        void InitializeEnemyPools()
        {
            //Debug.Log("풀 초기화 시작");
            int currentWaveIndex = esManager.currentWaveIndex;
            //각 풀에 정해진 최대 수량 만큼의 오브젝트를 미리 생성
            for (int i = 0; i < maxEnemyCount; i++)
            {
                //Debug.Log("일반 적 풀에 집어넣기");
                GameObject enemyObj = Instantiate(enemyDataArray[currentWaveIndex].body);
                //각 DataArray에 할당된 body는 EnemyFinal 컴포넌트를 갖고 있고, 여기에 EnemyData 할당
                if (enemyObj.TryGetComponent<EnemyFinal>(out EnemyFinal ea))
                {
                    ea.EnemyData = enemyDataArray[currentWaveIndex];
                }
                else
                {
                    Debug.LogError("EnemyFinal component not found on the enemy prefab.");
                }
                enemyObj.SetActive(false);
                enemyPool.Enqueue(enemyObj);
                enemyObj.transform.SetParent(enemyPar.transform);
            }
            for (int i = 0; i < maxEliteEnemyCount; i++)
            {
                GameObject eliteEnemyObj = Instantiate(eliteEnemyDataArray[0].body);
                if (eliteEnemyObj.TryGetComponent<EnemyFinal>(out EnemyFinal ea))
                {
                    ea.EnemyData = eliteEnemyDataArray[0];
                }
                else
                {
                    Debug.LogError("EnemyFinal component not found on the enemy prefab.");
                }
                eliteEnemyObj.SetActive(false);
                eliteEnemyPool.Enqueue(eliteEnemyObj);
                eliteEnemyObj.transform.SetParent(eliteEnemyPar.transform);
            }
            for (int i = 0; i < maxBossEnemyCount; i++)
            {
                GameObject bossEnemyObj = Instantiate(bossEnemyDataArray[0].body);
                if (bossEnemyObj.TryGetComponent<EnemyFinal>(out EnemyFinal ea))
                {
                    ea.EnemyData = bossEnemyDataArray[0];
                }
                else
                {
                    Debug.LogError("EnemyFinal component not found on the enemy prefab.");
                }
                bossEnemyObj.SetActive(false);
                bossEnemyPool.Enqueue(bossEnemyObj);
                bossEnemyObj.transform.SetParent(bossEnemyPar.transform);
            }
            poolInitWaiting = false; //풀 초기화 완료
        }


        /// <summary>
        /// EnemyData 기준 적 반환(비활성화) 및 현재 웨이브 인덱스에 맞게 갱신
        /// </summary>

        public void ReturnEnemy(EnemyData data, GameObject obj)
        {
            obj.SetActive(false);
            //검증을 위한 인덱스
            int index = esManager.currentWaveIndex;

            //현재 웨이브와 맞지 않는 오브젝트들은 파괴 후 재생성
            //현재 웨이브와 맞는 오브젝트들은 풀에 반환
            switch (data.type)
            {
                case EnemyData.EnemyType.Normal:
                    if (!CheckValid(index, enemyDataArray, data))
                    {
                        Destroy(obj);
                        //현재 웨이브에 맞는 오브젝트를 생성해서 큐에 집어넣기
                        if (index < enemyDataArray.Length)
                        {
                            GameObject newobj = Instantiate(enemyDataArray[index].body);
                            newobj.SetActive(false);
                            enemyPool.Enqueue(newobj);
                        }
                        return;
                    }
                    else enemyPool.Enqueue(obj);
                    break;
                case EnemyData.EnemyType.Elite:
                    if (!CheckValid(index / 3, eliteEnemyDataArray, data))
                    {
                        Destroy(obj);
                        //현재 웨이브에 맞는 오브젝트를 생성해서 큐에 집어넣기
                        if (index / 3 < eliteEnemyDataArray.Length)
                        {
                            GameObject newobj = Instantiate(eliteEnemyDataArray[index / 3].body);
                            newobj.SetActive(false);
                            eliteEnemyPool.Enqueue(newobj);
                        }
                        eliteEnemyPool.Enqueue(obj);
                        return;
                    }
                    else eliteEnemyPool.Enqueue(obj);
                    break;
                case EnemyData.EnemyType.Boss:
                    bossEnemyPool.Enqueue(obj);
                    break;
            }
        }

        bool CheckValid(int index, EnemyData[] dataArray, EnemyData data)
        {
            //조건 1 : 현재 웨이브 상태가 유효한 웨이브인가?
            bool cond1 = index >= 0 && index < dataArray.Length;
            //조건 2 : 반환해야 할 객체의 EnemyData가 현재 웨이브에 맞는 데이터인가?
            bool cond2 = data != null && data.enemyName == dataArray[index].enemyName;

            //Debug.Log("조건 검사 : " + cond1 + " , " + cond2);
            return cond1 && cond2;
        }
        #endregion
    }
}