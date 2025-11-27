using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DiceSurvivor.Manager;

public class ItemDetailDisplay : MonoBehaviour {
    #region Variables
    [Header("UI Elements")]
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    public TextMeshProUGUI itemLevelText;
    #endregion

    #region Unity Event Methods
    private void Start() {
        gameObject.SetActive(false); // 기본적으로 비활성화
    }
    #endregion

    #region Custom Methods
    public void ShowDetail(TestItem item) {
        if (item == null) return;



        // ShopManager에서 현재 아이템 레벨 가져오기
        int currentLevel = ItemManager.Instance.GetItemLevel(item.itemName);

        // UI 업데이트
        itemNameText.text = item.itemName;
        itemDescriptionText.text = ShopManagerTest.Instance.GetItemDescription(item);
        itemLevelText.text = $"Level: {currentLevel + 1} / {item.maxLevel}";

        gameObject.SetActive(true);
    }

    public void HideDetail() {
        gameObject.SetActive(false);
    }
    #endregion
}