using DiceSurvivor.Enemy;    
using System.Collections.Generic;
using UnityEngine;

namespace DiceSurvivor.Weapon
{
    /// <summary>
    /// 부메랑 투사체 컴포넌트
    /// </summary>
    public class BoomerangProjectile : MonoBehaviour
    {
        private BoomerangWeapon boomerangWeapon = null;
        private Transform owner;               // 발사한 플레이어
        private Vector3 initialDirection;      // 초기 방향
        private float damage;                  // 데미지
        private float maxRange;                // 최대 거리
        private float moveSpeed;               // 이동 속도
        private float projectileSize;          // 크기
        private bool isPiercing;               // 관통 여부
        private float rotationSpeed;           // 회전 속도

        private Vector3 startPosition;         // 시작 위치
        private float traveledDistance;        // 이동한 거리
        private bool isReturning;              // 돌아오는 중인지
        private HashSet<GameObject> hitEnemiesGoing;    // 갈 때 타격한 적
        private HashSet<GameObject> hitEnemiesReturning; // 올 때 타격한 적

        public bool IsActive { get; private set; }

        public BoomerangWeapon BW { get { return boomerangWeapon; } set { boomerangWeapon = value; } }

        private float groundY = 0f; // Prevent the projectile from going below this y-value. Adjust as needed.

        /// <summary>
        /// 부메랑 초기화
        /// </summary>
        public void Initialize(Transform launcher, Vector3 direction, float dmg, float range,
                             float speed, float size, bool piercing, float rotSpeed, BoomerangWeapon bw)
        {
            owner = launcher;
            initialDirection = direction.normalized;
            damage = dmg;
            maxRange = range;
            projectileSize = size;
            isPiercing = piercing;
            rotationSpeed = rotSpeed;
            moveSpeed = speed;
            BW = bw;

            startPosition = transform.position;
            traveledDistance = 0f;
            isReturning = false;
            IsActive = true;

            hitEnemiesGoing = new HashSet<GameObject>();
            hitEnemiesReturning = new HashSet<GameObject>();

            // 크기 설정
            transform.localScale = Vector3.one * projectileSize;

            //Debug.Log($"[BoomerangProjectile] 초기화 - 데미지: {damage}, 범위: {maxRange}, 속도: {moveSpeed}");
        }

        void Update()
        {
            if (!IsActive) return;

            // 소유자가 없으면 제거
            if (owner == null)
            {
                Deactivate();
                return;
            }

            // 회전 애니메이션
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

            // 이동 처리
            if (!isReturning)
            {
                // 전진 (관통 여부와 상관없이 계속 전진)
                MoveForward();
            }
            else
            {
                // 돌아오기 (플레이어의 현재 위치로)
                ReturnToOwner();
            }

            // Keep the boomerang from going below the ground.
            Vector3 currentPosition = transform.position;
            if (currentPosition.y < groundY)
            {
                transform.position = new Vector3(currentPosition.x, groundY, currentPosition.z);
            }
        }

        /// <summary>
        /// 전진 이동
        /// </summary>
        private void MoveForward()
        {
            // 이동
            Vector3 movement = initialDirection * moveSpeed  * Time.deltaTime;
            transform.position += movement;

            // 이동 거리 계산
            traveledDistance += movement.magnitude;

            // 최대 거리 도달 시 돌아오기 시작
            if (traveledDistance >= maxRange)
            {
                isReturning = true;
                //Debug.Log($"[BoomerangProjectile] 최대 거리 도달 - 돌아오기 시작");
            }
        }

        /// <summary>
        /// 소유자에게 돌아오기 (플레이어의 현재 위치로)
        /// </summary>
        private void ReturnToOwner()
        {
            if (owner == null)
            {
                Deactivate();
                return;
            }

            // 플레이어의 현재 위치로 이동
            Vector3 directionToOwner = (owner.position - transform.position).normalized;
            Vector3 movement = directionToOwner * moveSpeed * 2f * Time.deltaTime;
            transform.position += movement;

            // 소유자에게 도달했는지 체크
            if (Vector3.Distance(transform.position, owner.position) < 1f)
            {
                //Debug.Log($"[BoomerangProjectile] 소유자에게 도착 - 제거");
                Deactivate();
            }
        }

        /// <summary>
        /// 적과 충돌 시
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (!IsActive) return;

            // Enemy 레이어 체크
            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                // 갈 때와 올 때 각각 한 번씩만 데미지
                HashSet<GameObject> currentHitList = isReturning ? hitEnemiesReturning : hitEnemiesGoing;

                if (!currentHitList.Contains(other.gameObject))
                {
                    // 데미지 적용
                    var enemy = other.GetComponent<EnemyFinal>();
                    if (enemy != null)
                    {
                        boomerangWeapon.ApplyDamage(enemy.gameObject, damage);
                        currentHitList.Add(other.gameObject);

                        //Debug.Log($"[BoomerangProjectile] {other.name}에게 데미지 {damage} 적용 (돌아오는 중: {isReturning})");
                    }
                }
            }
        }

        /// <summary>
        /// 부메랑 비활성화
        /// </summary>
        private void Deactivate()
        {
            IsActive = false;
            Destroy(gameObject);
        }

        void OnDrawGizmosSelected()
        {
            // 최대 범위 표시
            if (startPosition != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(startPosition, maxRange);
            }
        }
    }
}
