using DiceSurvivor.Weapon;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace DiceSurvivor.Weapon
{
    public class WhipAttack : MeleeWeaponBase
    {
        #region Variables
        // 채찍 궤적을 따라 생성된 웨이포인트 리스트
        public List<Transform> wayPoints = new List<Transform>();
        #endregion

        #region Properties
        // 현재는 사용되지 않지만, 필요 시 속성 정의 가능
        #endregion

        #region Unity Event Methods
        // 무기 컨트롤러 초기화
        protected override void Awake()
        {
            base.Awake(); // 부모 클래스 초기화
        }

        protected override void Update()
        {
            base.Update();
        }
        #endregion

        #region Custom Methods
        /// <summary>
        /// 외부에서 전달받은 웨이포인트 리스트를 설정하고,
        /// 각 웨이포인트에서 Raycast를 통해 적을 감지하여 피해 처리
        /// </summary>
        /// <param name="points">웨이포인트 리스트</param>
        public void SetWayPoints(List<Transform> points)
        {
            wayPoints = points;

            // ✅ 이미 데미지를 준 적을 추적할 HashSet
            HashSet<GameObject> damagedEnemies = new HashSet<GameObject>();

            if (wayPoints != null && wayPoints.Count > 0)
            {
                foreach (Transform wp in wayPoints)
                {
                    Vector3 direction = wp.forward;
                    Ray ray = new Ray(wp.position, direction);

                    Debug.DrawRay(wp.position, direction * Weapon.range, Color.red, 1f);

                    if (Physics.Raycast(ray, out RaycastHit hit, Weapon.range))
                    {
                        if (hit.transform.CompareTag("Enemy"))
                        {
                            GameObject enemy = hit.transform.gameObject;

                            // ✅ 이미 데미지를 준 적이면 건너뜀
                            if (!damagedEnemies.Contains(enemy))
                            {
                                ApplyDamage(enemy, Weapon.damage);
                                damagedEnemies.Add(enemy); // ✅ 데미지 준 적으로 등록
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
