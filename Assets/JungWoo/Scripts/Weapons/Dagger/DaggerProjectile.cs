using DiceSurvivor.Enemy;
using DiceSurvivor.Weapon;
using UnityEngine;

namespace DiceSurvivor.Weapon
{
    /// <summary>
    /// 차크람 투사체 컴포넌트
    /// </summary>
    public class DaggerProjectile : MonoBehaviour
    {
        private DaggerWeapon daggerWeapon = null;
        private Vector3 moveDirection;         // 이동 방향
        private float damage;                  // 데미지
        private float maxRange;                // 최대 거리
        private float moveSpeed;               // 이동 속도
        private float projectileSize;          // 크기

        private Vector3 startPosition;         // 시작 위치
        private float traveledDistance;        // 이동한 거리
                                               //private bool hasHitEnemy;              // 적 타격 여부

        public bool IsActive { get; private set; }
        public DaggerWeapon DW { get { return daggerWeapon; } set { daggerWeapon = value; } }

        /// <summary>
        /// 차크람 초기화
        /// </summary>
        public void Initialize(Vector3 direction, float dmg, float range, float speed, float size, DaggerWeapon dw)
        {           
            moveDirection = direction.normalized;
            damage = dmg;
            maxRange = range;
            moveSpeed = speed;
            projectileSize = size;
            DW = dw;

            startPosition = transform.position;
            traveledDistance = 0f;
            //hasHitEnemy = false;
            IsActive = true;
            //Debug.Log($"IsActive 1:{IsActive }");

            // 크기 설정
            transform.localScale = Vector3.one * projectileSize;

            // 투사체가 바라보는 방향 설정
            transform.LookAt(transform.position + moveDirection);

            //Debug.Log($"[DaggerProjectile] 초기화 - 데미지: {damage}, 범위: {maxRange}, 속도: {moveSpeed}");
        }

        void Update()
        {
            if (!IsActive) return;

            // 이동
            Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;
            transform.position += movement;

            // 이동 거리 계산
            traveledDistance += movement.magnitude;

            // 최대 거리 도달 시 제거
            if (traveledDistance >= maxRange)
            {
                Deactivate();
            }
        }

        /// <summary>
        /// 적과 충돌 시
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            //Debug.Log($"IsActive 2:{IsActive}");

            //Debug.Log($"TriggerEnter 1:{other.tag}");

            if (IsActive == false) return;

            //Debug.Log($"TriggerEnter 2:{other.tag}");

            // Enemy 레이어 체크
            if (other.gameObject.tag == "Enemy")
            {
                //Debug.Log("12312312312312");
                // 즉시 데미지
                var enemy = other.GetComponent<EnemyFinal>();
                if (enemy != null)
                {
                    daggerWeapon.ApplyDamage(enemy.gameObject, damage);
                    //hasHitEnemy = true;

                    //Debug.Log($"[DaggerProjectile] {other.name}에게 데미지 {damage} 적용");

                    // 타격 이펙트
                    //CreateHitEffect(other.transform.position);

                    // 적과 부딪히면 즉시 사라짐
                    Deactivate();
                }
            }
        }

        /// <summary>
        /// 차크람 비활성화
        /// </summary>
        private void Deactivate()
        {

            Destroy(gameObject);
        }
    }
}
