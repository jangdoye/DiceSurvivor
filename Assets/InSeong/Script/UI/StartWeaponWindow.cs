using UnityEngine;
using System.Collections.Generic;

namespace DiceSurvivor.UI
{
    /// <summary>
    // StartWeaponPanel에 무작위로 근접 무기를 뽑아서 넘겨 주는 역할
    public class StartWeaponWindow : MonoBehaviour
    {
        #region Variables
        public TestItemArray meleeWeaponArray;
        StartWeaponPanel[] weaponSelections;


        #endregion

        #region Properties
        #endregion

        #region Unity Event Methods
        void Start()
        {
            weaponSelections = GetComponentsInChildren<StartWeaponPanel>();
            AssignRandomWeapons();
            Time.timeScale = 0f;
        }
        #endregion

        #region Custom Methods
        void AssignRandomWeapons()
        {
            var items = meleeWeaponArray.items;
            var usedIndices = new HashSet<int>();
            var rnd = new System.Random();

            // 패널 수만큼 중복 없이 무작위로 뽑아서 할당
            for (int i = 0; i < weaponSelections.Length && i < items.Count; i++)
            {
                int idx;
                do
                {
                    idx = rnd.Next(items.Count);
                } while (usedIndices.Contains(idx));
                usedIndices.Add(idx);

                items[idx].SetWeaponStats(1);
                weaponSelections[i].Parent = this;
                weaponSelections[i].SetRandomWeapon(items[idx]);
                //고른 후 상점을 호출하여 현재 보유하지 않은 모든 물리 무기를 비활성화
            }
        }

        public void RemoveUnused(string item)
        {
            foreach (var weapon in meleeWeaponArray.items)
            {
                if (weapon.itemName != item)
                {
                    weapon.canIBuy = false;
                }
            }
            transform.parent.gameObject.SetActive(false);
        }
        
        #endregion
    }
}