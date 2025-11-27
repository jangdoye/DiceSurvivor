using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DiceSurvivor.Manager;
using DiceSurvivor.Enemy;

namespace DiceSurvivor.Weapon
{
    /// <summary>
    /// Laser 무기 - 순간 발사 레이저 (0.1초 플래시)
    /// </summary>
    public class LaserWeapon : RangedWeaponBase
    {
        #region Variables
        [Header("Laser Specific")]
        [SerializeField] private GameObject hitEffectPrefab;          // 타격 이펙트 프리팹
        [SerializeField] private float burstDelay = 0.2f;             // 연발 간격

        [Header("Laser Width Settings")]
        [SerializeField] private float baseWidth = 0.5f;              // 기본 레이저 두께
        [SerializeField] private float widthPerLevel = 0.2f;          // 레벨당 두께 증가량
        [SerializeField] private float maxWidth = 2.0f;               // 최대 레이저 두께

        [Header("Runtime")]
        private List<LaserBeam> activeLasers;                         // 활성 레이저 목록
        private Coroutine burstFireCoroutine;                         // 연발 코루틴
        private bool isFiring = false;                                // 발사 중 플래그
        private float currentLaserWidth;                              // 현재 레이저 두께
        #endregion

        #region Unity Event Method
        protected override void Awake()
        {
            base.Awake();

            activeLasers = new List<LaserBeam>();
            CalculateLaserWidth();
        }

        protected override void Update()
        {
            // RangedWeaponBase의 Update를 사용하지 않고 자체 로직 사용
            if (firePoint == null) return;

            // 쿨다운 관리
            if (cooldown > 0)
                cooldown -= Time.deltaTime;

            // cooldown이 지나고 발사 중이 아닐 때만 새로운 공격 시작
            if (cooldown <= 0 && !isFiring)
            {
                Attack();
                cooldown = Weapon.cooldown;
            }

            CleanupInactiveLasers();
        }

        private void OnDestroy()
        {
            // 코루틴 정리
            if (burstFireCoroutine != null)
            {
                StopCoroutine(burstFireCoroutine);
            }

            // 활성 레이저 정리
            foreach (var laser in activeLasers)
            {
                if (laser != null && laser.gameObject != null)
                {
                    Destroy(laser.gameObject);
                }
            }
            activeLasers.Clear();
        }
        #endregion

        #region Custom Method
        /// <summary>
        /// RangedWeaponBase의 추상 메서드 구현 (사용하지 않음)
        /// </summary>
        protected override void ShootProjectile(GameObject target, int projectileCount)
        {
            // LaserWeapon은 이 메서드를 사용하지 않고 자체 발사 로직 사용
            // BurstFire 코루틴에서 처리
        }

        /// <summary>
        /// 레이저 두께 계산
        /// </summary>
        private void CalculateLaserWidth()
        {
            currentLaserWidth = baseWidth + (widthPerLevel * (Weapon.level - 1));
            currentLaserWidth = Mathf.Min(currentLaserWidth, maxWidth);
        }

        /// <summary>
        /// Attack 메서드 오버라이드 - 가장 먼 적을 향해 연발 발사
        /// </summary>
        protected override void Attack()
        {
            // 가장 먼 적 찾기
            GameObject farthestEnemy = FindFarthestEnemy(Weapon.range);

            // 이전 연발이 진행 중이면 중단
            if (burstFireCoroutine != null)
            {
                StopCoroutine(burstFireCoroutine);
                burstFireCoroutine = null;
            }

            // 연발 발사 시작
            if (farthestEnemy != null)
            {
                burstFireCoroutine = StartCoroutine(BurstFire(farthestEnemy));
            }
            else
            {
                return;
            }
            
        }

        /// <summary>
        /// 연발 발사 코루틴 - 같은 방향으로 연속 발사
        /// </summary>
        private IEnumerator BurstFire(GameObject targetEnemy)
        {
            isFiring = true;

            // 발사 방향 결정 (이번 공격 턴 동안 고정)
            Vector3 fireDirection;
            if (targetEnemy != null)
            {
                fireDirection = (targetEnemy.transform.position - firePoint.position).normalized;
                //Debug.Log($"[Laser] 타겟 발견: {targetEnemy.name}");
            }
            else
            {
                // 적이 없으면 플레이어 전방
                fireDirection = firePoint.forward;
                //Debug.Log("[Laser] 타겟 없음 - 전방 발사");
            }

            // projectileCount만큼 연속 발사 (같은 방향)
            for (int i = 0; i < Weapon.projectileCount; i++)
            {
                LaunchLaserBeam(fireDirection);

                // 마지막 발사가 아니면 짧은 딜레이
                if (i < Weapon.projectileCount - 1)
                {
                    yield return new WaitForSeconds(burstDelay);
                }
            }

            isFiring = false;
            burstFireCoroutine = null;
        }

        /// <summary>
        /// 레이저 발사 함수 - 순간 발사
        /// </summary>
        private void LaunchLaserBeam(Vector3 direction)
        {
            // 레이저 빔 생성 (플레이어 위치에 생성)
            GameObject laser = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity); 
            weaponController.PlaySfx();

            LaserBeam laserBeam = laser.GetComponent<LaserBeam>();
            if (laserBeam == null)
            {
                laserBeam = laser.AddComponent<LaserBeam>();
            }

            // 레이저 초기화 (0.1초 플래시)
            laserBeam.Initialize(firePoint, direction, Weapon.damage, Weapon.range,
                hitEffectPrefab, currentLaserWidth, this);

            activeLasers.Add(laserBeam);
        }

        /// <summary>
        /// 범위 내에서 가장 먼 적을 찾습니다.
        /// </summary>
        private GameObject FindFarthestEnemy(float searchRange)
        {
            GameObject farthest = null;
            float maxDistance = 0f;
            int enemyLayer = LayerMask.GetMask("Enemy");
            Collider[] enemies = Physics.OverlapSphere(transform.position, searchRange, enemyLayer);

            foreach (var enemy in enemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance > maxDistance && distance <= searchRange)
                {
                    maxDistance = distance;
                    farthest = enemy.gameObject;
                }
            }

            return farthest;
        }

        /// <summary>
        /// 비활성 레이저 정리
        /// </summary>
        private void CleanupInactiveLasers()
        {
            activeLasers.RemoveAll(laser => laser == null || !laser.IsActive);
        }

        /// <summary>
        /// 레벨업 시 레이저 두께 재계산
        /// </summary>
        public override void LevelUp()
        {
            base.LevelUp();
            CalculateLaserWidth();
        }
        #endregion
    }
}