using TMPro;
using UnityEngine;

public class TimerUI : MonoBehaviour
{
    public TextMeshProUGUI timerText;

    void Update()
    {
        if (GameTimerManager.Instance == null)
            return;

        // 진행 중이든 멈춰있든 현재 값 표시
        timerText.text = GameTimerManager.Instance.GetFormattedTime();
    }
}
