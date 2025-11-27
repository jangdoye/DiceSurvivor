using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomizeUIAutoSetup : MonoBehaviour
{
    [Header("Dropdown Targets")]
    public TMP_Dropdown headDropdown;
    public TMP_Dropdown eyeDropdown;
    public TMP_Dropdown mouthDropdown;

    [Header("Custom Names")]
    public List<string> headNames = new() { "head 1", "head 2", "head 3", "head 4", "head 5", "head 6" };
    public List<string> eyeNames = new() { "eye 1", "eye 2", "eye 3", "eye 4" };
    public List<string> mouthNames = new() { "mouth 1", "mouth 2" };

    [Header("Font Settings")]
    public float fontSize = 30f;

    private void Start()
    {
        ApplyDropdownNames(headDropdown, headNames);
        ApplyDropdownNames(eyeDropdown, eyeNames);
        ApplyDropdownNames(mouthDropdown, mouthNames);
    }

    private void ApplyDropdownNames(TMP_Dropdown dropdown, List<string> names)
    {
        if (dropdown == null || names == null || names.Count == 0) return;

        // 드롭다운 옵션 리스트 설정
        dropdown.options = names.Select(n => new TMP_Dropdown.OptionData(n)).ToList();
        dropdown.RefreshShownValue();

        // 드롭다운 템플릿 내 폰트 사이즈 조절
        if (dropdown.captionText != null)
            dropdown.captionText.fontSize = fontSize;

        if (dropdown.itemText != null)
            dropdown.itemText.fontSize = fontSize;
    }
}
