using UnityEngine;

public class PlayerCustomizer : MonoBehaviour
{
    [Header("Head Props")]
    public GameObject[] headProps;

    [Header("Eyes")]
    public GameObject[] eyes;

    [Header("Mouths")]
    public GameObject[] mouths;

    private void Start()
    {
        // 저장된 값 불러오기
        int headIndex = PlayerPrefs.GetInt("SelectedHeadProp", 0);
        int eyeIndex = PlayerPrefs.GetInt("SelectedEyeVariant", 0);
        int mouthIndex = PlayerPrefs.GetInt("SelectedMouthVariant", 0);

        //Debug.Log($"[PlayerCustomizer] Loaded: Head={headIndex}, Eye={eyeIndex}, Mouth={mouthIndex}");

        ApplyPart(headProps, headIndex);
        ApplyPart(eyes, eyeIndex);
        ApplyPart(mouths, mouthIndex);
    }

    private void ApplyPart(GameObject[] partArray, int selectedIndex)
    {
        for (int i = 0; i < partArray.Length; i++)
        {
            if (partArray[i] != null)
                partArray[i].SetActive(i == selectedIndex);
        }
    }
}
