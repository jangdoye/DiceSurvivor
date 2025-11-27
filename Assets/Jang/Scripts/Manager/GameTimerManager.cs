using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTimerManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static GameTimerManager Instance { get; private set; }

    [Header("설정 옵션")]
    [SerializeField] private bool dontDestroyOnLoad = true;           // 씬 전환 시 유지할지 여부
    [SerializeField] private bool autoStartOnSceneLoaded = true;      // 씬이 로드되면 자동 시작할지 여부

    [SerializeField] private float startTimeInSeconds = 600f;         // 시작 시간 (초) — 기본값: 10분 = 600초

    // 현재 타이머 상태
    public bool IsRunning { get; private set; } = false;
    public float RemainingTime { get; private set; } = 0f;

    // 타이머 종료 여부
    public bool IsFinished => RemainingTime <= 0f;

    // 싱글톤 설정
    private void Awake()
    {
        // 이미 인스턴스가 있으면 파괴 (중복 방지)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 필요하면 씬 전환 시에도 파괴되지 않도록 유지
        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);
    }

    // 씬 로드 이벤트 등록
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // 씬 로드 이벤트 해제
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬이 로드되었을 때 자동으로 타이머 초기화 & 시작
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetTimer();              // 10분으로 리셋
        if (autoStartOnSceneLoaded)
            StartTimer();          // 자동 시작
    }

    // 매 프레임마다 타이머 감소
    private void Update()
    {
        // 타이머가 실행 중이 아니거나 이미 끝났으면 리턴
        if (!IsRunning || IsFinished) return;

        // 시간 감소
        RemainingTime -= Time.deltaTime;

        // 0 이하로 떨어지면 종료 처리
        if (RemainingTime <= 0f)
        {
            RemainingTime = 0f;
            IsRunning = false;

            // 타이머가 끝났을 때 원하는 처리 (예: 보스 등장)
            //Debug.Log("⏰ 타이머 종료!");
        }
    }

    // 타이머 시작
    public void StartTimer()
    {
        IsRunning = true;
    }

    // 타이머 일시정지
    public void StopTimer()
    {
        IsRunning = false;
    }

    // 타이머 리셋 (시간: 시작시간, 정지 상태)
    public void ResetTimer()
    {
        IsRunning = false;
        RemainingTime = startTimeInSeconds;
    }

    // 남은 시간을 "MM:SS" 형식의 문자열로 반환
    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(RemainingTime / 60f);
        int seconds = Mathf.FloorToInt(RemainingTime % 60f);
        return $"{minutes:D2}:{seconds:D2}";
    }
}
