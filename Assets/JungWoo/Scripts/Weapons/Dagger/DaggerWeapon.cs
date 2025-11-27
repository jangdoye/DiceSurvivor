using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DiceSurvivor.Manager;
using DiceSurvivor.Enemy;

namespace DiceSurvivor.Weapon
{
    /// <summary>
    /// Dagger 무기 - 연속 발사형
    /// </summary>
    public class DaggerWeapon : RangedWeaponBase
    {
        [Header("Dagger Specific")]
        //[SerializeField] private GameObject DaggerPrefab;             // 차크람 프리팹
        [SerializeField] private float spawnHeightOffset = 0.5f;       // 투사체 발사 높이

        [Header("Runtime")]
        [Header("Runtime")]
        private List<DaggerProjectile> activeDaggers;                // 활성 차크람 목록
        private float attackTimer = 0.2f;                                // 공격 타이머
        

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            activeDaggers = new List<DaggerProjectile>();
        }

        protected override void Update()
        {
            // 쿨다운 체크
            attackTimer += Time.deltaTime;

            if (attackTimer >= 0.5)
            {
                //Debug.Log($"cooldown:{cooldown}");
                Attack();
                attackTimer = 0f;
            }

            // 비활성 차크람 정리
            CleanupInactiveDaggers();
        }

        public override void ApplyDamage(GameObject enemy, float damageAmount)
        {
            base.ApplyDamage(enemy, damageAmount);
        }

        protected override void ShootProjectile(GameObject target, int projectileCount)
        {
            Vector3 targetDirection;
            if (target != null)
            {
                targetDirection = (target.transform.position - transform.position).normalized;
            }
            else
            {
                targetDirection = transform.forward;
            }

            // 병렬 발사를 위한 오프셋 계산
            Vector3 perpDirection = Vector3.Cross(targetDirection, Vector3.up).normalized;
            float totalOffset = (projectileCount - 1) * 0.2f;
            Vector3 startOffset = -perpDirection * (totalOffset / 2f);

            for (int i = 0; i < projectileCount; i++)
            {
                Vector3 currentOffset = startOffset + perpDirection * (i * 0.2f);
                LaunchDagger(targetDirection, currentOffset);
            }
        }

        /// <summary>
        /// 차크람 발사
        /// </summary>
        private void LaunchDagger(Vector3 direction, Vector3 offset)
        {
            // 차크람 생성
            GameObject Dagger = Instantiate(projectilePrefab, transform.position + offset + Vector3.up * 0.5f * spawnHeightOffset, Quaternion.identity);
            weaponController.PlaySfx();

            // DaggerProjectile 컴포넌트 추가/설정
            DaggerProjectile projectile = Dagger.GetComponent<DaggerProjectile>();
            if (projectile == null)
            {
                projectile = Dagger.AddComponent<DaggerProjectile>();
            }

            //if(projectile.DW == null) projectile.DW = this;

            // 차크람 초기화
            projectile.Initialize(
                direction,              // 발사 방향
                Weapon.damage,                 // 데미지
                Weapon.range,                  // 최대 거리
                Weapon.projectileSpeed,        // 이동 속도
                Weapon.projectileSize,         // 크기
                this
            );

            activeDaggers.Add(projectile);

            //Debug.Log($"[Dagger] 발사! 방향: {direction}");
        }

        /// <summary>
        /// 비활성 차크람 정리
        /// </summary>
        private void CleanupInactiveDaggers()
        {
            activeDaggers.RemoveAll(Dagger => Dagger == null || !Dagger.IsActive);
        }
    }

    
}