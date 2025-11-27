using UnityEngine;
using DiceSurvivor.Manager;

/// <summary>
/// 상점 테스트용 임시 아이템.
/// </summary>
[CreateAssetMenu(fileName = "TestItem", menuName = "Scriptable Objects/TestItem")]
public class TestItem : ScriptableObject
{
    public enum ItemType
    {
        None = 0,
        MeleeWeapon = 1,
        RangedWeapon = 2,
        SplashWeapon = 4,
        //카테고리 수가 적고, Ranged와 Splash를 상위 카테고리 Sub로 묶기 위해 비트 연산 사용
        SubWeapon = RangedWeapon | SplashWeapon,
        Passive = 8
    }
    //임시 아이템의 각 속성들
    public string ID;
    public string itemName;
    public ItemType type;
    //아이템 최대 레벨
    public int maxLevel = 8;
    //아이템 구매 가능 여부
    public bool canIBuy = true;
    //아이템 이미지
    public Sprite itemImage;

    //데이터테이블에서 받아올 무기 정보
    public WeaponStats weapon;
    public void SetWeaponStats(int lv)
    {
        switch (type)
        {
            case ItemType.MeleeWeapon:
                weapon = DataTableManager.Instance.GetMeleeWeapon(itemName, lv);
                break;
            case ItemType.RangedWeapon:
                weapon = DataTableManager.Instance.GetRangedWeapon(itemName, lv);
                break;
            case ItemType.SplashWeapon:
                weapon = DataTableManager.Instance.GetSplashWeapon(itemName, lv);
                break;
            default:
                return;
        }
    }
}
