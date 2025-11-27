using DiceSurvivor.Enemy;
using System.Collections;
using System.Collections.Generic;


using UnityEngine;

namespace DiceSurvivor.Weapon
{
    /// <summary>
    /// 번개 볼트 컴포넌트
    /// </summary>
    public class LightningBolt : MonoBehaviour
    {
        private Vector3 targetPosition;
        private float primaryDamage;
        private float primaryRadius;
        private float secondaryDamage;
        private float secondaryRadius;
        private float secondaryDelay;
        private GameObject effectPrefab;

        private bool hasStruck = false;
        private bool hasExploded = false;

        public bool IsActive { get; private set; }

        /// <summary>
        /// 번개 초기화
        /// </summary>
        public void Initialize(Vector3 target, float damage1, float radius1,
                             float damage2, float radius2, float delay, GameObject effect)
        {
            targetPosition = target;
            primaryDamage = damage1;        //낙뢰 데미지
            primaryRadius = radius1;        //낙뢰 범위
            secondaryDamage = damage2;      //낙뢰후 폭발 데미지
            secondaryRadius = radius2;      //낙뢰후 폭발 범위
            secondaryDelay = delay;
            effectPrefab = effect;
            IsActive = true;

            // 번개 낙하 시작
            StartCoroutine(LightningStrike());
        }

        /// <summary>
        /// 번개 낙하 코루틴
        /// </summary>
        private IEnumerator LightningStrike()
        {
            // 번개 낙하 애니메이션 (0.2초)
            float fallTime = 0.2f;
            float elapsedTime = 0f;
            Vector3 startPos = transform.position;

            while (elapsedTime < fallTime)
            {
                transform.position = Vector3.Lerp(startPos, targetPosition, elapsedTime / fallTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPosition;

            // 1차 데미지 (projectileSize 범위)
            ApplyPrimaryDamage();

            // 지연 후 2차 데미지
            yield return new WaitForSeconds(secondaryDelay);

            // 2차 폭발 데미지 (explosionRadius 범위)
            ApplySecondaryDamage();

            // 번개 제거
            yield return new WaitForSeconds(0.5f);
            IsActive = false;
            Destroy(gameObject);
        }

        /// <summary>
        /// 1차 데미지 적용 (projectileSize 범위)
        /// </summary>
        private void ApplyPrimaryDamage()
        {
            if (hasStruck) return;
            hasStruck = true;

            //Debug.Log($"[LightningBolt] 1차 데미지 적용 - 위치: {transform.position}, 범위: {primaryRadius}, 데미지: {primaryDamage}");

            // projectileSize 범위 내 적에게 데미지
            int enemyLayer = LayerMask.GetMask("Enemy");
            Collider[] enemies = Physics.OverlapSphere(transform.position, primaryRadius, enemyLayer);

            foreach (var enemyCollider in enemies)
            {
                var enemy = enemyCollider.GetComponent<EnemyFinal>();
                if (enemy != null)
                {
                    enemy.TakeDamage(primaryDamage);
                    //Debug.Log($"[LightningBolt] 1차 데미지 {primaryDamage} → {enemyCollider.name}");
                }
            }

            // 1차 이펙트
            //CreateEffect(primaryRadius, Color.yellow);
        }

        /// <summary>
        /// 2차 폭발 데미지 적용 (explosionRadius 범위)
        /// </summary>
        private void ApplySecondaryDamage()
        {
            if (hasExploded) return;
            hasExploded = true;

            //Debug.Log($"[LightningBolt] 2차 폭발 데미지 - 위치: {transform.position}, 범위: {secondaryRadius}, 데미지: {secondaryDamage}");

            // explosionRadius 범위 내 적에게 데미지
            int enemyLayer = LayerMask.GetMask("Enemy");
            Collider[] enemies = Physics.OverlapSphere(transform.position, secondaryRadius, enemyLayer);

            foreach (var enemyCollider in enemies)
            {
                var enemy = enemyCollider.GetComponent<EnemyFinal>();
                if (enemy != null)
                {
                    enemy.TakeDamage(secondaryDamage);
                    //Debug.Log($"[LightningBolt] 2차 폭발 데미지 {secondaryDamage} → {enemyCollider.name}");
                }
            }

            // 2차 이펙트
            //CreateEffect(secondaryRadius, new Color(0.5f, 0.8f, 1f));
        }

        /// <summary>
        /// 이펙트 생성
        /// </summary>
        private void CreateEffect(float radius, Color color)
        {
            if (effectPrefab != null)
            {
                GameObject effect = Instantiate(effectPrefab, transform.position, Quaternion.identity);
                effect.transform.localScale = Vector3.one * radius;
                Destroy(effect);
            }
        }

        void OnDrawGizmosSelected()
        {
            // 1차 범위 (노란색)
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, primaryRadius);

            // 2차 범위 (파란색)
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, secondaryRadius);
        }
    }
}