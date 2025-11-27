using DiceSurvivor.Audio;
using DiceSurvivor.Enemy;
using UnityEngine;

namespace DiceSurvivor.Weapon 
{
    /// <summary>
    /// 독 플라스크 투사체 컴포넌트
    /// </summary>
    public class PoisonFlaskProjectile : MonoBehaviour
    {
        private PoisonFlaskWeapon flaskWeapon = null;
        private Vector3 startPosition;         // 시작 위치
        private Vector3 targetPosition;        // 목표 위치
        private float arcHeight;               // 포물선 높이
        private float throwDuration;           // 투척 시간
        private float explosionDamage;         // 폭발 데미지
        private float explosionRadius;         // 폭발 범위
        private float dotDamage;               // DoT 데미지
        private float dotDuration;             // DoT 지속시간
        private float projectileSize;          // 크기
        private GameObject poisonGasPrefab;    // 독가스 프리팹
        private Sound sound;                    //소리

        private float elapsedTime;             // 경과 시간
        private bool hasLanded;                // 착지 여부

        public bool IsActive { get; private set; }

        public PoisonFlaskWeapon PF { get { return flaskWeapon; } set { flaskWeapon = value; } }

        private void Start()
        {

        }
        /// <summary>
        /// 플라스크 초기화
        /// </summary>
        public void Initialize(Vector3 start, Vector3 target, float arc, float duration,
                             float explDamage, float explRadius, float dot, float dotDur, float size, GameObject gasPrefab,
                             PoisonFlaskWeapon pf)
        {
            startPosition = start;
            targetPosition = target;
            targetPosition.y = 0f; // 바닥 높이로 조정
            arcHeight = arc;
            throwDuration = duration;
            explosionDamage = explDamage;
            explosionRadius = explRadius;
            dotDamage = dot;
            dotDuration = dotDur;
            projectileSize = size;
            poisonGasPrefab = gasPrefab;
            PF = pf;

            elapsedTime = 0f;
            hasLanded = false;
            IsActive = true;

            // 크기 설정
            transform.localScale = Vector3.one * projectileSize;

            //Debug.Log($"[PoisonFlaskProjectile] 초기화 - 폭발: {explosionDamage}, DoT: {dotDamage}");
        }

        void Update()
        {
            if (!IsActive || hasLanded) return;

            // 포물선 이동
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / throwDuration;

            if (progress >= 1f)
            {
                // 착지
                transform.position = targetPosition;
                Land();
            }
            else
            {
                // 포물선 계산
                Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, progress);
                currentPos.y += arcHeight * 4f * progress * (1f - progress); // 포물선 높이
                transform.position = currentPos;

                // 회전
                transform.Rotate(Vector3.right * 360f * Time.deltaTime);
            }
        }

        /// <summary>
        /// 바닥 충돌 감지
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (!IsActive || hasLanded) return;

            // Ground 레이어 체크
            if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                Land();
            }
        }

        /// <summary>
        /// 착지 처리
        /// </summary>
        private void Land()
        {
            if (hasLanded) return;
            hasLanded = true;
            
            //Debug.Log($"[PoisonFlaskProjectile] 착지! 위치: {transform.position}");
            SfxManager.Instance.PlaySfx(SfxType.PoisonFlask);


            // 1차 폭발 데미지
            ApplyExplosionDamage();

            // DoT 구역 생성
            CreatePoisonZone();

            // 플라스크 제거
            IsActive = false;
            Destroy(gameObject);
        }

        /// <summary>
        /// 폭발 데미지 적용
        /// </summary>
        private void ApplyExplosionDamage()
        {
            int enemyLayer = LayerMask.GetMask("Enemy");
            Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRadius, enemyLayer);

            //Debug.Log($"[PoisonFlaskProjectile] 폭발! 범위: {explosionRadius}, 적: {enemies.Length}명");

            foreach (var enemyCollider in enemies)
            {
                var enemy = enemyCollider.GetComponent<EnemyFinal>();
                if (enemy != null)
                {
                    flaskWeapon.ApplyDamage(enemy.gameObject,explosionDamage);
                    //Debug.Log($"[PoisonFlaskProjectile] 폭발 데미지: {enemyCollider.name} - {explosionDamage}");
                }
            }

        }

        /// <summary>
        /// 독 구역 생성
        /// </summary>
        private void CreatePoisonZone()
        {
            GameObject poisonZone = new GameObject("PoisonZone");
            poisonZone.transform.position = transform.position;

            PoisonDotZone zone = poisonZone.AddComponent<PoisonDotZone>();
            zone.Initialize(explosionRadius, dotDamage, dotDuration, projectileSize, poisonGasPrefab, PF);

            //Debug.Log($"[PoisonFlaskProjectile] 독 구역 생성 - DoT: {dotDamage}/초, 지속: {dotDuration}초");
        }

    }

    /// <summary>
    /// 독 DoT 구역
    /// </summary>
    public class PoisonDotZone : MonoBehaviour
    {
        private PoisonFlaskWeapon flaskWeapon = null;
        private float radius;
        private float dotDamage;
        private float duration;
        private float projectileSize;
        private GameObject gasPrefab;
        private GameObject gasEffect;

        private float endTime;
        private float lastDotTime;

        public PoisonFlaskWeapon PF { get { return flaskWeapon; } set { flaskWeapon = value; } }

        public void Initialize(float rad, float dot, float dur, float size, GameObject prefab, PoisonFlaskWeapon pf)
        {
            radius = rad;
            dotDamage = dot;
            duration = dur;
            projectileSize = size;
            gasPrefab = prefab;
            PF = pf;

            endTime = Time.time + duration;
            lastDotTime = Time.time;

            // 트리거 콜라이더
            CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider>();
            collider.center=Vector3.up;
            collider.radius = radius;
            collider.height = 3;
            collider.isTrigger = true;

            // 독가스 프리팹이 있으면 사용, 없으면 기본 시각화
            if (gasPrefab != null)
            {
                CreateGasEffectFromPrefab();
            }
        }

        /// <summary>
        /// 독가스 프리팹으로 이펙트 생성
        /// </summary>
        private void CreateGasEffectFromPrefab()
        {
            // 프리팹 인스턴스 생성
            gasEffect = Instantiate(gasPrefab, transform.position, Quaternion.identity, transform);

            // 프리팹 스케일을 projectileSize와 연동
            gasEffect.transform.localScale = Vector3.one * projectileSize;

            // 자식 ParticleSystem들의 startLifetime을 duration과 연동
            ParticleSystem[] particleSystems = gasEffect.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                // 파티클 시스템 정지
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                // main 모듈 설정
                var main = ps.main;
                main.startLifetime = duration;  // startLifetime을 duration과 연동
                main.loop = false;              // 루프 비활성화

                // shape 모듈에서 radius 조정 (원형인 경우)
                var shape = ps.shape;
                if (shape.shapeType == ParticleSystemShapeType.Circle ||
                    shape.shapeType == ParticleSystemShapeType.Sphere)
                {
                    shape.radius = radius;
                }

                // 파티클 재생
                ps.Play();

                //Debug.Log($"[PoisonDotZone] ParticleSystem 설정 - StartLifetime: {duration}초, Scale: {projectileSize}");
            }

            //Debug.Log($"[PoisonDotZone] 독가스 프리팹 생성 - Scale: {gasEffect.transform.localScale}, Duration: {duration}초");
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

            if (enemies.Length > 0)
            {
                //Debug.Log($"[PoisonDotZone] {enemies.Length}명에게 DoT 데미지 {dotDamage} 적용");
            }
        }
    }
}