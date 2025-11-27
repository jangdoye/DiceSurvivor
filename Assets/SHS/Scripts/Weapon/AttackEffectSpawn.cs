using DiceSurvivor.Manager;
using DiceSurvivor.Type;
using DiceSurvivor.Utility;
using DiceSurvivor.Weapon;
using System.Collections.Generic;
using UnityEngine;

namespace DiceSurvivor.Weapon
{
    public class AttackEffectSpawn : WeaponAttackBase
    {
        #region Variables
        // 현재 생성된 공격 이펙트의 인스턴스
        private ParticleSystem effect;

        //Attack Handler
        public GameObject weaponHandler;

        // 생성할 공격 이펙트의 프리팹
        public ParticleSystem attackEffect;

        // 이펙트를 생성할 위치
        public Transform effectSpawnTransform;

        // 채찍 궤적에 사용할 WayPoint 프리팹
        public GameObject whipWayPointPrefab;

        // 기본 이펙트 삭제 시간 (무기에 따라 변경 가능)
        [SerializeField] private float defaultDestroyTime = 5f;

        // 현재 장착 중인 무기 타입
        [SerializeField] private MeleeWeaponType currentWeapon;
        #endregion

        #region Unity Event Method
        protected override void Awake()
        {
            base.Awake();
        }

        // 이펙트 위치 보정 (무기 소켓에 맞춰 위치 조정)
        private void LateUpdate()
        {
            if (effect == null) return; // 이펙트가 없으면 종료

            switch (currentWeapon)
            {
                case MeleeWeaponType.Hammer:
                    // 해머는 위치 고정 (보정 없음)
                    break;

                case MeleeWeaponType.GreatSword:
                    // 대검 전용 위치 보정 (필요 시 구현)
                    break;

                case MeleeWeaponType.Scythe:
                    // 낫 전용 위치 보정 (필요 시 구현)
                    break;

                case MeleeWeaponType.Whip:
                    // 채찍 전용 위치 보정 (필요 시 구현)
                    break;

                case MeleeWeaponType.Staff:
                case MeleeWeaponType.Spear:
                    //이펙트 위치를 맞춤
                    effect.transform.position = effectSpawnTransform.transform.position;
                    break;
            }
        }
        #endregion

        #region Custom Method
        protected override void Attack()
        {
            //사용 안함
        }
        // 공격 이펙트를 생성하는 함수 (애니메이션 이벤트에서 호출 가능)
        public void SpawnAttackEffect()
        {
            if (currentWeapon == MeleeWeaponType.Hammer)
            {
                effect = Instantiate(attackEffect, effectSpawnTransform.position, effectSpawnTransform.rotation); // 이펙트 생성
            }
            else
            {
                effect = Instantiate(attackEffect, effectSpawnTransform.position, effectSpawnTransform.rotation, effectSpawnTransform.transform); // 이펙트 생성
            }
                

            float destroyTime = (currentWeapon == MeleeWeaponType.Staff || currentWeapon == MeleeWeaponType.Spear)
                ? 0.5f
                : defaultDestroyTime; // 무기 타입에 따라 삭제 시간 설정

            switch (currentWeapon)
            {
                case MeleeWeaponType.Scythe:
                    ParticleSystem scytheParticle = effect.GetComponent<ParticleSystem>();
                    var scytheMain = scytheParticle.main;
                    var currentSize = scytheMain.startSize.constant;

                    scytheMain.startSize = new ParticleSystem.MinMaxCurve(currentSize + Weapon.range);
                    break;

                case MeleeWeaponType.GreatSword:
                    ParticleSystem greatswordParticle = effect.GetComponent<ParticleSystem>();
                    var greatswordMain = greatswordParticle.main;

                    greatswordMain.startSize = Weapon.range;
                    break;

                case MeleeWeaponType.Hammer:
                    WeaponController weaponController = this.GetComponentInParent<WeaponController>(); // 무기 정보 가져오기                    

                    WeaponSplashAttack aoe = effect.GetComponent<WeaponSplashAttack>(); // 해머 전용 이펙트 컴포넌트
                    aoe.Weapon = weaponController.currentWeaponStats; // 무기 스탯 전달

                    effect.transform.localScale = new Vector3(Weapon.explosionRadius * 0.5f, Weapon.explosionRadius * 0.5f, Weapon.explosionRadius * 0.5f);
                    
                    break;

                case MeleeWeaponType.Staff:
                case MeleeWeaponType.Spear:
                    effect.GetComponentInChildren<rotation>().topEnd = effectSpawnTransform; // 회전 이펙트의 끝 위치 설정
                    break;

                case MeleeWeaponType.Whip:
                    effect.GetComponentInChildren<rotation>().topEnd = effectSpawnTransform; // 회전 이펙트의 끝 위치 설정
                    particleAttractorMove particleRange = effect.GetComponentInChildren<particleAttractorMove>();
                    particleRange.speed = Weapon.range * 3;

                    // 채찍 궤적 생성기 설정
                    WayPointsGenerator generator = effect.gameObject.AddComponent<WayPointsGenerator>(); // 궤적 생성기 추가
                    generator.center = this.transform;               // 궤적 중심 설정
                    generator.wayPointPrefab = whipWayPointPrefab;     // 궤적 프리팹 설정
                    generator.count = 10;                              // 궤적 점 개수
                    generator.radius = 0.5f;

                    List<Transform> wayPoints = generator.GenerateWayPoints(); // 궤적 생성

                    WhipAttack whipAttack = weaponHandler.GetComponent<WhipAttack>(); // 채찍 공격 스크립트 가져오기
                    whipAttack.SetWayPoints(wayPoints);                      // 궤적 전달
                    break;
            }

            Destroy(effect.gameObject, destroyTime); // 일정 시간 후 이펙트 삭제
        }
        #endregion
    }
}
