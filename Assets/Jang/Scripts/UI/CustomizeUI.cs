using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class CustomizeUI : MonoBehaviour
{
    [Header("UI Reference")]
    public TMP_Dropdown headPropDropdown;
    public TMP_Dropdown eyeDropdown;
    public TMP_Dropdown mouthDropdown;

    [Header("Preview Character")]
    public GameObject previewCharacter;

    [Header("Customization Parts")]
    public GameObject[] headProps;
    public GameObject[] eyes;
    public GameObject[] mouths;

    private void Start()
    {
        // 불러와서 미리보기 적용
        int headIndex = PlayerPrefs.GetInt("SelectedHeadProp", 0);
        int eyeIndex = PlayerPrefs.GetInt("SelectedEyeVariant", 0);
        int mouthIndex = PlayerPrefs.GetInt("SelectedMouthVariant", 0);

        headPropDropdown.value = headIndex;
        eyeDropdown.value = eyeIndex;
        mouthDropdown.value = mouthIndex;

        UpdateCustomization(headProps, headIndex);
        UpdateCustomization(eyes, eyeIndex);
        UpdateCustomization(mouths, mouthIndex);

        headPropDropdown.onValueChanged.AddListener((index) => UpdateCustomization(headProps, index));
        eyeDropdown.onValueChanged.AddListener((index) => UpdateCustomization(eyes, index));
        mouthDropdown.onValueChanged.AddListener((index) => UpdateCustomization(mouths, index));
    }

    void UpdateCustomization(GameObject[] items, int selectedIndex)
    {
        for (int i = 0; i < items.Length; i++)
        {
            items[i].SetActive(i == selectedIndex);
        }
    }

    public void SaveCustomization()
    {
        int headIndex = headPropDropdown.value;
        int eyeIndex = eyeDropdown.value;
        int mouthIndex = mouthDropdown.value;

        PlayerPrefs.SetInt("SelectedHeadProp", headIndex);
        PlayerPrefs.SetInt("SelectedEyeVariant", eyeIndex);
        PlayerPrefs.SetInt("SelectedMouthVariant", mouthIndex);
        PlayerPrefs.Save();

        //Debug.Log($"Saved: Head={headIndex}, Eye={eyeIndex}, Mouth={mouthIndex}");

        // 메인 메뉴로 복귀
        StartCoroutine(ReturnToMainMenu());
    }

    IEnumerator ReturnToMainMenu()
    {
        yield return new WaitForEndOfFrame();
        SceneManager.LoadScene("MainMenu");
    }
}
