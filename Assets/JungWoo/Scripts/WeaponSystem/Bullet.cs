using DiceSurvivor.Battle;
using UnityEngine;

namespace DiceSurvivor.WeaponDataSystem
{
    public class Bullet : MonoBehaviour
    {
        [Header("이동")] // Inspector에서 헤더로 표시
        public float speed = 10f;               // 총알 이동 속도

        private Vector3 direction;              // 이동 방향
        private float damage;                   // 데미지 값
        private float maxRange;                 // 최대 사거리
        private float penetration;              // 관통력 (몇 개의 적을 관통할 수 있는지)
        private float knockback;                // 넉백 힘
        private float critChance;               // 크리티컬 확률
        private float critMultiplier;           // 크리티컬 데미지 배수
        private bool hasAreaDamage;             // 범위 데미지 여부
        private float areaDamageRadius;         // 범위 데미지 반경

        private Vector3 startPosition;          // 발사 시작 위치
        private int penetratedCount;            // 투사체 갯수

        public void Initialize(Vector3 dir, float dmg, float range, float pen, float knock,
                              float crit, float critMult, bool areaDmg, float areaRadius)
        {
            direction = dir.normalized;         // 방향 벡터 정규화
            damage = dmg;                       // 데미지 저장
            maxRange = range;                   // 최대 사거리 저장
            penetration = pen;                  // 관통력 저장
            knockback = knock;                  // 넉백 힘 저장
            critChance = crit;                  // 크리티컬 확률 저장
            critMultiplier = critMult;          // 크리티컬 배수 저장
            hasAreaDamage = areaDmg;            // 범위 데미지 여부 저장
            areaDamageRadius = areaRadius;      // 범위 데미지 반경 저장

            startPosition = transform.position; // 시작 위치 저장

            // 회전 설정
            if (direction != Vector3.zero) // 방향이 0이 아니면
                transform.rotation = Quaternion.LookRotation(direction); // 이동 방향을 바라보도록 회전
        }

        void Update()
        {
            // 이동
            transform.Translate(Vector3.forward * speed * Time.deltaTime);

            // 사거리 체크
            if (Vector3.Distance(startPosition, transform.position) >= maxRange)
            {
                Destroy(gameObject);
            }
        }

        void OnTriggerEnter(Collider other) // 트리거 충돌이 시작될 때 호출
        {
            if (other.CompareTag("Enemy")) // 적과 충돌하면
            {
                // 크리티컬 계산
                bool isCritical = Random.Range(0f, 1f) < critChance; // 랜덤 값이 크리티컬 확률보다 작으면 크리티컬
                float finalDamage = isCritical ? damage * critMultiplier : damage; // 크리티컬이면 배수 적용

                // 데미지 처리
                BattleBase damageable = other.GetComponent<BattleBase>(); // IDamageable 인터페이스 가져오기
                if (damageable != null) // 인터페이스가 존재하면
                {
                    damageable.TakeDamage(finalDamage); // 데미지 적용

                    // 넉백 처리
                    Rigidbody enemyRb = other.GetComponent<Rigidbody>(); // 적의 Rigidbody 가져오기
                    if (enemyRb != null) // Rigidbody가 있으면
                    {
                        enemyRb.AddForce(direction * knockback, ForceMode.Impulse); // 넉백 힘 적용
                    }
                }

                // 범위 데미지
                if (hasAreaDamage) // 범위 데미지가 있으면
                {
                    DealAreaDamage(transform.position, finalDamage * 0.7f); // 범위 데미지 처리 (70% 데미지)
                }

                // 관통 처리
                penetratedCount++; // 관통한 적 수 증가
                if (penetratedCount >= penetration) // 관통 한계에 도달하면
                {
                    Destroy(gameObject); // 총알 파괴
                }
            }
            /*else if (other.CompareTag("Wall") || other.CompareTag("Obstacle")) // 벽이나 장애물과 충돌하면
            {
                Destroy(gameObject); // 총알 파괴
            }*/
        }

        void DealAreaDamage(Vector3 center, float areaDamage) // 범위 데미지를 처리하는 함수
        {
            Collider[] enemies = Physics.OverlapSphere(center, areaDamageRadius); // 범위 내의 모든 콜라이더 가져오기

            foreach (Collider enemy in enemies) // 모든 콜라이더에 대해 반복
            {
                if (enemy.CompareTag("Enemy")) // 적이면
                {
                    BattleBase damageable = enemy.GetComponent<BattleBase>(); // IDamageable 인터페이스 가져오기
                    if (damageable != null) // 인터페이스가 존재하면
                    {
                        damageable.TakeDamage(areaDamage); // 범위 데미지 적용
                    }
                }
            }

            // 시각적 효과 (선택사항)
            // 여기에 폭발 이펙트나 파티클 시스템을 추가할 수 있습니다.
        }
    }

    /*// IDamageable 인터페이스 (적 스크립트에서 구현해야 함)
    public interface IDamageable
    {
        void TakeDamage(float damage); // 데미지를 받는 함수
    }*/
}


