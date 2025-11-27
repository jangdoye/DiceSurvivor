using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class StageNodeClickable : MonoBehaviour, IPointerClickHandler
{
    public StageNode node;

    void Awake()
    {
        if (node == null) node = GetComponent<StageNode>();
        // (Animator 직접 캐싱 불필요) node.TriggerLockShake()가 내부에서 알아서 찾음
    }

    public void OnPointerClick(PointerEventData eventData) => HandleClick();
    void OnMouseDown() => HandleClick(); // EventSystem 없을 때 백업

    void HandleClick()
    {
        if (node == null) return;

        if (!node.IsUnlocked())
        {
            // 잠금이면 즉시 흔들림
            node.TriggerLockShake();

            // 이동은 허용 (입장은 StageSelector에서 막힘)
            StageSelector.Instance?.OnNodeClicked(node);
            return;
        }

        // 언락일 때
        StageSelector.Instance?.OnNodeClicked(node);
    }
}
