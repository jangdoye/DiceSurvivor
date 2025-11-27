using DiceSurvivor.Audio;
using DiceSurvivor.Manager;
using DiceSurvivor.Enemy;
using DiceSurvivor.Weapon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceSurvivor.Weapon
{
    /// <summary>
    /// 모든 무기의 최상위 베이스
    /// - WeaponController 참조
    /// - 공통 쿨다운 로직
    /// - Attack() 추상 메서드
    /// </summary>
    public abstract class WeaponAttackBase : MonoBehaviour
    {
        #region Variables
        protected WeaponController weaponController;
        protected float cooldown;
        

        [SerializeField]
        protected WeaponStats weapon;
        #endregion


        #region Properties
        public WeaponStats Weapon
        {
            get
            {
                return weapon;
            }
            set
            {
                weapon = value;
            }
        }
        #endregion

        #region Interface
        // 인터페이스들
        public interface IDamageable
        {
            void TakeDamage(float damage);
        }        
        #endregion

        #region Unity Event Method

        protected virtual void Awake()
        {
            LoadWeaponData();
        }

        protected virtual void Update()
        {            
            if (cooldown > 0)
                cooldown -= Time.deltaTime;

            if (cooldown <= 0)
            {
                Attack();
                cooldown = Weapon.cooldown;
            }
        }
        #endregion

        #region Custom Method
        public virtual void LevelUp()
        {
            LoadWeaponData();
        }
        private void LoadWeaponData()
        {
            if (GetComponentInParent<WeaponController>() != null)
                weaponController = GetComponentInParent<WeaponController>();
            if (weaponController != null)
            {
                weaponController.OnWeaponLoaded += weaponStats =>
                {
                    Weapon = weaponStats;
                    //Debug.Log($"무기 로딩 완료: {Weapon.id}");
                };
            }
        }

        /// <summary>
        /// 자식 클래스에서 구현해야 하는 공격 로직
        /// </summary>
        protected abstract void Attack();

        /// <summary>
        /// 데미지 적용
        /// </summary>
        public virtual void ApplyDamage(GameObject enemy, float damageAmount)
        {
            if (enemy == null)
            {                
                return;
            }
                // IDamageable이 없으면 Health 컴포넌트 직접 찾기
                var health = enemy.GetComponent<EnemyFinal>();
                if (health != null)
                {
                    health.TakeDamage(damageAmount);                    
                }
                else
                {
                    Debug.LogError($"[SplashWeapon] {enemy.name}에서 IDamageable 또는 Health 컴포넌트를 찾을 수 없습니다!");
                }
            
        }
        #endregion
    }

    // ───────────────────────────────────────────────

    /// <summary>
    /// 근접 무기용 베이스
    /// - 가까운 적을 탐색해 즉시 공격하는 방식
    /// </summary>
    public abstract class MeleeWeaponBase : WeaponAttackBase
    {
        #region Custom Method
        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Enemy")
            {
                ApplyDamage(other.gameObject, Weapon.damage);
            }
        }

        protected override void Attack()
        {

        }
        #endregion
    }

    // ───────────────────────────────────────────────

    /// <summary>
    /// 원거리 무기용 베이스
    /// - 투사체를 발사하는 방식
    /// </summary>
    public abstract class RangedWeaponBase : WeaponAttackBase
    {
        #region Variables
        public GameObject projectilePrefab;
        public Transform firePoint;
        #endregion

        private void Start()
        {
            cooldown = Weapon.cooldown;
        }

        #region Custom Method
        protected override void Attack()
        {            
            GameObject target = FindEnemy();
            if (target != null)
            {
                ShootProjectile(target, Weapon.projectileCount);
                cooldown = Weapon.cooldown;
            }
        }

        protected abstract void ShootProjectile(GameObject target, int projectileCount);

        protected GameObject FindEnemy()
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
        #endregion

    }

    // ───────────────────────────────────────────────

    /// <summary>
    /// 범위(스플래시) 무기용 베이스
    /// - 일정 반경 내의 적들에게 지속 데미지/슬로우 적용
    /// </summary>
    public abstract class SplashWeaponBase : WeaponAttackBase
    {
        #region Interface
        public interface IMovement
        {
            void ApplySlow(float slowPercent, float duration);
        }

        #endregion

        #region Custom Method
        protected override void Attack()
        {
            if (cooldown > 0) return;

            Collider[] enemies = Physics.OverlapSphere(transform.position, Weapon.radius);
            foreach (var enemy in enemies)
            {
                if (enemy.TryGetComponent(out IDamageable dmg))
                {
                    ApplyDamage(enemy.gameObject, Weapon.dotDamage);
                    /*dmg.TakeDamage(Weapon.dotDamage * Time.deltaTime);*/
                }
            }

            cooldown = Weapon.cooldown;
        }
        /// <summary>
        /// 공격 수행
        /// </summary>
        protected abstract void PerformAttack();

        protected virtual void InitializeWeapon()
        {

        }
        #endregion

    }
}