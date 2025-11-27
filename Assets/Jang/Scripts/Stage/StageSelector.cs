using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelector : MonoBehaviour
{
    public static StageSelector Instance;

    [Header("Refs")]
    public PlayerMover player;
    public SceneFader sceneFader;
    public AudioManager audioManager;

    [Header("Start Node")]
    public StageNode startingNode;
    [SerializeField] private bool loadLastStagePosition = false;

    [Header("처음부터 열어둘 씬 이름들(예: Stage1)")]
    public string[] defaultUnlockedSceneNames;

    StageNode selectedNode;   // 현재 선택(커서)
    StageNode currentNode;    // 실제 도착 노드

    void Awake() => Instance = this;

    void Start()
    {
        audioManager = AudioManager.Instance;
        if (audioManager) audioManager.PlayBgm("Play");

        var nodes = FindObjectsOfType<StageNode>();

        // 0) 전부 잠금
        foreach (var n in nodes) n.SetLocked(true);

        // 1) 시작 노드는 항상 해제
        if (startingNode != null) startingNode.SetLocked(false);

        // 2) 인덱스 기반 자동 언락 (maxCleared+1까지)
        int maxCleared = PlayerPrefs.GetInt("MAX_CLEARED_STAGE_INDEX", 0);
        foreach (var n in nodes)
        {
            if (n.stageIndex > 0 && n.stageIndex <= maxCleared + 1)
                n.SetLocked(false);
        }

        // 3) 문자열 화이트리스트도 병행 지원
        if (defaultUnlockedSceneNames != null && defaultUnlockedSceneNames.Length > 0)
        {
            foreach (var n in nodes)
            {
                foreach (var s in defaultUnlockedSceneNames)
                {
                    if (!string.IsNullOrEmpty(s) && n.sceneName == s)
                    {
                        n.SetLocked(false);
                        break;
                    }
                }
            }
        }

        // 4) 개별 UNLOCK_<sceneName>=1 키도 존중
        foreach (var n in nodes)
        {
            if (PlayerPrefs.GetInt($"UNLOCK_{n.sceneName}", 0) == 1)
                n.SetLocked(false);
        }

        // 5) 시작 선택 노드 결정 & 이동
        StageNode found = null;
        if (loadLastStagePosition)
        {
            string last = PlayerPrefs.GetString("LastStageNode", "");
            if (!string.IsNullOrEmpty(last))
            {
                foreach (var n in nodes) { if (n.name == last) { found = n; break; } }
            }
        }

        selectedNode = found ?? (startingNode ? startingNode : FindClosestNodeToPlayer());
        if (player && selectedNode) player.MoveTo(selectedNode.transform.position);

        //Debug.Log($"[선택 씬 시작] 시작 노드: {selectedNode?.name}");
    }

    void Update()
    {
        if (!player) return;

        if (!player.IsMoving())
        {
            if (selectedNode && player.IsAtTarget())
            {
                if (currentNode != selectedNode)
                {
                    currentNode = selectedNode;
                    PlayerPrefs.SetString("LastStageNode", currentNode.name);
                    PlayerPrefs.Save();
                }
            }

            // 방향키 내비게이션 (잠금 노드도 이동 허용)
            var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (input.magnitude > 0.1f) TryMoveByInput(input);

            // 입장 키 (해제 상태 + 도착 완료만)
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                TryEnterCurrent();
        }
    }

    // 마우스 클릭(잠금이어도 이동은 허용; 입장은 아래에서 가드)
    public void OnNodeClicked(StageNode node)
    {
        if (!node) return;

        if (node == currentNode && player.IsAtTarget())
        {
            if (node.IsUnlocked())
            {
                Enter(node.sceneName, node);
            }
            else
            {
                // 현재 노드가 잠금 상태에서 재입장 시도 → 흔들림
                node.TriggerLockShake();
            }
        }
        else
        {
            selectedNode = node;
            if (player) player.MoveTo(node.transform.position);
        }
    }

    void TryEnterCurrent()
    {
        if (currentNode && player.IsAtTarget() && currentNode.IsUnlocked())
            Enter(currentNode.sceneName, currentNode);
        else if (currentNode && player.IsAtTarget() && !currentNode.IsUnlocked())
            currentNode.TriggerLockShake(); // 키보드로 잠금 입장 시도 시에도 흔들림
    }

    void Enter(string sceneName, StageNode node)
    {
        if (node != null && !node.IsUnlocked())
        {
            node.TriggerLockShake(); // 잠김: 흔들리고 종료
            //Debug.Log("[Enter] 잠금 상태라 입장 불가");
            return;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("sceneName이 비어 있습니다.");
            return;
        }

        if (audioManager) audioManager.StopBgm();
        if (sceneFader) sceneFader.FadeTo(sceneName);
        else SceneManager.LoadScene(sceneName);
    }

    // 잠금 여부와 무관하게 '방향'이 가장 잘 맞는 이웃으로 이동
    void TryMoveByInput(Vector2 input)
    {
        if (!selectedNode) return;

        Vector3 cur = selectedNode.transform.position;
        Vector3 dirInput = new(input.x, 0f, input.y);
        if (dirInput.sqrMagnitude < 1e-4f) return;
        dirInput.Normalize();

        StageNode best = null;
        float bestDot = 0.7f;

        foreach (var node in selectedNode.connectedNodes)
        {
            if (!node) continue;

            Vector3 d = node.transform.position - cur; d.y = 0f;
            if (d.sqrMagnitude < 1e-4f) continue;
            d.Normalize();

            float dot = Vector3.Dot(dirInput, d);
            if (dot > bestDot) { bestDot = dot; best = node; }
        }

        if (best)
        {
            selectedNode = best;
            if (player) player.MoveTo(best.transform.position);
        }
    }

    StageNode FindClosestNodeToPlayer()
    {
        StageNode closest = null;
        float min = float.MaxValue;
        foreach (var n in FindObjectsOfType<StageNode>())
        {
            float d = Vector3.Distance(player.transform.position, n.transform.position);
            if (d < min) { min = d; closest = n; }
        }
        return closest;
    }

    public void OnBackButton() => SceneManager.LoadScene("MainMenu");
}
