using DiceSurvivor.Enemy;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace DiceSurvivor.Weapon
{
    /// <summary>
    /// 소행성 공전 컴포넌트
    /// </summary>
    public class AsteroidOrbit : MonoBehaviour
    {
        private Transform centerPoint;         // 공전 중심 (플레이어)
        private float orbitRadius;             // 공전 반경
        private float asteroidSize;            // 소행성 크기
        private float damage;                  // 데미지
        private float orbitSpeed;              // 공전 속도
        private float currentAngle;            // 현재 각도
        private float orbitHeight;             // 공전 높이

        private HashSet<GameObject> hitEnemies; // 이미 타격한 적 목록

        public bool IsActive { get; private set; }

        /// <summary>
        /// 소행성 초기화
        /// </summary>
        public void Initialize(Transform center, float radius, float size, float dmg,
                             float speed, float startAngle, float height)
        {
            centerPoint = center;
            orbitRadius = radius;
            asteroidSize = size;
            damage = dmg;
            orbitSpeed = speed * 120f; // 속도 조정 (도/초)
            currentAngle = startAngle;
            orbitHeight = height;

            hitEnemies = new HashSet<GameObject>();
            IsActive = true;

            // 크기 설정
            transform.localScale = Vector3.one * asteroidSize;

            // Collider 크기 조정
            SphereCollider collider = GetComponent<SphereCollider>();
            

            // 초기 위치 설정
            UpdatePosition();

            //Debug.Log($"[AsteroidOrbit] 초기화 완료 - 위치: {transform.position}");
        }

        /// <summary>
        /// 공전 업데이트 (AsteroidWeapon에서 호출)
        /// </summary>
        public void UpdateOrbit()
        {
            if (!IsActive || centerPoint == null) return;

            // 공전
            currentAngle += orbitSpeed * Time.deltaTime;
            if (currentAngle >= 360f)
            {
                currentAngle -= 360f;
                // 한 바퀴 돌면 타격 목록 초기화
                hitEnemies.Clear();
            }

            UpdatePosition();
        }

        /// <summary>
        /// 위치 업데이트
        /// </summary>
        private void UpdatePosition()
        {
            if (centerPoint == null) return;

            // 원형 공전 위치 계산
            float radian = currentAngle * Mathf.Deg2Rad;
            float x = centerPoint.position.x + Mathf.Cos(radian) * orbitRadius;
            float z = centerPoint.position.z + Mathf.Sin(radian) * orbitRadius;
            float y = centerPoint.position.y + orbitHeight;

            transform.position = new Vector3(x, y, z);
        }

        /// <summary>
        /// 적과 충돌 시
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (!IsActive) return;

            // Enemy 레이어 체크
            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                // 이미 타격한 적인지 체크
                if (!hitEnemies.Contains(other.gameObject))
                {
                    // 데미지 적용
                    var enemy = other.GetComponent<EnemyFinal>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(damage);
                        hitEnemies.Add(other.gameObject);

                        //Debug.Log($"[AsteroidOrbit] {other.name}에게 데미지 {damage} 적용");

                    }
                }
            }
        }

        /// <summary>
        /// 소행성 비활성화
        /// </summary>
        public void Deactivate()
        {
            
            if (!IsActive) return;
            IsActive = false;

            // 페이드 아웃 효과
            StartCoroutine(FadeOutAndDestroy());
        }

        /// <summary>
        /// 페이드 아웃 후 제거
        /// </summary>
        private IEnumerator FadeOutAndDestroy()
        {
            float fadeTime = 0.5f;
            float elapsedTime = 0f;
            Vector3 originalScale = transform.localScale;

            while (elapsedTime < fadeTime)
            {
                if (this != null && transform != null)
                {
                    transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, elapsedTime / fadeTime);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                else
                {
                    break;
                }
            }

            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }

        void OnDrawGizmosSelected()
        {
            if (centerPoint != null)
            {
                // 공전 궤도 표시
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(centerPoint.position, orbitRadius);

                // 현재 위치에서 중심까지 선
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, centerPoint.position);
            }
        }
    }
}
