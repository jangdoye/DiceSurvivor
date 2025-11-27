using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어 주변에 있는 타일의 주기적으로 확인하여
/// 근처 거리 안에 있는 타일을 활성화하고, 먼 타일을 비활성화하는 클래스.
/// </summary>
public class PlayerTileTracker : MonoBehaviour
{
    #region Variables
    private Transform player;                 // 플레이어 Transform (자기 자신)
    [SerializeField]private float checkInterval = 0.3f;       // 타일 체크 주기 (초 단위)
    #endregion

    #region Unity Event Method
    void Start()
    {
        player = transform;                   // 플레이어 자신의 Transform 할당
        StartCoroutine(UpdateTilesRoutine()); // 타일 체크 코루틴 시작
    }
    #endregion

    #region Custom Method

    /// <summary>
    /// 일정 시간 간격으로 주변 타일 상태를 업데이트하는 코루틴
    /// </summary>
    IEnumerator UpdateTilesRoutine()
    {
        while (true)
        {
            UpdateNearbyTiles();                         // 주변 타일 체크
            yield return new WaitForSeconds(checkInterval); // 대기 시간 후
        }
    }

    /// <summary>
    /// 플레이어 주변 타일을 확인하고 거리 멀면 활성화, 가까우면 비활성화
    /// </summary>
    void UpdateNearbyTiles()
    {
        int chunkSize = MapChunkManager.Instance.GetChunkSize();         // 타일(청크) 크기
        float activationRange = MapChunkManager.Instance.GetActivationRange(); // 타일 활성화 거리 범위

        // 플레이어 위치를 기준으로 현재 어느 청크(타일) 안에 있는지 좌표 계산
        Vector2Int playerCoord = new(
            Mathf.FloorToInt(player.position.x / chunkSize),
            Mathf.FloorToInt(player.position.z / chunkSize)
        );

        // 활성화 범위 기준으로 얼마나 많은 청크를 확인할지 계산
        int range = Mathf.CeilToInt(activationRange / chunkSize);

        // range 범위 안에 있는 청크들을 반복하며 확인
        for (int x = -range; x <= range; x++)
        {
            for (int z = -range; z <= range; z++)
            {
                // 주변 청크 좌표 계산
                Vector2Int coord = new(playerCoord.x + x, playerCoord.y + z);

                // 해당 청크의 월드 위치 계산 (중심점 기준)
                Vector3 tileWorldPos = new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);

                // 플레이어와의 거리 계산
                float distance = Vector3.Distance(player.position, tileWorldPos);

                if (distance <= activationRange)
                    MapChunkManager.Instance.GetOrCreateTileAt(coord);   // 가까운 거리면 타일 생성 or 활성화
                else
                    MapChunkManager.Instance.DeactivateTile(coord);     // 먼 거리면 타일 비활성화
            }
        }
    }
    #endregion
}