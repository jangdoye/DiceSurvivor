using System.Collections;
using UnityEngine;

public class StageNode : MonoBehaviour
{
    [Header("Stage Info")]
    public string sceneName;

    [Header("Progression")]
    public int stageIndex = 0;

    [Header("Lock State")]
    [SerializeField] private bool locked = false;

    [Tooltip("자물쇠 아이콘(가능하면 할당). 없으면 자식에서 lockIconChildName으로 자동 탐색")]
    [SerializeField] private Transform lockIcon;
    [SerializeField] private string lockIconChildName = "LockIcon";

    [SerializeField] private Renderer[] tintTargets;
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color lockedColor = new(0.55f, 0.55f, 0.55f);

    public StageNode[] connectedNodes;

    // ====== 코드 흔들기 세팅 ======
    [Header("Lock Shake (Code)")]
    [Tooltip("흔들기 총 시간(초)")]
    [SerializeField] private float shakeDuration = 0.25f;
    [Tooltip("최대 흔들림 폭(유닛)")]
    [SerializeField] private float shakeAmplitude = 0.12f;
    [Tooltip("흔들림이 점점 줄어드는 감쇠(1=감쇠없음, 0.85~0.95 권장)")]
    [Range(0.7f, 1.0f)]
    [SerializeField] private float decay = 0.9f;
    [Tooltip("Position 흔들림 사용할지 (끄면 Rotation만)")]
    [SerializeField] private bool usePositionShake = true;
    [Tooltip("Rotation 흔들림 각도(±도)")]
    [SerializeField] private float rotateDegrees = 10f;

    Coroutine shakeRoutine;

    public bool IsUnlocked() => !locked;

    /// 파괴/미할당 시 자동 재탐색하는 안전 게터
    public Transform LockIcon
    {
        get
        {
            if (lockIcon == null)
            {
                var found = string.IsNullOrEmpty(lockIconChildName)
                    ? null
                    : transform.Find(lockIconChildName);
                if (found) lockIcon = found;
            }
            return lockIcon;
        }
        set { lockIcon = value; }
    }

    private void Start() => ApplyVisual();
    private void OnValidate() => ApplyVisual();

    public void SetLocked(bool value)
    {
        locked = value;
        ApplyVisual();
    }

    public void ForceLockVisualRefresh() => ApplyVisual();

    private void ApplyVisual()
    {
        var icon = LockIcon;
        if (icon) icon.gameObject.SetActive(!IsUnlocked());

        if (tintTargets != null)
        {
            var c = IsUnlocked() ? unlockedColor : lockedColor;
            foreach (var r in tintTargets)
            {
                if (!r) continue;
                if (r.material && r.material.HasProperty("_Color"))
                    r.material.color = c;
            }
        }
    }

    /// 잠긴 노드 상호작용 시 잠금 아이콘을 흔들리게 하는 공용 메서드(Animator 불필요)
    public void TriggerLockShake()
    {
        var icon = LockIcon;
        if (!icon) return;

        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(CoShake(icon));
    }

    IEnumerator CoShake(Transform target)
    {
        // 기준값 저장
        Vector3 basePos = target.localPosition;
        Quaternion baseRot = target.localRotation;

        float t = 0f;
        float amp = shakeAmplitude;

        // 초기에 한 번 짧게 팍
        if (usePositionShake) target.localPosition = basePos + Random.insideUnitSphere * (amp * 0.5f);
        target.localRotation = baseRot * Quaternion.Euler(0f, 0f, Random.Range(-rotateDegrees, rotateDegrees));

        while (t < shakeDuration)
        {
            t += Time.unscaledDeltaTime; // UI/메뉴에서도 흔들리게 TimeScale 영향 없음

            // 점점 감쇠
            amp *= decay;

            // Position 흔들림
            if (usePositionShake)
            {
                Vector2 circle = Random.insideUnitCircle * amp;
                target.localPosition = basePos + new Vector3(circle.x, circle.y, 0f);
            }

            // Rotation 흔들림 (좌우 턱턱)
            float rot = Mathf.Sin(t * 60f) * rotateDegrees * (amp / Mathf.Max(shakeAmplitude, 0.0001f));
            target.localRotation = baseRot * Quaternion.Euler(0f, 0f, rot);

            yield return null;
        }

        // 원위치
        target.localPosition = basePos;
        target.localRotation = baseRot;
        shakeRoutine = null;
    }
}
