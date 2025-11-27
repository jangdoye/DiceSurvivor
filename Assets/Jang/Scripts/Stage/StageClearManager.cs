using UnityEngine;

public class StageClearManager : MonoBehaviour
{
    public int stageID;

    public void OnClear()
    {
        PlayerPrefs.SetInt($"Stage{stageID}_Cleared", 1);
        PlayerPrefs.Save();
    }
}
