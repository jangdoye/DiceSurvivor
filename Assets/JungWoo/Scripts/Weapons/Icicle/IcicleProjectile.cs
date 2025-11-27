using DiceSurvivor.Enemy;
using DiceSurvivor.Weapon;

using UnityEngine;

/// <summary>
/// 고드름 투사체 컴포넌트
/// </summary>
public class IcicleProjectile : MonoBehaviour
{
    private Vector3 targetPosition;
    private float fallSpeed;
    private float explosionRadius;
    private float explosionDamage;
    private float dotDamage;
    private float duration;
    private GameObject explosionEffectPrefab;
    private GameObject dotEffectPrefab;
    private bool hasExploded = false;

    /// <summary>
    /// 투사체 초기화
    /// </summary>
    public void Initialize(Vector3 target, float speed, float radius, float explDamage,
                          float dot, float dur, GameObject explEffect, GameObject dotEffect)
    {
        targetPosition = target;
        fallSpeed = speed;
        explosionRadius = radius;
        explosionDamage = explDamage;
        dotDamage = dot;
        duration = dur;
        explosionEffectPrefab = explEffect;
        dotEffectPrefab = dotEffect;
    }

    void Update()
    {
        if (hasExploded) return;

        // 목표 지점으로 낙하
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, fallSpeed * Time.deltaTime);

        // 목표 도달 체크
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            Explode();
        }
    }

    /// <summary>
    /// 폭발 처리
    /// </summary>
    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        //Debug.Log($"[Icicle] 폭발! 위치: {transform.position}, 범위: {explosionRadius}");

        // 1차 폭발 데미지
        ApplyExplosionDamage();

        // 폭발 이펙트 생성
        if (explosionEffectPrefab != null)
        {
            GameObject explosion = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 2f);
        }

        // DoT 구역 생성
        CreateDotZone();

        // 투사체 제거
        Destroy(gameObject);
    }

    /// <summary>
    /// 폭발 데미지 적용
    /// </summary>
    private void ApplyExplosionDamage()
    {
        // 폭발 범위 내 적 탐색
        int enemyLayer = LayerMask.GetMask("Enemy");
        Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRadius, enemyLayer);

        //Debug.Log($"[Icicle] 폭발 범위 내 적: {enemies.Length}명");

        foreach (var enemyCollider in enemies)
        {
            var enemy = enemyCollider.GetComponent<EnemyFinal>();
            if (enemy != null)
            {
                enemy.TakeDamage(explosionDamage);
                //Debug.Log($"[Icicle] {enemyCollider.name}에게 폭발 데미지 {explosionDamage} 적용");
            }
        }
    }

    /// <summary>
    /// DoT 구역 생성
    /// </summary>
    private void CreateDotZone()
    {
        if (dotDamage <= 0 || duration <= 0) return;

        // DoT 구역 오브젝트 생성
        GameObject dotZone = new GameObject($"IcicleDotZone_{Time.time}");
        dotZone.transform.position = transform.position;

        // DotZone 컴포넌트 추가
        DotZone zone = dotZone.AddComponent<DotZone>();
        zone.Initialize(explosionRadius, dotDamage, duration, dotEffectPrefab);

        //Debug.Log($"[Icicle] DoT 구역 생성 - 데미지: {dotDamage}/초, 지속: {duration}초");
    }
}

/// <summary>
/// DoT 구역 컴포넌트
/// </summary>
public class DotZone : MonoBehaviour
{
    private IcicleWeapon icicleWeapon;
    private float radius;
    private float dotDamage;
    private float duration;
    private float endTime;
    private float lastDotTime;
    private GameObject effectObject;
    private ParticleSystem particleEffect;

    public bool IsActive { get; private set; }

    /// <summary>
    /// DoT 구역 초기화
    /// </summary>
    public void Initialize(float rad, float dot, float dur, GameObject effectPrefab)
    {
        radius = rad;
        dotDamage = dot;
        duration = dur;
        endTime = Time.time + duration;
        lastDotTime = Time.time;
        IsActive = true;

        // 파티클 이펙트 생성
        CreateParticleEffect(effectPrefab);

        // 트리거 콜라이더 추가
        SphereCollider collider = gameObject.AddComponent<SphereCollider>();
        collider.radius = radius;
        collider.isTrigger = true;
    }

    /// <summary>
    /// 파티클 이펙트 생성
    /// </summary>
    private void CreateParticleEffect(GameObject effectPrefab)
    {
        if (effectPrefab != null)
        {
            // 프리팹에서 파티클 생성
            effectObject = Instantiate(effectPrefab, transform.position, Quaternion.identity, transform);

            // 프리팹의 ParticleSystem 찾기
            particleEffect = effectObject.GetComponent<ParticleSystem>();
            if (particleEffect == null)
            {
                particleEffect = effectObject.GetComponentInChildren<ParticleSystem>();
            }

            // 프리팹 파티클의 startLifetime을 duration과 연동
            if (particleEffect != null)
            {
                // 파티클 시스템 정지 (설정 변경을 위해)
                particleEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                var main = particleEffect.main;
                main.startLifetime = duration;  // startLifetime을 duration과 연동
                main.duration = duration;        // 전체 지속시간도 duration과 연동
                main.loop = false;              // 루프 비활성화

                // 파티클 재생
                particleEffect.Play();

                //Debug.Log($"[DotZone] 프리팹 파티클 설정 - startLifetime: {duration}초");
            }
        }

        // 이펙트 크기를 explosionRadius와 연동
        effectObject.transform.localScale = Vector3.one * radius;

        // 자식 파티클 시스템들도 duration 연동 (프리팹에 여러 파티클이 있는 경우)
        ParticleSystem[] childParticles = effectObject.GetComponentsInChildren<ParticleSystem>();
        foreach (var childParticle in childParticles)
        {
            // 자기 자신은 제외 (이미 처리함)
            if (childParticle == particleEffect) continue;

            // 파티클 정지 후 설정 변경
            childParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var childMain = childParticle.main;
            childMain.startLifetime = duration;  // 모든 자식 파티클의 startLifetime도 연동
            childMain.duration = duration;

            // 파티클 재생
            childParticle.Play();
        }

        //Debug.Log($"[DotZone] 파티클 최종 설정 - Scale: {effectObject.transform.localScale}, Duration: {duration}, 자식 파티클 수: {childParticles.Length}");
    }

    void Update()
    {
        // 지속 시간 체크
        if (Time.time >= endTime)
        {
            Deactivate();
            return;
        }

        // 1초마다 DoT 데미지
        if (Time.time - lastDotTime >= 1f)
        {
            ApplyDotDamage();
            lastDotTime = Time.time;
        }
    }

    /// <summary>
    /// 범위 내 적에게 DoT 데미지 적용
    /// </summary>
    private void ApplyDotDamage()
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
            //Debug.Log($"[DotZone] {enemies.Length}명에게 DoT 데미지 {dotDamage} 적용");
        }
    }

    /// <summary>
    /// DoT 구역 비활성화
    /// </summary>
    private void Deactivate()
    {
        IsActive = false;
        //Debug.Log($"[DotZone] 종료 - 위치: {transform.position}");
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        // DoT 범위 표시
        Gizmos.color = new Color(0, 0.5f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, 15f);
    }
}
