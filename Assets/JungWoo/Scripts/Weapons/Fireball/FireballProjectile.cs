using DiceSurvivor.Enemy;
using DiceSurvivor.Weapon;

using UnityEngine;

namespace DiceSurvivor.Weapon
{
    /// <summary>
    /// 파이어볼 투사체 컴포넌트
    /// </summary>
    public class FireballProjectile : MonoBehaviour
    {
        private FireballWeapon fireballWeapon = null;
        private Vector3 moveDirection;         // 이동 방향
        private float directDamage;           // 직접 데미지
        private float explosionDamage;        // 폭발 데미지
        private float explosionRadius;        // 폭발 범위
        private float dotDamage;              // DoT 데미지
        private float dotDuration;            // DoT 지속시간
        private float maxRange;               // 최대 거리
        private float moveSpeed;              // 이동 속도
        private float projectileSize;         // 투사체 크기
        private GameObject explosionEffect;   // 폭발 이펙트

        private Vector3 startPosition;        // 시작 위치
        private float traveledDistance;       // 이동한 거리
        private bool hasExploded;             // 폭발 여부

        public bool IsActive { get; private set; }
        public FireballWeapon FB { get { return fireballWeapon; } set { fireballWeapon = value; } }

        /// <summary>
        /// 파이어볼 초기화
        /// </summary>
        public void Initialize(Vector3 direction, float damage, float explDamage, float explRadius,
                             float dot, float duration, float range, float speed, float size, GameObject effect, FireballWeapon fb)
        {
            moveDirection = direction.normalized;
            directDamage = damage;
            explosionDamage = explDamage;
            explosionRadius = explRadius;
            dotDamage = dot;
            dotDuration = duration;
            maxRange = range;
            projectileSize = size;
            explosionEffect = effect;
            moveSpeed = speed;      // projectileSpeed는 이동 시간(초)
            FB = fb;

            startPosition = transform.position;
            traveledDistance = 0f;
            hasExploded = false;
            IsActive = true;

            // 크기 설정
            transform.localScale = Vector3.one * projectileSize;



            //Debug.Log($"[FireballProjectile] 초기화 - 직접: {directDamage}, 폭발: {explosionDamage}, DoT: {dotDamage}");
        }

        void Update()
        {
            if (!IsActive || hasExploded) return;

            // 이동
            Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;
            transform.position += movement;

            // 이동 거리 계산
            traveledDistance += movement.magnitude;

            // 최대 거리 도달 시 폭발
            if (traveledDistance >= maxRange)
            {
                Explode(transform.position);
            }
        }

        /// <summary>
        /// 적과 충돌 시
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (!IsActive || hasExploded) return;

            // Enemy 레이어 체크
            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                // 직접 데미지
                var enemy = other.GetComponent<EnemyFinal>();
                if (enemy != null)
                {
                    fireballWeapon.ApplyDamage(enemy.gameObject, directDamage);
                    //Debug.Log($"[FireballProjectile] 직접 타격: {other.name} - 데미지: {directDamage}");
                }

                // 충돌 위치에서 폭발
                Explode(transform.position);
            }
        }

        /// <summary>
        /// 폭발 처리
        /// </summary>
        private void Explode(Vector3 position)
        {
            if (hasExploded) return;
            hasExploded = true;

            //Debug.Log($"[FireballProjectile] 폭발! 위치: {position}, 범위: {explosionRadius}");

            // 폭발 범위 내 적에게 데미지
            int enemyLayer = LayerMask.GetMask("Enemy");
            Collider[] enemies = Physics.OverlapSphere(position, explosionRadius, enemyLayer);

            foreach (var enemyCollider in enemies)
            {
                var enemy = enemyCollider.GetComponent<EnemyFinal>();
                if (enemy != null)
                {
                    // 폭발 데미지
                    fireballWeapon.ApplyDamage(enemy.gameObject,explosionDamage);
                    //Debug.Log($"[FireballProjectile] 폭발 데미지: {enemyCollider.name} - {explosionDamage}");
                }
            }

            // DoT 구역 생성 (레벨 3부터)
            if (dotDamage > 0 && dotDuration > 0)
            {
                CreateDotZone(position);
            }

            // 폭발 이펙트
            //CreateExplosionEffect(position);

            // 투사체 제거
            IsActive = false;
            Destroy(gameObject);
        }

        /// <summary>
        /// DoT 구역 생성
        /// </summary>
        private void CreateDotZone(Vector3 position)
        {
            GameObject dotZone = new GameObject($"FireballDotZone");
            dotZone.transform.position = position;

            FireDotZone zone = dotZone.AddComponent<FireDotZone>();
            zone.Initialize(explosionRadius, dotDamage, dotDuration);

            if (explosionEffect != null)
            {
                GameObject effect = Instantiate(explosionEffect, position, Quaternion.identity);
                Destroy(effect, dotDuration);
            }

            //Debug.Log($"[FireballProjectile] DoT 구역 생성 - 데미지: {dotDamage}/초, 지속: {dotDuration}초");
        }

        /// <summary>
        /// 폭발 이펙트 생성
        /// </summary>
        private void CreateExplosionEffect(Vector3 position)
        {
            
        }
    }
}