/*using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DiceSurvivor.Battle;
using DiceSurvivor.WeaponDataSystem;
using static DiceSurvivor.WeaponDataSystem.UnifiedWeaponDataManager;

namespace DiceSurvivor.Weapon
{
    public class Asteroid : MonoBehaviour
    {
        #region Variables
        //참조
        [Header("무기 설정")]
        public string weaponName = "Asteroid";
        public int weaponLevel = 1;

        [Header("Meteor Stats")]
        public float damage = 10f;
        public float radius = 0.5f;
        public float cooldown = 8f;
        public float projectileSpeed = 1f; // 1초당 회전 수
        public int projectileCount = 2;
        public float duration = 3f;

        [Header("Orbit Settings")]
        public Transform target; // 따라다닐 대상
        public float orbitDistance = 3f; // 궤도 거리

        [Header("Projectile Settings")]
        public GameObject meteorPrefab; // 운석 프리팹
        public LayerMask enemyLayer = 1; // 적 레이어


        private WeaponStats weaponData;
        private List<GameObject> activeMeteors = new List<GameObject>();
        private bool isActive = false;

        #endregion

        #region Unity Event Method
        void Start()
        {
            // 타겟이 없으면 플레이어를 찾아봄
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    target = player.transform;
            }
            LoadWeaponData();

            // 무한 반복 시작
            StartCoroutine(MeteorCycle());
        }
        void OnDestroy()
        {
            DestroyMeteors();
        }

        // 사거리 시각화 (Scene 뷰에서만 보임)
        void OnDrawGizmosSelected()
        {
            if (target != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(target.position, orbitDistance);

                if (isActive)
                {
                    Gizmos.color = Color.red;
                    foreach (GameObject meteor in activeMeteors)
                    {
                        if (meteor != null)
                            Gizmos.DrawWireSphere(meteor.transform.position, weaponData.radius);
                    }
                }
            }
        }
        #endregion

        #region Custom Method
        IEnumerator MeteorCycle()
        {
            while (target != null)
            {
                // 운석 활성화
                yield return StartCoroutine(ActivateMeteors());

                // 쿨다운 대기
                yield return new WaitForSeconds(weaponData.cooldown);
            }
        }

        IEnumerator ActivateMeteors()
        {
            isActive = true;
            CreateMeteors();

            // duration 동안 회전
            float elapsedTime = 0f;

            while (elapsedTime < weaponData.duration && target != null)
            {
                float deltaTime = Time.deltaTime;
                elapsedTime += deltaTime;

                // 1초에 projectileSpeed 바퀴 회전 (360도 * projectileSpeed * deltaTime)
                float rotationSpeed = 360f * weaponData.projectileSpeed;
                float angleIncrement = rotationSpeed * deltaTime;

                // 모든 운석 회전
                RotateMeteors(angleIncrement);

                // 공격 체크 (매 프레임마다)
                CheckForAttack();

                yield return null;
            }

            // 운석들 제거
            DestroyMeteors();
            isActive = false;
        }

        void CreateMeteors()
        {
            if (meteorPrefab == null || target == null) return;

            // 기존 운석들 정리
            DestroyMeteors();

            // projectileCount 수만큼 운석 생성
            float angleStep = 360f / weaponData.projectileCount;

            for (int i = 0; i < weaponData.projectileCount; i++)
            {
                float angle = i * angleStep;
                float radian = angle * Mathf.Deg2Rad;

                Vector3 position = target.position + new Vector3(
                    Mathf.Cos(radian) * orbitDistance,
                    0f,
                    Mathf.Sin(radian) * orbitDistance
                );

                GameObject meteor = Instantiate(meteorPrefab, position, Quaternion.identity);

                // 운석에 MeteorInstance 컴포넌트 추가
                MeteorInstance instance = meteor.GetComponent<MeteorInstance>();
                if (instance == null)
                    instance = meteor.AddComponent<MeteorInstance>();

                instance.Initialize(this, weaponData.damage, i);
                activeMeteors.Add(meteor);
            }
        }

        void RotateMeteors(float angleIncrement)
        {
            if (target == null) return;

            float angleStep = 360f / weaponData.projectileCount;

            for (int i = 0; i < activeMeteors.Count; i++)
            {
                if (activeMeteors[i] != null)
                {
                    MeteorInstance instance = activeMeteors[i].GetComponent<MeteorInstance>();
                    if (instance != null)
                    {
                        instance.currentAngle += angleIncrement;
                        if (instance.currentAngle >= 360f)
                            instance.currentAngle -= 360f;

                        float radian = instance.currentAngle * Mathf.Deg2Rad;
                        Vector3 newPosition = target.position + new Vector3(
                            Mathf.Cos(radian) * orbitDistance,
                            0f,
                            Mathf.Sin(radian) * orbitDistance
                        );

                        activeMeteors[i].transform.position = newPosition;

                        // 운석이 회전 방향을 바라보도록 설정
                        Vector3 direction = (newPosition - target.position).normalized;
                        activeMeteors[i].transform.rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.up, direction));
                    }
                }
            }
        }

        void CheckForAttack()
        {
            foreach (GameObject meteor in activeMeteors)
            {
                if (meteor != null)
                {
                    // 각 운석 주변의 적들 체크
                    Collider[] enemies = Physics.OverlapSphere(meteor.transform.position, weaponData.radius, enemyLayer);

                    foreach (Collider enemy in enemies)
                    {
                        // 데미지 적용
                        BattleBase damageable = enemy.GetComponent<BattleBase>();
                        if (damageable != null)
                        {
                            damageable.TakeDamage(weaponData.damage * Time.deltaTime); // 초당 데미지
                        }
                    }
                }
            }
        }

        void DestroyMeteors()
        {
            foreach (GameObject meteor in activeMeteors)
            {
                if (meteor != null)
                    Destroy(meteor);
            }
            activeMeteors.Clear();
        }

        void LoadWeaponData()
        {

            weaponData = DataTableManager.Instance.GetMeleeWeapon(weaponName, weaponLevel);
            if (weaponData == null)
            {
                Debug.LogError($"무기 '{weaponName}' 레벨 {weaponLevel}의 데이터를 찾을 수 없습니다.");
                return;
            }
        }
    }
    #endregion

    // 개별 운석 인스턴스 클래스
    public class MeteorInstance : MonoBehaviour
    {
        public Asteroid parent;
        public float damage;
        public int index;
        public float currentAngle;

        public void Initialize(Asteroid parentScript, float dmg, int idx)
        {
            parent = parentScript;
            damage = dmg;
            index = idx;
            currentAngle = idx * (360f / parentScript.projectileCount);
        }

        void OnTriggerEnter(Collider other)
        {
            // 적과 충돌 시 추가 효과 (선택사항)
            if (((1 << other.gameObject.layer) & parent.enemyLayer) != 0)
            {
                BattleBase damageable = other.GetComponent<BattleBase>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                }
            }
        }
    }

    
}*/