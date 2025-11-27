using System.Collections.Generic;
using UnityEngine;

namespace DiceSurvivor.Weapon
{
    /// <summary>
    /// LightningStaff 무기 - 범위 내 가장 먼 적에게 번개 공격
    /// </summary>
    public class LightningStaffWeapon : SplashWeaponBase
    {
        #region Variables
        [Header("LightningStaff Specific")]
        [SerializeField] private float targetRandomOffset = 0.5f;        // 타겟 주변 랜덤 오프셋
        [SerializeField] private float secondaryDamageDelay = 0.3f;    // 2차 데미지 지연 시간
        [SerializeField] private GameObject lightningPrefab;           // 번개 프리팹
        [SerializeField] private GameObject lightningEffectPrefab;     // 번개 이펙트 프리팹
        [SerializeField] private Transform player;                      //Player 위치

        [Header("Runtime")]
        private float attackTimer = 0f;                                // 공격 타이머
        private List<LightningBolt> activeBolts;                       // 활성 번개 목록
        #endregion

        #region Unity Event Method
        protected override void Awake()
        {
            base.Awake();

            // 리스트 초기화
            activeBolts = new List<LightningBolt>();
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

            // 비활성 번개 정리
            //CleanupInactiveBolts();
        }

        /// <summary>
        /// 공격 수행
        /// </summary>
        protected override void PerformAttack()
        {
            // radius 범위 내에서 가장 먼 적들 찾기
            List<GameObject> farthestEnemies = GetFarthestEnemiesInRange();

            if (farthestEnemies.Count == 0)
            {
                //Debug.Log("[LightningStaff] 범위 내에 적이 없습니다.");
                return;
            }

            // projectileCount 만큼 번개 생성
            for (int i = 0; i < Weapon.projectileCount && i < farthestEnemies.Count; i++)
            {
                // 랜덤하게 선택
                int randomIndex = Random.Range(0, farthestEnemies.Count);
                GameObject target = farthestEnemies[randomIndex];

                if (target != null)
                {
                    StrikeLightning(target);
                    farthestEnemies.RemoveAt(randomIndex); // 중복 방지
                }
                else
                {
                    //Debug.Log($"target: {target}");
                }
            }
        }

        /// <summary>
        /// radius 범위 내에서 가장 먼 적들 찾기
        /// </summary>
        private List<GameObject> GetFarthestEnemiesInRange()
        {
            List<GameObject> enemiesInRange = new List<GameObject>();

            // radius 범위 내 모든 적 찾기
            int enemyLayer = LayerMask.GetMask("Enemy");
            Collider[] enemies = Physics.OverlapSphere(transform.position, Weapon.radius, enemyLayer);

            // 거리 계산 및 정렬
            System.Array.Sort(enemies, (a, b) =>
            {
                float distA = Vector3.Distance(transform.position, a.transform.position);
                float distB = Vector3.Distance(transform.position, b.transform.position);
                return distB.CompareTo(distA); // 내림차순 (먼 순서)
            });

            // GameObject 리스트로 변환
            foreach (var collider in enemies)
            {
                enemiesInRange.Add(collider.gameObject);
            }

            //Debug.Log($"[LightningStaff] 범위({Weapon.radius}) 내 적: {enemiesInRange.Count}명");

            return enemiesInRange;
        }

        /// <summary>
        /// 번개 공격
        /// </summary>
        private void StrikeLightning(GameObject target)
        {
            // 타겟 주변 랜덤 위치 계산
            Vector2 randomCircle = Random.insideUnitCircle * targetRandomOffset;
            Vector3 strikePosition = target.transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            // 번개 생성
            GameObject lightning = Instantiate(lightningPrefab, strikePosition, Quaternion.identity);
            weaponController.PlaySfx();

            // LightningBolt 컴포넌트 추가/설정
            LightningBolt bolt = lightning.GetComponent<LightningBolt>();
            if (bolt == null)
            {
                bolt = lightning.AddComponent<LightningBolt>();
            }

            // 번개 초기화
            bolt.Initialize(strikePosition, Weapon.damage, Weapon.projectileSize, Weapon.explosionDamage,
                          Weapon.explosionRadius, secondaryDamageDelay, lightningEffectPrefab);

            activeBolts.Add(bolt);

            //Debug.Log($"[LightningStaff] 번개 생성! 타겟: {target.name}, 위치: {strikePosition}");
        }

        /// <summary>
        /// 비활성 번개 정리
        /// </summary>
        private void CleanupInactiveBolts()
        {
            //activeBolts.RemoveAll(bolt => bolt == null || !bolt.IsActive);
        }

        private void OnDrawGizmosSelected()
        {
            // 공격 범위 표시
            Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, Weapon.radius);
        }
    }

    #endregion
}