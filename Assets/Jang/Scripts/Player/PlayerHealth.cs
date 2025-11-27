using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DiceSurvivor.Audio;

namespace DiceSurvivor.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("체력")]
        public float maxHealth = 100f;
        [SerializeField] private float currentHealth;

        [Header("무적/피격 연출")]
        [Tooltip("피해 후 무적 유지 시간(초)")]
        public float invincibleTime = 1f;
        [Tooltip("깜빡임 간격(초)")]
        public float blinkInterval = 0.1f;
        [Tooltip("타임스케일 영향을 받지 않게 깜빡임")]
        public bool useUnscaledTimeForBlink = false;

        [Header("UI")]
        public HPBar hpBarUI; // MaxValue, CurrentValue를 가진 HP UI 래퍼

        [Header("사망 처리")]
        [Tooltip("사망 시 일시정지할지 여부")]
        public bool pauseOnDeath = true;


        [SerializeField] private Animator animator;              // <- 플레이어의 Animator
        [SerializeField] private string deathTrigger = "Die";    // <- 트리거 이름
        [SerializeField] private float deathSequenceDelay = 2f;  // <- 트리거 후 UI/정지까지 대기 시간

        // 상태
        public bool IsInvincible { get; private set; }
        public bool IsDead { get; private set; }

        // 내부
        private readonly List<Renderer> renderers = new();
        private Coroutine invincibleRoutine;
        private Coroutine blinkRoutine;

        private void Awake()
        {
            currentHealth = maxHealth;
            CollectAllRenderers();
        }

        private void Start()
        {
            if (hpBarUI != null)
            {
                hpBarUI.MaxValue = maxHealth;
                hpBarUI.CurrentValue = currentHealth;
            }

        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.T))
            {
                TakeDamage(20);
            }
        }
        private void OnDisable()
        {
            StopBlinkImmediate();
            SetVisible(true);
            IsInvincible = false;
        }

        // === 외부에서 호출 ===
        public void TakeDamage(float amount, bool triggerInvincible = true)
        {
            if (IsDead || amount <= 0f) return;
            if (IsInvincible) return;

            // 플레이어가 공격 받았을 때 Sfx 실행
            SfxManager.Instance.PlaySfx(SfxType.PlayerAttacked);

            currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
            UpdateHPUI();

            if (currentHealth <= 0f)
            {
                Die();
                return;
            }

            if (triggerInvincible && invincibleTime > 0f)
            {
                if (invincibleRoutine != null) StopCoroutine(invincibleRoutine);
                invincibleRoutine = StartCoroutine(InvincibleCoroutine(invincibleTime));
            }
            else
            {
                // 무적은 안 쓰고 깜빡임만 줄 때
                StartBlink(invincibleTime > 0f ? invincibleTime : 0.25f);
            }
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f) return;

            currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
            UpdateHPUI();
        }

        public void DrainSilently(float amount)
        {
            if (IsDead || amount <= 0f) return;

            currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
            UpdateHPUI();

            if (currentHealth <= 0f) Die();
        }

        // === 무적 & 깜빡임 ===
        private IEnumerator InvincibleCoroutine(float duration)
        {
            IsInvincible = true;
            StartBlink(duration);

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                yield return null;
            }

            IsInvincible = false;
            invincibleRoutine = null;
        }

        public void StartBlink(float duration)
        {
            if (duration <= 0f) return;
            if (blinkRoutine != null) StopCoroutine(blinkRoutine);
            blinkRoutine = StartCoroutine(BlinkCoroutine(duration));
        }

        public void StopBlinkImmediate()
        {
            if (blinkRoutine != null) StopCoroutine(blinkRoutine);
            blinkRoutine = null;
            SetVisible(true);
        }

        private IEnumerator BlinkCoroutine(float duration)
        {
            float elapsed = 0f;
            bool visible = true;
            SetVisible(true);

            while (elapsed < duration)
            {
                visible = !visible;
                SetVisible(visible);

                if (useUnscaledTimeForBlink) yield return new WaitForSecondsRealtime(blinkInterval);
                else yield return new WaitForSeconds(blinkInterval);

                elapsed += blinkInterval;
            }

            SetVisible(true);
            blinkRoutine = null;
        }

        // === 렌더러/HP UI/사망 ===
        private void CollectAllRenderers()
        {
            renderers.Clear();
            var all = GetComponentsInChildren<Renderer>(true); // 착용 아이템까지 포함
            renderers.AddRange(all);
        }

        private void SetVisible(bool show)
        {
            if (renderers.Count == 0) CollectAllRenderers();
            foreach (var r in renderers)
            {
                if (r == null) continue;
                r.enabled = show;
            }
        }

        private void UpdateHPUI()
        {
            if (hpBarUI != null)
                hpBarUI.CurrentValue = currentHealth;
        }

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;

            StopBlinkImmediate();
            SetVisible(true);
            //Debug.Log("[PlayerHealth] Player Died");

            // 플레이어가 죽을 때 SFX
            SfxManager.Instance.PlaySfx(SfxType.PlayerDeath);

            if (animator != null && !string.IsNullOrEmpty(deathTrigger))
                animator.SetTrigger(deathTrigger);

            // 자식 오브젝트 전부 정지
            foreach (Transform child in transform)
            {
                if (child.CompareTag("Customization"))
                    continue; // 커스터마이징 파츠는 유지

                child.gameObject.SetActive(false);
            }

            // 플레이어 본체의 움직임도 막기
            GetComponent<PlayerController>().enabled = false;
            GetComponent<CharacterController>().enabled = false;

            // 이후 흐름 코루틴
            StartCoroutine(DeathFlow());
        }

        private IEnumerator DeathFlow()
        {
            // 애니메이션이 재생될 시간을 기다림
            // (정확히 맞추고 싶으면 Animation Event로 이 코루틴의 다음 단계 호출)
            float wait = Mathf.Max(0f, deathSequenceDelay);
            if (wait > 0f) yield return new WaitForSeconds(wait);

            // 게임오버 UI
            GameOverUI gameOver = FindObjectOfType<GameOverUI>();
            if (gameOver != null) gameOver.ShowGameOverUI();

            // 스테이지 실패 SFX
            SfxManager.Instance.PlaySfx(SfxType.Defeat);

            if (pauseOnDeath)
                Time.timeScale = 0f; // 애니메이션 끝난 뒤에 멈춤
        }
    }
}