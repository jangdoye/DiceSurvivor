using UnityEngine;
using System.Collections;
using DiceSurvivor.Weapon;

namespace DiceSurvivor.Enemy
{
    /// <summary>
    /// 적 기본 클래스 - IDamageable, IMovement 인터페이스 구현
    /// </summary>
    public class WJEnemy : MonoBehaviour, IDamageable, IMovement
    {
        [Header("Enemy Stats")]
        [SerializeField] private float maxHealth = 100f;          // 최대 체력
        [SerializeField] private float currentHealth;             // 현재 체력
        [SerializeField] private float moveSpeed = 3f;            // 기본 이동 속도
        [SerializeField] private float damage = 10f;              // 공격력
        [SerializeField] private float attackRange = 1.5f;        // 공격 범위
        [SerializeField] private float attackCooldown = 1f;       // 공격 쿨다운

        [Header("Movement")]
        [SerializeField] private float currentMoveSpeed;          // 현재 이동 속도 (감속 적용)
        [SerializeField] private bool isSlowed = false;           // 감속 상태
        private Coroutine slowCoroutine;                          // 감속 코루틴

        [Header("Target")]
        [SerializeField] private Transform target;                // 추적 대상 (플레이어)
        [SerializeField] private float detectionRange = 15f;      // 감지 범위

        [Header("Visual")]
        [SerializeField] private Renderer enemyRenderer;          // 렌더러 (색상 변경용)
        private Color originalColor;                              // 원래 색상

        [Header("State")]
        [SerializeField] private bool isDead = false;             // 사망 여부
        [SerializeField] private bool isAttacking = false;        // 공격 중 여부
        private float lastAttackTime;                             // 마지막 공격 시간

        /// <summary>
        /// 초기화
        /// </summary>
        void Start()
        {
            // 체력 초기화
            currentHealth = maxHealth;
            currentMoveSpeed = moveSpeed;

            // 플레이어 찾기
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }

            // 렌더러 찾기
            if (enemyRenderer == null)
            {
                enemyRenderer = GetComponent<Renderer>();
            }

            // 원래 색상 저장
            if (enemyRenderer != null)
            {
                originalColor = enemyRenderer.material.color;
            }

            // Enemy 레이어 설정
            gameObject.layer = LayerMask.NameToLayer("Enemy");
        }

        /// <summary>
        /// 매 프레임 업데이트
        /// </summary>
        void Update()
        {
            if (isDead) return;

            // 타겟이 있으면 추적
            if (target != null)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                // 감지 범위 내에 있으면
                if (distanceToTarget <= detectionRange)
                {
                    // 공격 범위 내에 있으면 공격
                    if (distanceToTarget <= attackRange)
                    {
                        AttackTarget();
                    }
                    else
                    {
                        // 공격 범위 밖이면 이동
                        //MoveToTarget();
                    }
                }
                else
                {
                    // 감지 범위 밖이면 대기
                    Idle();
                }
            }
        }

        /// <summary>
        /// 타겟으로 이동
        /// </summary>
        private void MoveToTarget()
        {
            if (target == null || isAttacking) return;

            // 타겟 방향 계산
            Vector3 direction = (target.position - transform.position).normalized;

            // 이동 (감속 적용된 속도 사용)
            transform.position += direction * currentMoveSpeed * Time.deltaTime;

            // 타겟 바라보기
            transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
        }

        /// <summary>
        /// 타겟 공격
        /// </summary>
        private void AttackTarget()
        {
            if (target == null) return;

            // 쿨다운 체크
            if (Time.time - lastAttackTime < attackCooldown) return;

            // 공격 수행
            isAttacking = true;

            // 플레이어에게 데미지 (실제 구현 시 Player 스크립트와 연동)
            //Debug.Log($"{name}이(가) 플레이어를 공격! 데미지: {damage}");

            // 타겟 바라보기
            transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));

            lastAttackTime = Time.time;

            // 공격 애니메이션 후 상태 복구
            Invoke(nameof(EndAttack), 0.5f);
        }

        /// <summary>
        /// 공격 종료
        /// </summary>
        private void EndAttack()
        {
            isAttacking = false;
        }

        /// <summary>
        /// 대기 상태
        /// </summary>
        private void Idle()
        {
            // 대기 애니메이션이나 행동
        }

        /// <summary>
        /// 데미지 받기 (IDamageable 구현)
        /// </summary>
        public void TakeDamage(float damageAmount)
        {
            if (isDead) return;

            // 체력 감소
            currentHealth -= damageAmount;

            // 상세 로그 출력
            //Debug.Log($"[Enemy] {gameObject.name} 데미지 받음!");
            //Debug.Log($"[Enemy] 받은 데미지: {damageAmount}");
            //Debug.Log($"[Enemy] 남은 체력: {currentHealth}/{maxHealth}");
            //Debug.Log($"[Enemy] 체력 비율: {(currentHealth / maxHealth * 100):F1}%");

            // 피격 효과
            StartCoroutine(DamageEffect());

            // 사망 체크
            if (currentHealth <= 0)
            {
                //Debug.Log($"[Enemy] {gameObject.name} 체력 0 도달 - 사망 처리");
                Die();
            }
        }

        /// <summary>
        /// 피격 효과
        /// </summary>
        private IEnumerator DamageEffect()
        {
            if (enemyRenderer != null)
            {
                // 빨간색으로 변경
                enemyRenderer.material.color = Color.red;
                yield return new WaitForSeconds(0.1f);

                // 원래 색상으로 복구 (감속 중이면 파란색 유지)
                if (isSlowed)
                {
                    enemyRenderer.material.color = Color.blue;
                }
                else
                {
                    enemyRenderer.material.color = originalColor;
                }
            }
        }

        /// <summary>
        /// 감속 적용 (IMovement 구현)
        /// </summary>
        public void ApplySlow(float slowPercent, float duration)
        {
            // 기존 감속 코루틴 중지
            if (slowCoroutine != null)
            {
                StopCoroutine(slowCoroutine);
            }

            // 새로운 감속 적용
            slowCoroutine = StartCoroutine(SlowEffect(slowPercent, duration));
        }

        /// <summary>
        /// 감속 효과 코루틴
        /// </summary>
        private IEnumerator SlowEffect(float slowPercent, float duration)
        {
            // 감속 적용
            isSlowed = true;
            currentMoveSpeed = moveSpeed * (1f - slowPercent);
            //EnemyActivity의 이동속도를 조절하는걸로 변경

            // 시각적 효과 (파란색)
            if (enemyRenderer != null)
            {
                enemyRenderer.material.color = Color.blue;
            }

            //Debug.Log($"{name} 감속됨: {slowPercent * 100}% 감소, {duration}초 동안");

            // 지속 시간 대기
            yield return new WaitForSeconds(duration);

            // 감속 해제
            isSlowed = false;
            currentMoveSpeed = moveSpeed;

            // 색상 복구
            if (enemyRenderer != null)
            {
                enemyRenderer.material.color = originalColor;
            }

            //Debug.Log($"{name} 감속 해제됨");
        }

        /// <summary>
        /// 사망 처리
        /// </summary>
        private void Die()
        {
            if (isDead) return;

            isDead = true;
            //Debug.Log($"{name} 사망!");

            // 사망 효과
            StartCoroutine(DeathEffect());
        }

        /// <summary>
        /// 사망 효과
        /// </summary>
        private IEnumerator DeathEffect()
        {
            // 크기 축소 애니메이션
            float elapsedTime = 0f;
            float shrinkDuration = 0.5f;
            Vector3 originalScale = transform.localScale;

            while (elapsedTime < shrinkDuration)
            {
                transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, elapsedTime / shrinkDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // 오브젝트 파괴
            Destroy(gameObject);
        }

        /// <summary>
        /// 체력 회복
        /// </summary>
        public void Heal(float healAmount)
        {
            currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
            //Debug.Log($"{name} 체력 회복: {healAmount}, 현재 체력: {currentHealth}/{maxHealth}");
        }

        /// <summary>
        /// 현재 체력 비율 반환
        /// </summary>
        public float GetHealthPercentage()
        {
            return currentHealth / maxHealth;
        }

        /// <summary>
        /// Gizmo 그리기 (디버그용)
        /// </summary>
        void OnDrawGizmosSelected()
        {
            // 감지 범위
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // 공격 범위
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}