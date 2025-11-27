using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DiceSurvivor.Manager;
using DiceSurvivor.Enemy;

namespace DiceSurvivor.Weapon
{
    /// <summary>
    /// Asteroid 무기 - 플레이어 주위를 공전하는 소행성
    /// </summary>
    public class AsteroidWeapon : SplashWeaponBase
    {
        #region Variables
        [Header("Asteroid Specific")]
        [SerializeField] private GameObject asteroidPrefab;             // 소행성 프리팹
        [SerializeField] private float orbitHeight = 0.2f;              // 공전 높이
        [SerializeField] private Transform player;                      //Player 위치

        [Header("Runtime")]
        private float spawnTimer = 0f;                                  // 스폰 타이머
        private List<AsteroidOrbit> activeAsteroids;                    // 활성 소행성 목록
        private Coroutine deactivateCoroutine;                          // 비활성화 코루틴
        private const float MinCooldown = 0.1f;                         // 1프레임 생성 방지용 최소 쿨다운

        #endregion

        #region Property
        private bool HasActiveWave => activeAsteroids != null && activeAsteroids.Count > 0;

        // 7레벨 이상에서 duration과 cooldown이 같은지 체크
        private bool IsInstantReplace => Weapon.level >= 7 && Weapon.duration >= Weapon.cooldown;
        #endregion

        #region Unity Event Method
        protected override void Awake()
        {

            base.Awake();

            // 리스트 초기화
            activeAsteroids = new List<AsteroidOrbit>();

            if (asteroidPrefab == null)
            {
                Debug.LogError("[Asteroid] asteroidPrefab이 비어 있습니다. 인스펙터에서 지정하세요.");
            }
        }

        protected override void Update()
        {
            if (player == null) return;

            // 쿨다운 체크
            spawnTimer += Time.deltaTime;

            // 활성 파동이 없을 때만 새 파동 생성 (프레임당 무한 생성 방지)
            float cd = Mathf.Max(Weapon.cooldown, MinCooldown);
            if (!HasActiveWave && spawnTimer >= cd)
            {
                PerformAttack();
                spawnTimer = 0f;
            }

            // 활성 소행성들 업데이트
            UpdateActiveAsteroids();

            // 비활성 소행성 정리
            CleanupInactiveAsteroids();
        }
        private void OnDrawGizmosSelected()
        {
            // 공전 궤도 표시
            Gizmos.color = new Color(0.5f, 0.3f, 0.2f, 0.3f);
            if (player != null)
            {
                Gizmos.DrawWireSphere(player.position, Weapon.radius);
            }
            else
            {
                Gizmos.DrawWireSphere(transform.position, Weapon.radius);
            }
        }

        private void OnDestroy()
        {
            if (deactivateCoroutine != null)
            {
                StopCoroutine(deactivateCoroutine);
            }
            CleanupAllAsteroids();
        }

        #endregion

        #region Custom Method
        /// <summary>
        /// 활성 소행성 업데이트
        /// </summary>
        private void UpdateActiveAsteroids()
        {
            if (activeAsteroids == null) return; // NRE 1차 방어

            for (int i = activeAsteroids.Count - 1; i >= 0; i--)
            {

                if (activeAsteroids[i] == null || !activeAsteroids[i].IsActive)
                {

                    activeAsteroids.RemoveAt(i);
                }
                else
                {
                    activeAsteroids[i].UpdateOrbit();
                }
            }
        }

        /// <summary>
        /// 공격 수행 - 소행성 생성
        /// </summary>
        protected override void PerformAttack()
        {
            if (asteroidPrefab == null || player == null) return;

            //if (HasActiveWave) return;

            // 기존 소행성 제거
            //CleanupAllAsteroids();

            // projectileCount 만큼 소행성 생성
            int count = Mathf.Max(1, Weapon.projectileCount);
            float angleStep = 360f / count;

            for (int i = 0; i < count; i++)
            {
                float angle = angleStep * i;
                CreateAsteroid(angle);
            }

            // duration 경과 후 전체 제거
            if (deactivateCoroutine != null) StopCoroutine(deactivateCoroutine);
            if (Weapon.duration > 0f)
                deactivateCoroutine = StartCoroutine(DeactivateAfterDuration());

            //Debug.Log($"[Asteroid] {Weapon.projectileCount}개 소행성 생성 - Duration: {Weapon.duration}초");
        }

        /// <summary>
        /// 소행성 생성
        /// </summary>
        private void CreateAsteroid(float startAngle)
        {
            // 소행성 생성 (플레이어가 아닌 월드 공간에 생성)
            GameObject asteroid = Instantiate(asteroidPrefab, player.position, Quaternion.identity);
            weaponController.PlaySfx();

            asteroid.transform.SetParent(this.transform, worldPositionStays: true);

            // AsteroidOrbit 컴포넌트 추가/설정
            AsteroidOrbit orbit = asteroid.GetComponent<AsteroidOrbit>();


            // 소행성 초기화
            orbit.Initialize(
                player,                 // 중심점 (플레이어)
                Weapon.radius,                 // 공전 반경
                Weapon.projectileSize,         // 크기
                Weapon.damage,                 // 데미지
                Weapon.projectileSpeed,        // 회전 속도
                startAngle,            // 시작 각도
                orbitHeight            // 공전 높이
            );
            activeAsteroids.Add(orbit);
        }

        /// <summary>
        /// duration 후 비활성화
        /// </summary>
        private IEnumerator DeactivateAfterDuration()
        {
            yield return new WaitForSeconds(Weapon.duration);

            // 7레벨 이상이고 duration과 cooldown이 같으면 즉시 제거
            if (IsInstantReplace)
            {
                CleanupAllAsteroidsInstant();
                //Debug.Log($"[Asteroid] Lv.{Weapon.level} - Duration 종료, 즉시 교체");
            }
            else
            {
                CleanupAllAsteroids();
                //Debug.Log($"[Asteroid] Duration 종료 - 소행성 비활성화");
            }
        }

        /// <summary>
        /// 비활성 소행성 정리
        /// </summary>
        private void CleanupInactiveAsteroids()
        {
            if (activeAsteroids == null) return;
            activeAsteroids.RemoveAll(asteroid => asteroid == null || !asteroid.IsActive);
        }

        /// <summary>
        /// 모든 소행성 제거
        /// </summary>
        private void CleanupAllAsteroids()
        {
            if (activeAsteroids == null) return;
            foreach (var asteroid in activeAsteroids)
            {
                if (asteroid != null)
                {
                    asteroid.Deactivate();
                }
            }
            activeAsteroids.Clear();
        }

        /// <summary>
        /// 모든 소행성 즉시 제거 (7-8레벨용)
        /// </summary>
        private void CleanupAllAsteroidsInstant()
        {
            if (activeAsteroids == null) return;
            foreach (var asteroid in activeAsteroids)
            {
                if (asteroid != null && asteroid.gameObject != null)
                {
                    // 즉시 제거
                    Destroy(asteroid.gameObject);
                }
            }
            activeAsteroids.Clear();
        }

        public override void LevelUp()
        {
            base.LevelUp();

            // 레벨업 시 즉시 새로운 소행성 생성
            // 7-8레벨은 즉시 교체, 그 외는 기존 방식
            if (IsInstantReplace)
            {
                CleanupAllAsteroidsInstant();
            }
            else
            {
                CleanupAllAsteroids();
            }
            spawnTimer = 0f;
            PerformAttack();
        }
        #endregion
    }
}