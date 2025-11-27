using DiceSurvivor.Manager;
using UnityEngine;

namespace DiceSurvivor.Enemy {
    public class EnemyActivity : MonoBehaviour
    {
        #region Variables
        //여깄는 건 기본값 - 모든 값 변경은 복사본에만 적용해야 함
        [SerializeField] protected EnemyData enemyData;
        protected EnemyData copiedData;
        public Transform playerPos;
        //엘리트 길막 방지
        protected int blockedCounter = 0;
        protected float blockRadius = 0.25f;
        //화면 이탈 시 재진입을 위한 경계선 offset
        [SerializeField] protected float border = 2f;
        //화면 이탈 시간 체크
        protected float outTimeCheck = 0f;
        [SerializeField] protected float outTimeThreshold = 2f;

        //적 뭉치기 방지에 사용될 거리 임계점
        [SerializeField] protected float clusterThreshold = 2f;
        #endregion

        #region Properties
        public EnemyData EnemyData
        {
            get { return copiedData; }
            set { copiedData = value; }
        }
        #endregion

        #region Unity Event Methods
        protected virtual void Start()
        {
            copiedData = enemyData.GetCopy();
            border = EnemySpawnManager.Instance.spawnDistance;
            // 플레이어의 위치를 찾기
            playerPos = EnemySpawnManager.Instance.playerPos;
            if (playerPos == null)
            {
                Debug.LogError("Player position not found. Please ensure the player is tagged correctly.");
            }

            //적의 타입에 따라서 다른 이동 방식을 설정
        }
        protected virtual void Update()
        {
            if (playerPos == null) return;
            if (copiedData.type == EnemyData.EnemyType.Normal || copiedData.type == EnemyData.EnemyType.Summoned)
            {
                if (Repulse()) return;
                NormalEnemyMove();
            }
            else
            {
                //엘리트 타입은 충돌감지 추가
                CheckCollision();
                EliteEnemyMove();
            }
            //보스 타입은 전용 스크립트를 사용 (패턴의 존재)
        }

        #endregion

        #region Custom Methods
        void NormalEnemyMove()
        {
            if (checkBorderOut())
            {
                outTimeCheck += Time.deltaTime;
                //화면을 벗어난 시간이 2초 이상이라면 풀에 다시 반환
                if (outTimeCheck >= outTimeThreshold)
                {
                    EnemyPoolManager.Instance.ReturnEnemy(enemyData, this.gameObject);
                }
            }
            else outTimeCheck = 0;
            Vector3 dir = (playerPos.position - transform.position).normalized;
            dir = new Vector3(dir.x, 0, dir.z);
            transform.position += dir * Time.deltaTime * enemyData.speed;

            OnOverlapped();
        }
        void EliteEnemyMove()
        {
            Vector3 dir = (playerPos.position - transform.position).normalized;
            dir = new Vector3(dir.x, 0, dir.z);
            if (checkBorderOut())
            {
                //경계 이탈 시 재진입
                ResetPosition();
            }
            transform.position += dir * Time.deltaTime * enemyData.speed;
            OnOverlapped();
            //TODO : 일정 시간 이상 (counter가 일정 횟수 이상) 막혀있다면 강제 재진입
            if (blockedCounter > 100)
            {
                ResetPosition();
            }
        }
        //화면 경계선을 벗어났는 지 판단하는 함수 - true면 화면 경계선을 이탈했음
        protected virtual bool checkBorderOut()
        {
            Vector3 minWorld = playerPos.position - new Vector3(border, 0, border);
            Vector3 maxWorld = playerPos.position + new Vector3(border, 0, border);

            Vector3 pos = transform.position;
            bool xOutBorder = pos.x < minWorld.x - border || pos.x > maxWorld.x + border;
            bool zOutBorder = pos.z < minWorld.z - border || pos.z > maxWorld.z + border;
            return xOutBorder || zOutBorder;
        }

        //경계를 벗어난 위치를 재조정
        protected virtual void ResetPosition()
        {
            Vector3 minWorld = playerPos.position - new Vector3(border, 0, border);
            Vector3 maxWorld = playerPos.position + new Vector3(border, 0, border);
            Vector3 pos = transform.position;

            if (pos.x < minWorld.x)
                pos.x = maxWorld.x - border * 0.9f;
            else if (pos.x > maxWorld.x)
                pos.x = minWorld.x + border * 0.9f;
            if (pos.z < minWorld.z)
                pos.z = maxWorld.z - border * 0.9f;
            else if (pos.z > maxWorld.z)
                pos.z = minWorld.z + border * 0.9f;
            transform.position = pos;
        }

        protected virtual void CheckCollision()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, blockRadius);

            foreach (var hit in hits)
            {
                // Doodad 태그 오브젝트와만 충돌 판정
                if (hit.CompareTag("Doodad"))
                {
                    blockedCounter++;
                    return;
                }
            }
        }
        //겹쳐 있는 적 해제하기
        void OnOverlapped()
        {
            Collider[] overlaps = Physics.OverlapSphere(transform.position, blockRadius);
            foreach (var other in overlaps)
            {
                if (other.transform != transform && other.CompareTag("Enemy"))
                {
                    Vector3 pushDir = (transform.position - other.transform.position).normalized;
                    pushDir = new Vector3(pushDir.x, 0, pushDir.z);
                    transform.position += pushDir * Time.deltaTime * enemyData.speed;
                }
                //지형지물에 막히게 하기
                else if (other.transform != transform && other.CompareTag("Doodad"))
                {
                    Vector3 dir = (transform.position - other.transform.position).normalized;
                    dir = new Vector3(dir.x, 0, dir.z);
                    transform.position += dir * Time.deltaTime * enemyData.speed;
                }
            }
        }

        //주기적인 적 분산
        bool Repulse()
        {
            //플레이어와의 거리가 너무 멀어지면 현재 상황에 상관없이 false 반환
            if (Vector3.Distance(transform.position, playerPos.position) > clusterThreshold) return false;
            Collider[] nearby = Physics.OverlapSphere(transform.position, 0.75f);
            int enemyCount = 0;

            foreach (var collider in nearby)
            {
                if (collider.CompareTag("Enemy") && collider.transform != transform)
                {
                    enemyCount++;
                }
            }

            // 3마리 이상 몰려있으면 다음 프레임은 멈춰있기
            if (enemyCount >= 3) return true;
            else return false;
        }

        //TODO : 무기 피격 시 체력 손실 및 넉백 처리
        public void TakeDamage(float damage)
        {
            
        }

        public void Die()
        {
            // 적 사망 처리 : 비활성화 + 풀로 돌아가기
        }

        void GetKnockBack(float force)
        {
            //플레이어에게 가는 반대 방향으로 밀리기
        }
        #endregion
    }

}
