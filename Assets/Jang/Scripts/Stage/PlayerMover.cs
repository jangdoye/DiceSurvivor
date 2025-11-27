using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    private Vector3 targetPosition;     // 이동할 목표 위치
    private bool isMoving = false;      // 현재 이동 중인지 여부
    public float moveSpeed = 5f;        // 이동 속도
    public float rotateSpeed = 10f;     // 회전 속도

    // 외부에서 호출하여 이동 시작
    public void MoveTo(Vector3 pos)
    {
        targetPosition = pos;
        isMoving = true;
    }

    // 목표 위치 도착 여부 확인
    public bool IsAtTarget()
    {
        return Vector3.Distance(transform.position, targetPosition) < 0.05f;
    }

    // 이동 중인지 확인
    public bool IsMoving()
    {
        return isMoving;
    }

    private void Update()
    {
        if (!isMoving) return;

        // 목표 위치로 이동
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // 이동 방향을 바라보도록 회전
        Vector3 dir = (targetPosition - transform.position).normalized;
        dir.y = 0f; // y축 제거해서 수평 회전만

        if (dir != Vector3.zero)
        {
            Quaternion look = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, rotateSpeed * Time.deltaTime);
        }

        // 도착하면 이동 중지
        if (IsAtTarget())
        {
            isMoving = false;
        }
    }
}
