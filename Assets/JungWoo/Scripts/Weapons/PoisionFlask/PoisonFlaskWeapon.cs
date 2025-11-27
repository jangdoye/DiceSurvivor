using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DiceSurvivor.Manager;
using DiceSurvivor.Enemy;

namespace DiceSurvivor.Weapon
{
    /// <summary>
    /// PoisonFlask 무기 - 곡선 투척 DoT 투사체
    /// </summary>
    public class PoisonFlaskWeapon : RangedWeaponBase
    {
        [Header("PoisonFlask Specific")]
        [SerializeField] private GameObject poisonGasPrefab;           // 독가스 DoT 프리팹
        [SerializeField] private float arcHeight = 3f;                 // 포물선 높이
        [SerializeField] private float throwDuration = 1f;             // 투척 시간

        [Header("Runtime")]
        private List<PoisonFlaskProjectile> activeFlasks;              // 활성 플라스크 목록
        private float attackTimer = 0f;                                // 공격 타이머
        private float sfxTimer = 0f;                                   // 소리 타이머

        protected override void Awake()
        {
            base.Awake(); 
        }

        private void Start()
        {
            activeFlasks = new List<PoisonFlaskProjectile>();
        }

        protected override void Update()
        {
            
            // 쿨다운 체크
            attackTimer += Time.deltaTime;

            if (attackTimer >= cooldown)
            {
                Attack();
                attackTimer = 0f;
            }

            // 비활성 플라스크 정리
            CleanupInactiveFlasks();
        }

        protected override void ShootProjectile(GameObject target, int projectileCount)
        {
            // 가장 가까운 적들 찾기
            List<GameObject> closestEnemies = FindClosestEnemies(projectileCount);

            // 각 타겟에게 플라스크 투척
            for (int i = 0; i < projectileCount; i++)
            {
                if (i < closestEnemies.Count)
                {
                    target = closestEnemies[i];
                }

                ThrowFlask(target);
            }
        }

        /// <summary>
        /// 가장 가까운 N개의 적 찾기
        /// </summary>
        private List<GameObject> FindClosestEnemies(int count)
        {
            List<GameObject> enemies = new List<GameObject>();

            int enemyLayer = LayerMask.GetMask("Enemy");
            Collider[] allEnemies = Physics.OverlapSphere(transform.position, 50f, enemyLayer);

            // 거리순 정렬
            System.Array.Sort(allEnemies, (a, b) =>
            {
                float distA = Vector3.Distance(transform.position, a.transform.position);
                float distB = Vector3.Distance(transform.position, b.transform.position);
                return distA.CompareTo(distB);
            });

            // 상위 N개 선택
            for (int i = 0; i < Mathf.Min(count, allEnemies.Length); i++)
            {
                enemies.Add(allEnemies[i].gameObject);
            }

            return enemies;
        }

        /// <summary>
        /// 플라스크 투척
        /// </summary>
        private void ThrowFlask(GameObject target)
        {
            // 타겟 위치 결정
            Vector3 targetPosition;
            if (target != null)
            {
                targetPosition = target.transform.position;
            }
            else
            {
                // 타겟이 없으면 전방 랜덤 위치
                float randomAngle = Random.Range(-30f, 30f);
                Vector3 direction = Quaternion.Euler(0, randomAngle, 0) * transform.forward;
                targetPosition = transform.position + direction * Weapon.range;
            }

            
            // 플라스크 생성
            GameObject flask = Instantiate(projectilePrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
            sfxTimer += Time.deltaTime;
            if (sfxTimer >= throwDuration)
            {
                weaponController.PlaySfx();
                sfxTimer = 0f;
            }


            // PoisonFlaskProjectile 컴포넌트 추가/설정
            PoisonFlaskProjectile projectile = flask.GetComponent<PoisonFlaskProjectile>();
            
            // 플라스크 초기화
            projectile.Initialize(
                transform.position,     // 시작 위치
                targetPosition,         // 목표 위치
                arcHeight,              // 포물선 높이
                throwDuration,          // 투척 시간
                Weapon.explosionDamage,        // 폭발 데미지
                Weapon.explosionRadius,        // 폭발 범위
                Weapon.dotDamage,              // DoT 데미지
                Weapon.duration,               // DoT 지속시간
                Weapon.projectileSize,         // 크기
                poisonGasPrefab,        // 독가스 프리팹
                this
            );

            activeFlasks.Add(projectile);

            //Debug.Log($"[PoisonFlask] 투척! 타겟: {(target != null ? target.name : "없음")}");
        }

        /// <summary>
        /// 비활성 플라스크 정리
        /// </summary>
        private void CleanupInactiveFlasks()
        {
            activeFlasks.RemoveAll(flask => flask == null || !flask.IsActive);
        }
    }

    
}