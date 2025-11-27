using System.Collections.Generic;
using UnityEngine;

namespace DiceSurvivor.Utility
{
    public class WayPointsGenerator : MonoBehaviour
    {
        #region Variables
        public Transform center;                  // 중심 위치 (waypoint 기준점)
        public GameObject wayPointPrefab;         // 생성할 waypoint 프리팹
        public int count = 10;                    // 생성할 waypoint 개수
        public float radius = 2f;                 // 중심으로부터의 거리
        #endregion

        #region Properties
        #endregion

        #region Unity Event Methods
        private void Awake()
        {
            if(center != null)
            {
                center = transform;
            }
        }

        // Scene 뷰에서 Gizmo를 그리는 함수 (디버그용 시각화)
        private void OnDrawGizmos()
        {
            float angleStep = 180f / (count - 1);             // 각 waypoint 간의 각도 간격
            Vector3 centerPos = this.center != null ? center.position : transform.position;              // 중심 위치 설정

            for (int i = 0; i < count; i++)                   // waypoint 개수만큼 반복
            {
                float angleA = -90f + angleStep * i;          // 현재 각도 A
                float angleB = -90f + angleStep * (i + 1);    // 다음 각도 B

                Vector3 pointA = centerPos + new Vector3(Mathf.Cos(angleA * Mathf.Deg2Rad), 0, Mathf.Sin(angleA * Mathf.Deg2Rad)) * radius; // A 위치 계산
                Vector3 pointB = centerPos + new Vector3(Mathf.Cos(angleB * Mathf.Deg2Rad), 0, Mathf.Sin(angleB * Mathf.Deg2Rad)) * radius; // B 위치 계산

                // Gizmo를 그리려면 여기에 Line이나 Sphere를 추가할 수 있음
            }
        }
        #endregion

        #region Custom Methods

        // WayPoint들을 생성하고 리스트로 반환하는 함수
        [ContextMenu("Generate WayPoints")]
        public List<Transform> GenerateWayPoints()
        {
            List<Transform> wayPoints = new List<Transform>(); // 생성된 waypoint들을 저장할 리스트

            for (int i = 0; i < count; i++)                    // waypoint 개수만큼 반복
            {
                float angle = Mathf.Lerp(-90f, 90f, i / (float)(count - 1)); // -90도부터 90도까지 균등 분포
                Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.right; // 방향 벡터 계산

                Vector3 offset = Vector3.up * 0.5f - dir * 0.3f;             // 위치 오프셋 (위로 + 왼쪽으로)
                Vector3 pos = center.position + dir * radius + offset;      // waypoint 위치 계산

                GameObject wp = Instantiate(wayPointPrefab, pos, Quaternion.LookRotation(dir), center); // waypoint 프리팹 생성
                wp.name = $"WayPoint_{i:D2}";                                // 이름 지정
                wayPoints.Add(wp.transform);                                 // 리스트에 추가
            }

            return wayPoints; // 생성된 waypoint 리스트 반환
        }
        #endregion
    }
}
