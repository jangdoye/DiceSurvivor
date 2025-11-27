using DiceSurvivor.Manager;
using DiceSurvivor.Weapon;
using UnityEngine;
using System.Collections;
using DiceSurvivor.Player;
using DiceSurvivor.Audio;

namespace DiceSurvivor.Enemy 
{
    /// <summary>
    /// EnemyActivity와 WJEnemy의 기능을 합친 최종 적 클래스
    /// </summary>
    public class EnemyFinal : MonoBehaviour, IDamageable, IMovement
    {
        #region Variables
        // EnemyActivity 원본 변수들
        [SerializeField] protected EnemyData enemyData;
        protected EnemyData copiedData;
        public Transform playerPos;
        protected int blockedCounter = 0;
        protected float blockRadius = 0.25f;
        [SerializeField] protected float border = 2f;
        protected float outTimeCheck = 0f;
        [SerializeField] protected float outTimeThreshold = 2f;
        [SerializeField] protected float clusterThreshold = 2f;

        // WJEnemy 원본 변수들
        [SerializeField] private float currentHealth;
        [SerializeField] private float currentMoveSpeed;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private Transform target;
        [SerializeField] private float detectionRange = 15f;
        [SerializeField] private Renderer enemyRenderer;
        private Color originalColor;
        [SerializeField] private bool isDead = false;
        [SerializeField] private bool isAttacking = false;
        [SerializeField] private bool isSlowed = false;
        private float lastAttackTime;
        private Coroutine slowCoroutine;
        #endregion

        #region Properties
        public EnemyData EnemyData
        {
            get { return copiedData; }
            set { copiedData = value; }
        }
        #endregion

        #region Unity Event Methods
        protected virtual void Start()
        {
            // EnemyActivity의 Start 로직
            copiedData = enemyData.GetCopy();
            border = EnemySpawnManager.Instance.spawnDistance;
            playerPos = EnemySpawnManager.Instance.playerPos;
            if (playerPos == null)
            {
                Debug.LogError("Player position not found. Please ensure the player is tagged correctly.");
            }

            // WJEnemy의 Start 로직
            currentHealth = copiedData.maxHealth;
            currentMoveSpeed = copiedData.speed;
            target = playerPos; // playerPos를 target으로 사용

            if (enemyRenderer == null)
            {
                enemyRenderer = GetComponent<Renderer>();
            }

            if (enemyRenderer != null)
            {
                originalColor = enemyRenderer.material.color;
            }

            gameObject.layer = LayerMask.NameToLayer("Enemy");
        }
        
        protected virtual void Update()
        {
            if (playerPos == null) return;

            // WJEnemy의 Update 로직 (타겟 추적 및 공격)
            if (!isDead && target != null)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (distanceToTarget <= detectionRange)
                {
                    if (distanceToTarget <= attackRange)
                    {
                        AttackTarget();
                    }
                    else
                    {
                        // EnemyActivity의 타입별 이동 로직 적용
                        HandleMovementByType();
                    }
                }
                else
                {
                    Idle();
                }
            }
        }
        protected virtual void OnEnable()
        {
            ResetForReuse();
            if (enemyData.type == EnemyData.EnemyType.Elite)
            {
                this.transform.localScale = Vector3.one * 1.5f;
            } else if (enemyData.type == EnemyData.EnemyType.Boss)
            {
                this.transform.localScale = Vector3.one * 2.5f;
            }
            else
            {
                this.transform.localScale = Vector3.one;
            }
        }

        protected virtual void OnDisable()
        {
            // 풀로 반납될 때 혹시 남아있을 수 있는 코루틴/Invoke 정리
            if (slowCoroutine != null)
            {
                StopCoroutine(slowCoroutine);
                slowCoroutine = null;
            }
            StopAllCoroutines();
            CancelInvoke();

            // 색상 원복 (재활성화 때도 다시 설정하지만, 여기서도 안전하게 한 번)
            if (enemyRenderer != null)
            {
                enemyRenderer.material.color = originalColor;
            }
            if(enemyData.type == EnemyData.EnemyType.Normal)
            {
                EnemySpawnManager.Instance.deadCounter++;
            }
        }
        #endregion

        #region Custom Methods
        private void ResetForReuse()
        {
            // 사망/상태 플래그 초기화
            isDead = false;
            isAttacking = false;
            isSlowed = false;

            // 능력치/타이머 초기화
            if (copiedData == null)
            {
                // Start()가 한 번도 안 돈 경우(씬 첫 로드 직후), 방어적으로 복사
                copiedData = enemyData.GetCopy();
            }

            currentHealth = copiedData.maxHealth;
            currentMoveSpeed = copiedData.speed;
            lastAttackTime = -999f;

            // 이동/경계 체크 값 초기화
            blockedCounter = 0;
            outTimeCheck = 0f;

            // 비주얼 초기화
            if (enemyRenderer == null)
            {
                enemyRenderer = GetComponent<Renderer>();
            }
            if (enemyRenderer != null)
            {
                // originalColor가 아직 안 잡힌 경우 방어적으로 저장
                if (originalColor == default)
                {
                    originalColor = enemyRenderer.material.color;
                }
                enemyRenderer.material.color = originalColor;
            }

            // 안전하게 잔여 코루틴/Invoke 제거
            if (slowCoroutine != null)
            {
                StopCoroutine(slowCoroutine);
                slowCoroutine = null;
            }
            StopAllCoroutines();
            CancelInvoke();

            // 레이어/스케일 등도 혹시 변경되었을 수 있으니 정리
            gameObject.layer = LayerMask.NameToLayer("Enemy");
            transform.localScale = Vector3.one;
        }
        // EnemyActivity의 타입별 이동 처리
        protected virtual void HandleMovementByType()
        {
            if (copiedData.type == EnemyData.EnemyType.Normal || copiedData.type == EnemyData.EnemyType.Summoned)
            {
                if (Repulse()) return;
                NormalEnemyMove();
            }
            else if (copiedData.type == EnemyData.EnemyType.Elite)
            {
                CheckCollision();
                EliteEnemyMove();
            }
            // 보스 타입은 별도 처리 (BossActivity에서 오버라이드)
            else if (copiedData.type == EnemyData.EnemyType.Boss)
            {
                BossMove();
            }
        }

        // EnemyActivity의 NormalEnemyMove (수정)
        void NormalEnemyMove()
        {
            if (checkBorderOut())
            {
                outTimeCheck += Time.deltaTime;
                if (outTimeCheck >= outTimeThreshold)
                {
                    EnemyPoolManager.Instance.ReturnEnemy(enemyData, this.gameObject);
                }
            }
            else outTimeCheck = 0;

            MoveToTarget(); // WJEnemy의 이동 로직 사용
            OnOverlapped();
        }

        // EnemyActivity의 EliteEnemyMove (수정)
        void EliteEnemyMove()
        {
            if (checkBorderOut())
            {
                ResetPosition();
            }

            MoveToTarget(); // WJEnemy의 이동 로직 사용
            OnOverlapped();

            if (blockedCounter > 100)
            {
                ResetPosition();
            }
        }

        // WJEnemy의 MoveToTarget (virtual로 변경)
        protected virtual void MoveToTarget()
        {
            if (target == null || isAttacking) return;

            Vector3 direction = (target.position - transform.position).normalized;
            direction = new Vector3(direction.x, 0, direction.z);

            transform.position += direction * currentMoveSpeed * Time.deltaTime;
            transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
        }

        // WJEnemy의 AttackTarget (virtual로 변경)
        protected virtual void AttackTarget()
        {
            if (target == null) return;
            if (Time.time - lastAttackTime < attackCooldown) return;

            isAttacking = true;
            Debug.Log($"{name}이(가) 플레이어를 공격! 데미지: {damage}");
            
            if (target.TryGetComponent<DiceSurvivor.Player.PlayerController>(out var pc))
            {
                pc.TakeDamage(damage);
            }
            transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));


            lastAttackTime = Time.time;
            Invoke(nameof(EndAttack), 0.5f);
        }

        // 보스 전용 이동 메서드 (BossActivity에서 오버라이드 가능)
        protected virtual void BossMove()
        {
            // 기본 보스 이동: 패턴이 비활성화 상태일 때만 플레이어 추적
            MoveToTarget();
        }

        // 보스 전용 접촉 데미지 처리 (BossActivity에서 사용)
        protected virtual void DealContactDamage()
        {
            // 플레이어와의 거리 체크
            if (Vector3.Distance(transform.position, target.position) <= attackRange)
            {
                // 실제 구현 시 플레이어에게 copiedData.damage만큼 피해
                Debug.Log($"보스 접촉 피해! 데미지: {copiedData.damage}");
            }
        }

        // WJEnemy의 EndAttack (virtual로 변경)
        protected virtual void EndAttack()
        {
            isAttacking = false;
        }

        // WJEnemy의 Idle (virtual로 변경)
        protected virtual void Idle()
        {
            // 대기 상태
        }

        // WJEnemy의 TakeDamage (IDamageable 구현)
        public void TakeDamage(float damageAmount)
        {
            if (isDead) return;

            currentHealth -= damageAmount;

            //Debug.Log($"[Enemy] {gameObject.name} 데미지 받음!");
            //Debug.Log($"[Enemy] 받은 데미지: {damageAmount}");
            //Debug.Log($"[Enemy] 남은 체력: {currentHealth}/{copiedData.maxHealth}");
            //Debug.Log($"[Enemy] 체력 비율: {(currentHealth / copiedData.maxHealth * 100):F1}%");

            StartCoroutine(DamageEffect());

            if (currentHealth <= 0)
            {
                Debug.Log($"[Enemy] {gameObject.name} 체력 0 도달 - 사망 처리");
                Die();
            }
        }

        // WJEnemy의 DamageEffect
        private IEnumerator DamageEffect()
        {
            if (enemyRenderer != null)
            {
                enemyRenderer.material.color = Color.red;
                yield return new WaitForSeconds(0.1f);

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

        // WJEnemy의 ApplySlow (IMovement 구현)
        public void ApplySlow(float slowPercent, float duration)
        {
            if (slowCoroutine != null)
            {
                StopCoroutine(slowCoroutine);
            }

            slowCoroutine = StartCoroutine(SlowEffect(slowPercent, duration));
        }

        // WJEnemy의 SlowEffect
        private IEnumerator SlowEffect(float slowPercent, float duration)
        {
            isSlowed = true;
            currentMoveSpeed = copiedData.speed * (1f - slowPercent);

            if (enemyRenderer != null)
            {
                enemyRenderer.material.color = Color.blue;
            }

            yield return new WaitForSeconds(duration);

            isSlowed = false;
            currentMoveSpeed = copiedData.speed;

            if (enemyRenderer != null)
            {
                enemyRenderer.material.color = originalColor;
            }

            Debug.Log($"{name} 감속 해제됨");
        }

        // WJEnemy의 Die (수정: 풀 시스템 연동)
        private void Die()
        {
            if (isDead) return;

            isDead = true;
            Debug.Log($"{name} 사망!");

            GrantRewards();

            StartCoroutine(DeathEffect());
        }

        // WJEnemy의 DeathEffect (수정: 풀로 반환)
        private IEnumerator DeathEffect()
        {
            float elapsedTime = 0f;
            float shrinkDuration = 0.5f;
            Vector3 originalScale = transform.localScale;

            while (elapsedTime < shrinkDuration)
            {
                transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, elapsedTime / shrinkDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            //사망 시 플레이어가 경험치 및 일정 확률로 골드 획득
            //

            // 크기 복구 후 풀로 반환 (Destroy 대신)
            transform.localScale = originalScale;
            EnemyPoolManager.Instance.ReturnEnemy(copiedData, this.gameObject);
        }

        // WJEnemy의 Heal
        public void Heal(float healAmount)
        {
            currentHealth = Mathf.Min(currentHealth + healAmount, copiedData.maxHealth);
            Debug.Log($"{name} 체력 회복: {healAmount}, 현재 체력: {currentHealth}/{copiedData.maxHealth}");
        }

        // WJEnemy의 GetHealthPercentage
        public float GetHealthPercentage()
        {
            return currentHealth / copiedData.maxHealth;
        }

        // EnemyActivity의 checkBorderOut
        protected virtual bool checkBorderOut()
        {
            Vector3 minWorld = playerPos.position - new Vector3(border, 0, border);
            Vector3 maxWorld = playerPos.position + new Vector3(border, 0, border);

            Vector3 pos = transform.position;
            bool xOutBorder = pos.x < minWorld.x - border || pos.x > maxWorld.x + border;
            bool zOutBorder = pos.z < minWorld.z - border || pos.z > maxWorld.z + border;
            return xOutBorder || zOutBorder;
        }

        // EnemyActivity의 ResetPosition
        protected virtual void ResetPosition()
        {
            Vector3 minWorld = playerPos.position - new Vector3(border, 0, border);
            Vector3 maxWorld = playerPos.position + new Vector3(border, 0, border);
            Vector3 pos = transform.position;

            if (pos.x < minWorld.x)
                pos.x = maxWorld.x - border * 0.9f;
            else if (pos.x > maxWorld.x)
                pos.x = minWorld.x + border * 0.9f;
            if (pos.z < minWorld.z)
                pos.z = maxWorld.z - border * 0.9f;
            else if (pos.z > maxWorld.z)
                pos.z = minWorld.z + border * 0.9f;
            transform.position = pos;
        }

        // EnemyActivity의 CheckCollision
        protected virtual void CheckCollision()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, blockRadius);

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Doodad"))
                {
                    blockedCounter++;
                    return;
                }
            }
        }

        // EnemyActivity의 OnOverlapped
        void OnOverlapped()
        {
            Collider[] overlaps = Physics.OverlapSphere(transform.position, blockRadius);
            foreach (var other in overlaps)
            {
                if (other.transform != transform && other.CompareTag("Enemy"))
                {
                    Vector3 pushDir = (transform.position - other.transform.position).normalized;
                    pushDir = new Vector3(pushDir.x, 0, pushDir.z);
                    transform.position += pushDir * Time.deltaTime * copiedData.speed;
                }
                else if (other.transform != transform && other.CompareTag("Doodad"))
                {
                    Vector3 dir = (transform.position - other.transform.position).normalized;
                    dir = new Vector3(dir.x, 0, dir.z);
                    transform.position += dir * Time.deltaTime * copiedData.speed;
                }
            }
        }

        // EnemyActivity의 Repulse
        bool Repulse()
        {
            if (Vector3.Distance(transform.position, playerPos.position) > clusterThreshold) return false;
            Collider[] nearby = Physics.OverlapSphere(transform.position, 0.75f);
            int enemyCount = 0;

            foreach (var collider in nearby)
            {
                if (collider.CompareTag("Enemy") && collider.transform != transform)
                {
                    enemyCount++;
                }
            }

            if (enemyCount >= 3) return true;
            else return false;
        }

        // WJEnemy의 OnDrawGizmosSelected
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }

        private void GrantRewards()
        {
            if (copiedData == null || copiedData.type == EnemyData.EnemyType.Summoned)
                return;

            // -------- 경험치 보상 --------
            float expReward = copiedData.exp;
            if (Random.value > copiedData.expDropRate) expReward = 0f;

            if (expReward > 0f)
            {
                PlayerExperience exp = FindObjectOfType<PlayerExperience>();
                if (exp != null)
                {
                    exp.GainExp(expReward);
                    Debug.Log($"[Reward] EXP +{expReward}");
                }
            }

            // -------- 골드 보상 --------
            int goldReward = copiedData.gold;
            if (Random.value > copiedData.goldDropRate) goldReward = 0;

            if (goldReward > 0)
            {
                // PlayerController 등에 AddCoins() 같은 메서드가 있다면 여기서 호출
                PlayerController player = FindObjectOfType<PlayerController>();
                if (player != null)
                {
                    player.AddCoins(goldReward);
                    Debug.Log($"[Reward] GOLD +{goldReward}");
                }
            }
        }
    
        #endregion
    }
}