using UnityEngine;
using TMPro;


public class GoldWallet : MonoBehaviour
{
    public static GoldWallet Instance;

    [SerializeField] private int gold = 0;
    [SerializeField] private TextMeshProUGUI goldText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        RefreshUI();
    }

    public void Add(int amount)
    {
        gold += amount;
        RefreshUI();
    }

    public bool TrySpend(int amount)
    {
        if (gold < amount) return false;
        gold -= amount;
        RefreshUI();
        return true;
    }

    public void RefreshUI()
    {
        if (goldText != null)
            goldText.text = gold.ToString();
    }

    public int GetGold() => gold;
}
