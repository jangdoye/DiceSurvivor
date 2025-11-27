using UnityEngine;
using System;
using System.Collections;

public class Box : MonoBehaviour
{
    #region Variables
    public event Action onBoxDestroyed;

    private Animator animator;
    private bool isBroken = false;

    public GameObject[] dropItems; // 아이템 프리팹들
    public Transform dropPoint;    // 아이템 생성 위치
    #endregion

    #region Unity Event Method
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    #endregion

    #region Custom Method
    public void BreakOnTouch()
    {
        if (isBroken) return;
        isBroken = true;

        animator.SetTrigger("Break");
        onBoxDestroyed?.Invoke(); // 박스 스포너에 알림

        StartCoroutine(DestroyAfterDelay());
        DropItem();
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(1f); // 애니메이션 재생 시간
        Destroy(gameObject);
    }

    private void DropItem()
    {
        if (dropItems.Length == 0) return;

        int randomIndex = UnityEngine.Random.Range(0, dropItems.Length);
        GameObject item = Instantiate(dropItems[randomIndex], dropPoint.position, Quaternion.identity);
    }
    private void OnDestroy()
    {
        if (!isBroken)
        {
            onBoxDestroyed?.Invoke();
        }
    }
    #endregion
}
