using System.Collections.Generic;
using UnityEngine;
using DiceSurvivor.Manager;
using DiceSurvivor.Enemy;
using System.Collections;

namespace DiceSurvivor.Weapon
{
    /// <summary>
    /// KillingAura 무기 - 자기 주변 원형 범위에 지속적으로 DoT 데미지를 주는 무기
    /// </summary>
    public class KillingAuraWeapon : SplashWeaponBase
    {
        #region Variables
        [Header("KillingAura Specific")]
        //[SerializeField] private float dotInterval = 0.5f; // DoT 데미지 간격 (1초)

        // 범위 내 적 배열 관리
        private List<GameObject> enemiesInAura = new List<GameObject>();

        private Dictionary<GameObject, Coroutine> activeDots = new Dictionary<GameObject, Coroutine>();
        #endregion

        #region Unity Event Method
        protected override void Awake()
        {
            base.Awake();
            weaponController.PlaySfx();
        }

        protected override void Update()
        {
            // 범위를 벗어난 적 정리
            CleanupNullEnemies();
            
        }
        private void FixedUpdate()
        {
            UpdateParticleScale();
        }

        /// <summary>
        /// 트리거 진입 - Enemy Layer 감지
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            // EnemyFinal이 붙어있는 경우
            EnemyFinal enemy = other.GetComponent<EnemyFinal>();
            if (enemy != null)
            {
                if (!enemiesInAura.Contains(other.gameObject))
                {
                    enemiesInAura.Add(other.gameObject);

                    // DOT 시작
                    Coroutine dot = StartCoroutine(ApplyDot(other.gameObject));
                    activeDots.Add(other.gameObject, dot);

                    EffectSlow.ApplySlow(other.gameObject, Weapon.duration);
                }
            }
        }

        /// <summary>
        /// 트리거 체류 - 지속적인 감속 효과 갱신
        /// </summary>
        private void OnTriggerStay(Collider other)
        {
            // Enemy 레이어만 처리
            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                // 감속 효과 갱신 (매 프레임)
                if (Weapon.duration > 0 && other.gameObject != null)
                {
                    // 감속 효과 지속적으로 갱신
                    EffectSlow.ApplySlow(other.gameObject, Weapon.duration);
                }
            }
        }

        /// <summary>
        /// 트리거 퇴장 - Enemy가 범위를 벗어남
        /// </summary>
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                // enemiesInAura에서 제거
                enemiesInAura.Remove(other.gameObject);

                // DOT 중단
                if (activeDots.ContainsKey(other.gameObject))
                {
                    StopCoroutine(activeDots[other.gameObject]);
                    activeDots.Remove(other.gameObject);
                }

                // 감속 초기화 (원래 속도 복귀)
                EffectSlow.ApplySlow(other.gameObject, 0f);
            }
        }

        /// <summary>
        /// Gizmo 그리기 (디버그용)
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            // 공격 범위 표시
            Gizmos.color = new Color(0.5f, 0f, 0.5f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, Weapon.radius);

            // 범위 내 적과의 연결선 표시
            Gizmos.color = Color.red;
            foreach (var enemy in enemiesInAura)
            {
                if (enemy != null)
                {
                    Gizmos.DrawLine(transform.position, enemy.transform.position);
                }
            }
        }

        /// <summary>
        /// 오브젝트 파괴 시 정리
        /// </summary>
        private void OnDestroy()
        {
            // 배열 정리
            enemiesInAura.Clear();
            activeDots.Clear();

        }
        #endregion

        #region Custom Method
        protected override void Attack()
        {
            ApplyDotToAllEnemiesInRange();
        }

        /// <summary>
        /// Aura Collider 설정 (Layer 감지용)
        /// </summary>
        private void SetupAuraCollider()
        {
            // 기존 콜라이더 제거
            Collider[] colliders = GetComponents<Collider>();
            foreach (var col in colliders)
            {
                Destroy(col);
            }

            // 새 Sphere Collider 추가 (트리거)
            SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = Weapon.radius;

            //Debug.Log($"[KillingAura] Collider 설정 - 반경: {Weapon.radius}");
        }

        /// <summary>
        /// 【위치: Line 121-142】 파티클 스케일 업데이트 (radius와 연동)
        /// </summary>
        public void UpdateParticleScale()
        {
            // radius에 비례하여 파티클 스케일 설정
            float scaleMultiplier = Weapon.radius;
            transform.localScale = new Vector3(scaleMultiplier, 1, scaleMultiplier);
        }

        /// <summary>
        /// 범위 내 모든 적에게 DoT 데미지 및 감속 적용 (1초마다 실행)
        /// </summary>
        private void ApplyDotToAllEnemiesInRange()
        {
            // 배열 복사본으로 작업 (순회 중 수정 방지)
            List<GameObject> enemiesCopy = new List<GameObject>(enemiesInAura);

            // 적이 없으면 리턴
            if (enemiesCopy.Count == 0) return;

            //Debug.Log($"[KillingAura] DoT 타이머 발동! 범위 내 적: {enemiesCopy.Count}명");

            int damageAppliedCount = 0;

            foreach (var enemy in enemiesCopy)
            {
                // null 체크
                if (enemy == null)
                {
                    continue;
                }

                // DoT 데미지 적용
                if (Weapon.dotDamage > 0)
                {
                    // Enemy 컴포넌트 직접 찾아서 데미지 적용
                    var enemyComponent = enemy.GetComponent<EnemyFinal>();
                    if (enemyComponent != null)
                    {
                        enemyComponent.TakeDamage(Weapon.dotDamage * Weapon.duration);
                        damageAppliedCount++;
                    }
                }

                // 감속 효과 적용 (duration 값 사용)
                if (Weapon.duration > 0)
                {
                    EffectSlow.ApplySlow(enemy, Weapon.duration);
                }
            }
            //Debug.Log($"[KillingAura] DoT 데미지 적용 완료: {damageAppliedCount}명에게 {dotDamage} 데미지");
        }

        /// <summary>
        /// null이 된 적 제거
        /// </summary>
        private void CleanupNullEnemies()
        {
            // null 적 제거
            enemiesInAura.RemoveAll(enemy => enemy == null);

            // DoT 시간 딕셔너리에서도 제거
            List<GameObject> toRemove = new List<GameObject>();
            foreach (var kvp in activeDots)
            {
                if (kvp.Key == null)
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var enemy in toRemove)
            {
                activeDots.Remove(enemy);
            }
        }

        private IEnumerator ApplyDot(GameObject enemy)
        {
            while (enemy != null && enemiesInAura.Contains(enemy))
            {
                ApplyDamage(enemy, Weapon.dotDamage);
                yield return new WaitForSeconds(Weapon.duration); // 0.5초마다
            }
        }

        /// <summary>
        /// 현재 범위 내 적 수 반환
        /// </summary>
        public int GetEnemyCount()
        {
            CleanupNullEnemies();
            return enemiesInAura.Count;
        }

        /// <summary>
        /// 범위 내 적 배열 반환
        /// </summary>
        public List<GameObject> GetEnemiesInRange()
        {
            CleanupNullEnemies();
            return new List<GameObject>(enemiesInAura);
        }
        protected override void PerformAttack() { }
    }
    #endregion
}