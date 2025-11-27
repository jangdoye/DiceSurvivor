using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DiceSurvivor.Manager;

namespace DiceSurvivor.Weapon
{
    /// <summary>
    /// Boomerang 무기 - 가장 가까운 적에게 투척 후 돌아오는 무기
    /// </summary>
    public class BoomerangWeapon : RangedWeaponBase
    {
        [Header("Boomerang Specific")]
        [SerializeField] private float rotationSpeed = 720f;           // 회전 속도 (도/초)

        [Header("Runtime")]
        private List<BoomerangProjectile> activeBoomerangs;            // 활성 부메랑 목록
        private float attackTimer = 0f;                                // 공격 타이머


        protected override void Awake()
        {

            base.Awake();
        }

        private void Start()
        {
            // 리스트 초기화
            activeBoomerangs = new List<BoomerangProjectile>();
        }

        protected override void Update()
        {
            // 쿨다운 체크
            attackTimer += Time.deltaTime;

            if (attackTimer >= Weapon.cooldown)
            {

                Attack();
                attackTimer = 0f;
            }

            // 비활성 부메랑 정리
            CleanupInactiveBoomerangs();
        }

        /// <summary>
        /// 무기 초기화
        /// </summary>
        //protected override void InitializeWeapon();
        //protected override void ShootProjectile(GameObject target)
        //{

        //}


        /// <summary>
        /// 공격 수행
        /// </summary>
        protected override void Attack()
        {
            // 가장 가까운 적 찾기
            GameObject closestEnemy = FindEnemy();

            if (closestEnemy == null)
            {
                //Debug.Log("[Boomerang] 타겟이 없습니다.");
                return;
            }

            // projectileCount 만큼 부메랑 발사
            for (int i = 0; i < Weapon.projectileCount - 1; i++)
            {
                LaunchBoomerang(closestEnemy, i * 0.1f); // 약간의 딜레이를 두고 발사
            }
            LaunchBoomerang(closestEnemy, Weapon.cooldown);
        }
        protected override void ShootProjectile(GameObject target, int projectileCount)
        {
            for (int i = 0; i < projectileCount; i++)
            {
                LaunchBoomerang(target, 0.5f);
            }
        }
        /// <summary>
        /// 부메랑 발사
        /// </summary>
        private void LaunchBoomerang(GameObject target, float delay)
        {
            StartCoroutine(LaunchBoomerangWithDelay(target, delay));
        }

        /// <summary>
        /// 딜레이 후 부메랑 발사
        /// </summary>
        private IEnumerator LaunchBoomerangWithDelay(GameObject target, float delay)
        {
            yield return new WaitForSeconds(delay);

            // 부메랑 생성
            GameObject boomerang = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            weaponController.PlaySfx();

            // BoomerangProjectile 컴포넌트 추가/설정
            BoomerangProjectile projectile = boomerang.GetComponent<BoomerangProjectile>();
            if (projectile == null)
            {
                projectile = boomerang.AddComponent<BoomerangProjectile>();
            }

            // 초기 방향 설정 (타겟이 있으면 타겟 방향, 없으면 정면)
            Vector3 initialDirection;
            if (target != null)
            {
                initialDirection = (target.transform.position - transform.position).normalized;
            }
            else
            {
                initialDirection = transform.forward;
            }

            // 부메랑 초기화
            projectile.Initialize(
            transform,              // 발사 위치 (플레이어)
                initialDirection,       // 발사 방향
                Weapon.damage,                 // 데미지
                Weapon.range,                  // 최대 거리
                Weapon.projectileSpeed,             // 이동 속도
                Weapon.projectileSize,         // 크기
                Weapon.isPiercing,             // 관통 여부
                rotationSpeed,          // 회전 속도
                this
            );

            activeBoomerangs.Add(projectile);

            //Debug.Log($"[Boomerang] 발사! 타겟: {(target != null ? target.name : "없음")}");
        }

        /// <summary>
        /// 비활성 부메랑 정리
        /// </summary>
        private void CleanupInactiveBoomerangs()
        {
            activeBoomerangs.RemoveAll(boomerang => boomerang == null || !boomerang.IsActive);
        }

        public override void LevelUp()
        {
            base.LevelUp();
            //Debug.Log($"[Boomerang] 레벨업! 현재 레벨: {Weapon.level}");
        }
    }
}

