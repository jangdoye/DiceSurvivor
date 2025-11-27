using DiceSurvivor.Enemy;
using UnityEngine;

namespace DiceSurvivor.Weapon
{
    /// <summary>
    /// 화염 DoT 구역
    /// </summary>
    public class FireDotZone : MonoBehaviour
    {
        private float radius;
        private float dotDamage;
        private float duration;
        private float endTime;
        private float lastDotTime;

        public void Initialize(float rad, float dot, float dur)
        {
            radius = rad;
            dotDamage = dot;
            duration = dur;
            endTime = Time.time + duration;
            lastDotTime = Time.time;

            // 트리거 콜라이더
            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = radius;
            collider.isTrigger = true;
        }

        void Update()
        {
            if (Time.time >= endTime)
            {
                Destroy(gameObject);
                return;
            }

            // 1초마다 DoT
            if (Time.time - lastDotTime >= 1f)
            {
                ApplyDot();
                lastDotTime = Time.time;
            }
        }

        private void ApplyDot()
        {
            int enemyLayer = LayerMask.GetMask("Enemy");
            Collider[] enemies = Physics.OverlapSphere(transform.position, radius, enemyLayer);

            foreach (var enemyCollider in enemies)
            {
                var enemy = enemyCollider.GetComponent<EnemyFinal>();
                if (enemy != null)
                {
                    enemy.TakeDamage(dotDamage);
                }
            }
        }
    }
}