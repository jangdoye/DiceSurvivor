/*using System.Collections.Generic;
using UnityEngine;
using DiceSurvivor.Manager;

namespace DiceSurvivor.Weapon
{
    /// <summary>
    /// 범위 무기 기본 클래스
    /// </summary>
    public abstract class SplashWeaponBases : MonoBehaviour
    {
        [Header("Base Settings")]
        [SerializeField] protected string weaponName;
        [SerializeField] protected int currentLevel = 1;
        [SerializeField] protected int lostLevel = 1;

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
        protected bool isAttacking;
        protected Transform player;

        // 범위 내 적들을 관리하기 위한 딕셔너리
        protected Dictionary<GameObject, float> enemiesInRange = new Dictionary<GameObject, float>();

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

            // 쿨다운이 있는 무기는 쿨다운 체크
            if (cooldown > 0)
            {
                if (Time.time - lastAttackTime >= cooldown)
                {
                    PerformAttack();
                    lastAttackTime = Time.time;
                }
            }
            else
            {
                // 쿨다운이 0인 무기는 지속적으로 공격
                ContinuousAttack();
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
        /// 지속 공격 (쿨다운이 0인 무기용)
        /// </summary>
        protected virtual void ContinuousAttack(
           )
        {
            // 기본적으로는 PerformAttack 호출
            PerformAttack();
        }

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
            projectileSpeed = stats.projectileSpeed;
            projectileCount = stats.projectileCount;
            explosionRadius = stats.explosionRadius;
            explosionDamage = stats.explosionDamage;
            dotDamage = stats.dotDamage;
            duration = stats.duration;
            isPiercing = stats.isPiercing;
            canReturn = stats.canReturn;

            Debug.Log($"{weaponName} Lv.{currentLevel} - Radius: {radius}, DotDamage: {dotDamage}, Duration: {duration}");
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
        /// 범위 내 적 감지
        /// </summary>
        protected virtual Collider[] GetEnemiesInRange(float detectionRadius)
        {
            int enemyLayer = LayerMask.GetMask("Enemy");
            return Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        }

        /// <summary>
        /// 데미지 적용
        /// </summary>
        protected virtual void ApplyDamage(GameObject enemy, float damageAmount)
        {
            if (enemy == null)
            {
                Debug.LogWarning("[SplashWeapon] ApplyDamage: enemy가 null입니다");
                return;
            }

            Debug.Log($"[SplashWeapon] {enemy.name}에게 데미지 {damageAmount} 적용 시도");

            // Enemy 컴포넌트를 찾아서 데미지 적용
            var enemyComponent = enemy.GetComponent<IDamageable>();
            if (enemyComponent != null)
            {
                enemyComponent.TakeDamage(damageAmount);
                Debug.Log($"[SplashWeapon] IDamageable 인터페이스로 데미지 적용 성공");
            }
            else
            {
                // IDamageable이 없으면 Health 컴포넌트 직접 찾기
                var health = enemy.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damageAmount);
                    Debug.Log($"[SplashWeapon] Health 컴포넌트로 데미지 적용 성공");
                }
                else
                {
                    Debug.LogError($"[SplashWeapon] {enemy.name}에서 IDamageable 또는 Health 컴포넌트를 찾을 수 없습니다!");
                }
            }
        }

        /// <summary>
        /// 감속 효과 적용
        /// </summary>
        

        protected virtual void OnDrawGizmosSelected()
        {
            // 범위 표시
            if (radius > 0)
            {
                Gizmos.color = new Color(1, 0, 0, 0.3f);
                Gizmos.DrawWireSphere(transform.position, radius);
            }
        }
    }

   

    // 임시 Health 컴포넌트 (실제 프로젝트에 있다면 제거)
    public class Health : MonoBehaviour
    {
        public float currentHealth = 100f;

        public void TakeDamage(float damage)
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}*/