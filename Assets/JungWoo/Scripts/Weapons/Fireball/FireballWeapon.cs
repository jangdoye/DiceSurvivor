using System.Collections.Generic;
using UnityEngine;

namespace DiceSurvivor.Weapon
{
    /// <summary>
    /// Fireball 무기 - 무작위 방향으로 폭발 투사체 발사
    /// </summary>
    public class FireballWeapon : RangedWeaponBase
    {
        [Header("Fireball Specific")]
        [SerializeField] private GameObject explosionEffectPrefab;     // 폭발 이펙트

        [Header("Runtime")]
        private List<FireballProjectile> activeFireballs;              // 활성 파이어볼 목록
        //private float attackTimer = 0f;                                // 공격 타이머

        protected override void Awake()
        {
            base.Awake();
        }
        private void Start()
        {
            activeFireballs = new List<FireballProjectile>();
        }
        protected override void Update()
        {
            base.Update();

            // 비활성 파이어볼 정리
            CleanupInactiveFireballs();
        }
        /// <summary>
        /// 공격 수행
        /// </summary>
        protected override void ShootProjectile(GameObject target, int projectileCount)
        {
            for (int i = 0; i < projectileCount; i++)
            {
                LaunchFireball();
            }
        }
        /// <summary>
        /// 파이어볼 발사
        /// </summary>
        private void LaunchFireball()
        {

            // 무작위 방향 생성
            float randomAngle = Random.Range(0f, 360f);
            Vector3 randomDirection = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;

            // 파이어볼 생성
            GameObject fireball = Instantiate(projectilePrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            weaponController.PlaySfx();

            // FireballProjectile 컴포넌트 추가/설정
            FireballProjectile projectile = fireball.GetComponent<FireballProjectile>();
            if (projectile == null)
            {
                projectile = fireball.AddComponent<FireballProjectile>();
            }

            // 파이어볼 초기화
            projectile.Initialize(
                randomDirection,        // 발사 방향
                Weapon.damage,                 // 직접 데미지
                Weapon.explosionDamage,        // 폭발 데미지
                Weapon.explosionRadius,        // 폭발 범위
                Weapon.dotDamage,              // DoT 데미지 (레벨 3부터)
                Weapon.duration,               // DoT 지속시간
                Weapon.range,                  // 최대 거리
                Weapon.projectileSpeed,        // 이동 속도
                Weapon.projectileSize,         // 투사체 크기
                explosionEffectPrefab,   // 폭발 이펙트
                this
            );
            activeFireballs.Add(projectile);

            //Debug.Log($"[Fireball] 발사! 방향: {randomDirection}");
        }
        /// <summary>
        /// 비활성 파이어볼 정리
        /// </summary>
        private void CleanupInactiveFireballs()
        {
            activeFireballs.RemoveAll(fireball => fireball == null || !fireball.IsActive);
        }
    }
}