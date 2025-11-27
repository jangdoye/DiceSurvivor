using System.Collections.Generic;
using UnityEngine;
using DiceSurvivor.Manager;
using DiceSurvivor.Enemy;

namespace DiceSurvivor.Weapon
{
    /// <summary>
    /// Icicle 무기 - 원거리 투척 + 폭발 + 도트 데미지
    /// </summary>
    public class IcicleWeapon : SplashWeaponBase
    {
        #region Variables
        [Header("Icicle Range Settings")]
        [SerializeField] private float minDistance = 5f;           // 최소 투척 거리
        [SerializeField] private float maxDistance = 15f;          // 최대 투척 거리

        [Header("Projectile Settings")]
        [SerializeField] private GameObject iciclePrefab;          // 고드름 프리팹
        [SerializeField] private float projectileHeight = 10f;     // 투사체 시작 높이
        [SerializeField] private float fallSpeed = 20f;            // 낙하 속도

        [Header("Effect Settings")]
        [SerializeField] private GameObject explosionEffectPrefab; // 폭발 이펙트 프리팹
        [SerializeField] private GameObject dotEffectPrefab;       // DoT 이펙트 프리팹
        [SerializeField] private Transform player;                 // Player 좌표

        [Header("Runtime")]
        private float attackTimer = 0f;                            // 공격 타이머
        private List<IcicleProjectile> activeProjectiles;          // 활성 투사체 목록
        private List<DotZone> activeDotZones;                      // 활성 DoT 구역 목록

        #endregion
        #region Unity Event Method
        protected override void Awake()
        {
            base.Awake();

            // 리스트 초기화
            activeProjectiles = new List<IcicleProjectile>();
            activeDotZones = new List<DotZone>();

            
        }

        protected override void Update()
        {
            if (player == null) return;

            // 쿨다운 체크
            attackTimer += Time.deltaTime;

            if (attackTimer >= Weapon.cooldown)
            {
                PerformAttack();
                attackTimer = 0f;
            }

            // 비활성 DoT 구역 정리
            CleanupInactiveDotZones();
        }

        /// <summary>
        /// 공격 수행
        /// </summary>
        protected override void PerformAttack()
        {
            // projectileCount 만큼 투사체 생성
            for (int i = 0; i < Weapon.projectileCount; i++)
            {
                LaunchIcicle();
            }
        }

        /// <summary>
        /// 고드름 투척
        /// </summary>
        private void LaunchIcicle()
        {
            // 무작위 목표 지점 선택 (플레이어로부터 일정 거리)
            Vector3 targetPosition = GetRandomTargetPosition();

            // 투사체 시작 위치 (목표 지점 위)
            Vector3 startPosition = targetPosition + Vector3.up * projectileHeight;

            // 고드름 생성
            GameObject icicle = Instantiate(iciclePrefab, startPosition, Quaternion.identity);
            weaponController.PlaySfx();

            // IcicleProjectile 컴포넌트 추가/설정
            IcicleProjectile projectile = icicle.GetComponent<IcicleProjectile>();
            if (projectile == null)
            {
                projectile = icicle.AddComponent<IcicleProjectile>();
            }

            // 투사체 설정
            projectile.Initialize(targetPosition, fallSpeed, Weapon.explosionRadius, Weapon.explosionDamage,
                                 Weapon.dotDamage, Weapon.duration, explosionEffectPrefab, dotEffectPrefab);

            activeProjectiles.Add(projectile);

            //Debug.Log($"[Icicle] 고드름 발사! 목표: {targetPosition}");
        }

        /// <summary>
        /// 무작위 목표 지점 선택
        /// </summary>
        private Vector3 GetRandomTargetPosition()
        {
            // 플레이어로부터 무작위 방향
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

            // 무작위 거리 (최소~최대 사이)
            float randomDistance = Random.Range(minDistance, maxDistance);

            // 목표 위치 계산
            Vector3 offset = new Vector3(
                Mathf.Cos(randomAngle) * randomDistance,
                0f,
                Mathf.Sin(randomAngle) * randomDistance
            );

            return player.position + offset;
        }

        /// <summary>
        /// 비활성 DoT 구역 정리
        /// </summary>
        private void CleanupInactiveDotZones()
        {
            activeDotZones.RemoveAll(zone => zone == null || !zone.IsActive);
        }

    }
    #endregion

}