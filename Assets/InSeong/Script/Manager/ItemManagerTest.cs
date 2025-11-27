using System.Collections.Generic;
using UnityEngine;

namespace DiceSurvivor.Manager {
    public class ItemManager : SingletonManager<ItemManager>
    {
        #region Variables
        //플레이어가 보유한 아이템의 레벨 정보
        private Dictionary<string, int> itemLevelDict = new Dictionary<string, int>();

        //플레이어가 실제로 장착한 아이템들
        [SerializeField] private TestItem meleeWeapon;
        [SerializeField] private List<TestItem> subWeapons = new List<TestItem>();
        [SerializeField] private List<TestItem> passives = new List<TestItem>();

        //최대 보유 가능 수량
        [SerializeField] private int maxWeapons = 1;
        [SerializeField] private int maxSubWeapons = 6;
        [SerializeField] private int maxPassives = 6;

        private Dictionary<TestItem.ItemType, int> maxItemCounts;
        #endregion

        #region Properties
        public TestItem GetMeleeWeapon { get { return meleeWeapon; } }
        public List<TestItem> GetSubWeapons
        { get { return subWeapons; } }
        public List<TestItem> GetPassives { get { return passives; } }

        
        #endregion

        #region Unity Event Methods
        protected override void Awake()
        {
            base.Awake();
            maxItemCounts = new Dictionary<TestItem.ItemType, int>() {
                { TestItem.ItemType.MeleeWeapon, maxWeapons },
                //원거리 아이템과 스플래시 아이템은 둘이 합쳐서 maxSubWeapons 개수로 제한, 초기화만 이렇게
                { TestItem.ItemType.RangedWeapon, maxSubWeapons },
                { TestItem.ItemType.SplashWeapon, maxSubWeapons },
                { TestItem.ItemType.Passive, maxPassives }
            };
        }

        #endregion

        #region Custom Methods
        //아이템 레벨 조회
        public int GetItemLevel(string itemName)
        {
            return itemLevelDict.TryGetValue(itemName, out int level) ? level : 0;
        }

        public void SetItemInfo(TestItem item, WeaponStats weapon)
        {
            if (item == null || weapon == null) return;
            item.SetWeaponStats(weapon.level);
        }

        //아이템 구매/레벨업
        public bool BuyItem(TestItem item)
        {
            //Debug.Log(ShopManagerTest.Instance.isBuyingMode);
            if (!ShopManagerTest.Instance.isBuyingMode) return false;
            //Debug.Log(item.canIBuy);
            if (!item.canIBuy) return false;

            string itemName = item.itemName;
            bool hasItem = GetItemLevel(itemName) > 0;

            //Debug.Log("아이템 보유 여부 점검");
            //Debug.Log(hasItem);
            //Debug.Log("아이템 추가 가능 여부 점검");
            //Debug.Log(CanAddItem(item.type));
            if (hasItem)
            {
                //Debug.Log("기존 아이템 레벨업 : " + item.itemName);
                //기존 아이템 레벨업
                if (GetItemLevel(itemName) >= item.maxLevel) return false;
                itemLevelDict[itemName]++;
            }
            else
            {
                //Debug.Log("새로운 아이템 추가 : " + item.itemName);
                //새로운 아이템 추가
                if (!CanAddItem(item.type)) return false;
                AddNewItem(item);
                itemLevelDict[itemName] = 1;
            }
            SyncItemData();
            ShopManagerTest.Instance.VanishMaxLevelItem(item);
            return true;
        }

        //아이템 추가 가능 여부 확인
        public bool CanAddItem(TestItem.ItemType itemType)
        {
            return GetCurrentItemCount(itemType) < GetMaxItemCount(itemType);
        }

        //현재 아이템 개수 조회
        public int GetCurrentItemCount(TestItem.ItemType itemType)
        {
            switch (itemType)
            {
                case TestItem.ItemType.MeleeWeapon:
                    return meleeWeapon != null ? 1 : 0;
                case TestItem.ItemType.RangedWeapon:
                    return subWeapons.Count;
                case TestItem.ItemType.SplashWeapon:
                    return subWeapons.Count;
                case TestItem.ItemType.Passive:
                    return passives.Count;
                default:
                    return 0;
            }
        }

        //최대 아이템 개수 조회
        public int GetMaxItemCount(TestItem.ItemType itemType)
        {
            return maxItemCounts.TryGetValue(itemType, out int count) ? count : 0;
        }

        //새로운 아이템 추가
        private void AddNewItem(TestItem item)
        {
            //Debug.Log(item.type);
            switch (item.type)
            {
                case TestItem.ItemType.MeleeWeapon:
                    meleeWeapon = item;
                    break;
                case TestItem.ItemType.RangedWeapon:
                    subWeapons.Add(item);
                    break;
                case TestItem.ItemType.SplashWeapon:
                    subWeapons.Add(item);
                    break;
                case TestItem.ItemType.Passive:
                    passives.Add(item);
                    break;
            }
        }

        //아이템 레벨 동기화
        public void SyncItemData()
        {
            //아이템 레벨 정보를 실제 아이템에 동기화
            if (meleeWeapon != null)
            {
                meleeWeapon.SetWeaponStats(GetItemLevel(meleeWeapon.itemName));
            }
            foreach (var subWeapon in subWeapons)
            {
                subWeapon.SetWeaponStats(GetItemLevel(subWeapon.itemName));
            }
            foreach (var passive in passives)
            {
                passive.SetWeaponStats(GetItemLevel(passive.itemName));
            }
        }
        #endregion
    }
}