using System.Collections;
using UnityEngine;

namespace DiceSurvivor.Weapon
{
    /// <summary>
    /// 창 또는 스태프 무기의 공격 동작을 처리하는 클래스입니다.
    /// 무기를 일정 거리만큼 앞으로 찌른 후 되돌리는 애니메이션을 실행하며,
    /// 적과 충돌 시 데미지를 출력합니다.
    /// </summary>
    public class SpearStaffAttack : MeleeWeaponBase
    {
        #region Variables

        // 공격에 사용되는 무기 오브젝트 (창 또는 스태프)
        public GameObject spear;

        #endregion

        #region Properties
        // 현재는 사용되지 않지만, 향후 무기 속성 등을 정의할 수 있음
        #endregion

        #region Unity Event Methods

        // 초기화 시 호출되며, WeaponAttackBase의 Awake 로직을 실행
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Update()
        {
            base.Update();
        }

        // 무기 콜라이더가 적과 충돌했을 때 호출됨
        // 적 태그가 있는 오브젝트와 충돌 시 데미지 로그 출력
        protected override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);
        }

        #endregion

        #region Custom Methods

        // 공격 실행: 무기를 앞으로 찌르는 애니메이션을 시작
        public void ExecuteAttack()
        {
            StartCoroutine(SetPositionZCoroutine());
        }

        // 무기를 일정 거리만큼 앞으로 이동시킨 후 원래 위치로 되돌리는 코루틴
        // 찌르기 → 잠깐 멈춤 → 되돌리기 순서로 애니메이션 처리
        private IEnumerator SetPositionZCoroutine()
        {
            Vector3 start = spear.transform.localPosition; // 시작 위치 저장
            Vector3 end = start + new Vector3(0, 0, Weapon.range); // 찌르기 목표 위치 계산
            float duration = 0.2f; // 전체 애니메이션 시간

            // 앞으로 찌르기 (절반 시간 사용)
            float elapsed = 0f;
            while (elapsed < duration * 0.5f)
            {
                spear.transform.localPosition = Vector3.Lerp(start, end, elapsed / (duration * 0.5f));
                elapsed += Time.deltaTime;
                yield return null;
            }

            spear.transform.localPosition = end;

            // 찌른 상태에서 잠깐 멈춤 (타격 타이밍)
            yield return new WaitForSeconds(0.05f);

            // 원래 위치로 되돌리기 (나머지 절반 시간 사용)
            elapsed = 0f;
            while (elapsed < duration * 0.5f)
            {
                spear.transform.localPosition = Vector3.Lerp(end, start, elapsed / (duration * 0.5f));
                elapsed += Time.deltaTime;
                yield return null;
            }

            spear.transform.localPosition = start; // 위치 초기화
        }

        #endregion
    }
}
