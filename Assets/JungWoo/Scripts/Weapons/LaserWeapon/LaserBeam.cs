using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DiceSurvivor.Enemy;

namespace DiceSurvivor.Weapon
{
    /// <summary>
    /// 레이저 투사체 컴포넌트 - 순간 발사 버전
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class LaserBeam : MonoBehaviour
    {
        private LaserWeapon laserWeapon;
        private LineRenderer lineRenderer;
        private Transform playerTransform;
        private Vector3 targetDirection;         // 발사 시점의 방향 (고정)
        private float damage;
        private float maxRange;
        private GameObject hitEffectPrefab;
        private float initialWidth;

        private float flashDuration = 0.1f;      // 레이저 지속 시간 (0.1초)
        private HashSet<EnemyFinal> damagedEnemies; // 이번 발사에서 데미지 받은 적들

        public bool IsActive { get; private set; }

        public LaserWeapon LA { get { return laserWeapon; } set { laserWeapon = value; } }

        void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.enabled = false;
            damagedEnemies = new HashSet<EnemyFinal>();
        }

        public void Initialize(Transform player, Vector3 direction, float dmg, float range,
            GameObject hitPrefab, float laserWidth, LaserWeapon la)
        {
            playerTransform = player;
            targetDirection = direction.normalized;
            damage = dmg;
            maxRange = range;
            hitEffectPrefab = hitPrefab;
            initialWidth = laserWidth;
            LA = la;

            // LineRenderer 설정
            lineRenderer.startWidth = initialWidth;
            lineRenderer.endWidth = initialWidth;

            IsActive = true;
            lineRenderer.enabled = true;
            damagedEnemies.Clear();

            // 레이저 위치 설정 및 즉시 데미지 적용
            SetLaserPosition();
            ApplyInstantDamage();

            // 0.1초 후 자동 삭제
            StartCoroutine(FlashAndDestroy());
        }

        private void SetLaserPosition()
        {
            // 시작점: 플레이어 위치
            Vector3 startPos = playerTransform.position;

            // 끝점: 플레이어 위치 + 방향 * 최대 거리
            Vector3 endPos = startPos + targetDirection * maxRange;

            endPos.y = startPos.y;

            // 레이저 라인 설정
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
        }

        private void ApplyInstantDamage()
        {
            if (playerTransform == null) return;

            Vector3 startPos = playerTransform.position;

            // 레이저 경로상의 모든 적 검출 (일직선)
            RaycastHit[] hits = Physics.RaycastAll(startPos, targetDirection, maxRange, LayerMask.GetMask("Enemy"));

            if (hits.Length > 0)
            {
                // 거리순 정렬
                System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

                // 관통: 레이저에 닿은 모든 적에게 즉시 데미지
                foreach (var hit in hits)
                {
                    var enemy = hit.collider.GetComponent<EnemyFinal>();
                    if (enemy != null && !damagedEnemies.Contains(enemy))
                    {
                        laserWeapon.ApplyDamage(enemy.gameObject, damage);
                        damagedEnemies.Add(enemy);
                        CreateHitEffect(hit.point);

                        //Debug.Log($"[LaserBeam] 즉시 데미지: {enemy.name} (거리: {hit.distance:F1})");
                    }
                }
            }
        }

        private void CreateHitEffect(Vector3 position)
        {
            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);
                Destroy(effect, 1.0f);
            }
        }

        private IEnumerator FlashAndDestroy()
        {
            // 0.1초 동안 표시
            yield return new WaitForSeconds(flashDuration);

            // 레이저 비활성화 및 삭제
            IsActive = false;
            lineRenderer.enabled = false;
            Destroy(gameObject);
        }

        void OnDestroy()
        {
            damagedEnemies?.Clear();
        }
    }
}