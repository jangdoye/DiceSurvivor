using UnityEngine;
using System.Collections.Generic;
using DiceSurvivor.Weapon;
using DiceSurvivor.Manager; 

namespace DiceSurvivor.Player
{
    public class PlayerWeaponControl : MonoBehaviour
    {
        #region Variables
        [SerializeField] List<WeaponController> weapons = new List<WeaponController>();
        [SerializeField] List<WeaponController> childweapons = new List<WeaponController>();
        float timer = 0;
        public float cooldown = 0.3f;
        #endregion

        #region Properties
        #endregion

        #region Unity Event Methods
        void Start()
        {
            foreach (Transform objects in gameObject.transform)
            {
                GameObject item = objects.gameObject;
                if (item.tag == "WeaponController")
                {
                    WeaponController weapon = item.GetComponent<WeaponController>();
                    WeaponController childWeapon = item.GetComponentInChildren<WeaponController>();
                    if (weapon != null || childWeapon != null)
                    {
                        if (weapon != null) weapons.Add(weapon);
                        else childweapons.Add(childWeapon);
                    }
                    //Debug.Log("무기 추가됨: " + item.name);
                }
            }
        }

        void Update()
        {
            timer += Time.deltaTime;
            if (timer >= cooldown)
            {
                CheckEnabledWeapon();
                timer = 0;
            }
        }
        #endregion

        #region Custom Methods
        void CheckEnabledWeapon()
        {
            ItemManager im = ItemManager.Instance;
            foreach (WeaponController weapon in weapons)
            {
                foreach (var item in im.GetSubWeapons)
                {
                    if (item != null && item.itemName == weapon.WeaponName)
                    {
                        weapon.gameObject.SetActive(true);
                        break;
                    }
                }
            }
            foreach (WeaponController weapon in childweapons)
            {
                GameObject parent = weapon.gameObject.transform.parent.gameObject;
                if (parent.activeSelf) continue;
                if (im.GetMeleeWeapon != null && im.GetMeleeWeapon.itemName == weapon.WeaponName)
                {
                    parent.SetActive(true);
                    weapon.gameObject.SetActive(true);
                }
            }
            //Debug.Log("활성화 된 무기 찾기 완료");
        }
        #endregion
    }
}