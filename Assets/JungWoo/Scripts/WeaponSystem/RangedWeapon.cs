using DiceSurvivor.Manager;
using System.Collections.Generic;
using UnityEngine;

namespace DiceSurvivor.Weapon
{
    /// <summary>
    /// 원거리 무기 기본 클래스
    /// </summary>
    public abstract class RangedWeapon : MonoBehaviour
    {
        [Header("Base Settings")]
        [SerializeField] protected string weaponName;
        [SerializeField] protected int currentLevel = 1;

        [Header("Weapon Stats")]
        protected float damage;
        protected float cooldown;
        protected float radius;
        protected float range;
        protected float projectileSize;
        protected float projectileSpeed;
        protected int projectileCount;
        protected float explosionRadius;
        protected float explosionDamage;
        protected float dotDamage;
        protected float duration;
        protected bool isPiercing;
        protected bool canReturn;

        [Header("Runtime")]
        protected float lastAttackTime;
        protected Transform player;

        protected virtual void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null)
            {
                Debug.LogError($"{weaponName}: Player not found!");
            }

            InitializeWeapon();
        }

        protected virtual void Update()
        {
            if (player == null) return;

            // 쿨다운 체크
            if (Time.time - lastAttackTime >= cooldown)
            {
                PerformAttack();
                lastAttackTime = Time.time;
            }
        }

        /// <summary>
        /// 무기 초기화
        /// </summary>
        protected abstract void InitializeWeapon();

        /// <summary>
        /// 공격 수행
        /// </summary>
        protected abstract void PerformAttack();

        /// <summary>
        /// 무기 스탯 업데이트
        /// </summary>
        public virtual void UpdateWeaponStats(WeaponStats stats)
        {
            if (stats == null)
            {
                Debug.LogError($"{weaponName}: WeaponStats is null!");
                return;
            }

            damage = stats.damage;
            cooldown = stats.cooldown;
            radius = stats.radius;
            range = stats.range;
            projectileSize = stats.projectileSize;
            projectileSpeed = stats.projectileSpeed;                 // DataTable에서 projectileSpeed 값을 travelTime으로 할당
            projectileCount = stats.projectileCount;
            explosionRadius = stats.explosionRadius;
            explosionDamage = stats.explosionDamage;
            dotDamage = stats.dotDamage;
            duration = stats.duration;
            isPiercing = stats.isPiercing;
            canReturn = stats.canReturn;

            Debug.Log($"{weaponName} Lv.{currentLevel} - Damage: {damage}, Speed: {projectileSpeed}, Count: {projectileCount}");
        }

        /// <summary>
        /// 레벨 업
        /// </summary>
        public virtual void LevelUp()
        {
            currentLevel++;
            LoadWeaponData();
        }

        /// <summary>
        /// 무기 데이터 로드
        /// </summary>
        protected abstract void LoadWeaponData();

        /// <summary>
        /// 가장 가까운 적 찾기
        /// </summary>
        protected GameObject FindClosestEnemy()
        {
            GameObject closest = null;
            float minDistance = float.MaxValue;

            int enemyLayer = LayerMask.GetMask("Enemy");
            Collider[] enemies = Physics.OverlapSphere(transform.position, 50f, enemyLayer);

            foreach (var enemy in enemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = enemy.gameObject;
                }
            }

            return closest;
        }

        protected virtual void OnDrawGizmosSelected()
        {
            // 공격 범위 표시
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}