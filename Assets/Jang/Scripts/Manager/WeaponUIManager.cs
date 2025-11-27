using UnityEngine;
using UnityEngine.UI;
using DiceSurvivor.Manager; 

public class WeaponUIManager : SingletonManager<WeaponUIManager>
{
    [Header("Main Weapon")]
    [SerializeField] private Image mainWeaponImage;

    [Header("Sub Weapons ")]
    [SerializeField] private Image[] subWeaponImages;

    [Header("Equipments ")]
    [SerializeField] private Image[] equipmentImages;

    // 메인 무기 설정
    public void SetMainWeapon(Sprite sprite)
    {
        if (mainWeaponImage != null)
        {
            mainWeaponImage.sprite = sprite;
            mainWeaponImage.enabled = true;
        }
    }

    // 서브 무기 슬롯 설정
    public void SetSubWeapon(int index, Sprite sprite)
    {
        if (index < 0 || index >= subWeaponImages.Length) return;

        subWeaponImages[index].sprite = sprite;
        subWeaponImages[index].enabled = true;
    }

    // 장비 슬롯 설정
    public void SetEquipment(int index, Sprite sprite)
    {
        if (index < 0 || index >= equipmentImages.Length) return;

        equipmentImages[index].sprite = sprite;
        equipmentImages[index].enabled = true;
    }

}
