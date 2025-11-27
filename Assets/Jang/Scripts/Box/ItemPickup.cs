using DiceSurvivor.Audio;
using DiceSurvivor.Player;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public enum ItemType { Heal, Coin, Buff, Magnet }
    public ItemType itemType;

    public float healAmount = 20f;
    public int goldAmount = 1;
    public float buffDuration = 5f;
    public float speedMultiplier = 1.5f;
    public float magnetDuration = 5f;

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log($"Trigger with {other.name}");

        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        // 효과 실행 ...
        SfxManager.Instance.PlaySfx(SfxType.CollectItem);

        switch (itemType)
        {
            case ItemType.Heal:
                player.Heal(healAmount);
                break;

            case ItemType.Coin:
                player.AddCoins(goldAmount);
                break;

            case ItemType.Buff:
                player.ApplySpeedBuff(speedMultiplier, buffDuration);
                break;

            case ItemType.Magnet:
                player.ActivateMagnet(magnetDuration);
                break;
        }

        Destroy(gameObject); // 아이템 제거
    }
}
