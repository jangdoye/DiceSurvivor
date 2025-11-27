#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class StageNodeAutoLinker : MonoBehaviour
{
    public float linkRadius = 3f;

    [ContextMenu("Auto Link Nearby Nodes")]
    void AutoLink()
    {
        StageNode[] allNodes = FindObjectsOfType<StageNode>();
        foreach (StageNode node in allNodes)
        {
            var neighbors = new System.Collections.Generic.List<StageNode>();
            foreach (StageNode other in allNodes)
            {
                if (other == node) continue;
                if (Vector3.Distance(node.transform.position, other.transform.position) <= linkRadius)
                {
                    neighbors.Add(other);
                }
            }
            node.connectedNodes = neighbors.ToArray();
            EditorUtility.SetDirty(node);
        }

        //Debug.Log("노드 자동 연결 완료!");
    }
}
#endif
