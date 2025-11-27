using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DiceSurvivor.Player;
using TMPro;
/*
Final TODO : 스프라이트 씌우고 교체하기
상점 열리게 하는거 ItemManager에서 하게 하기
*/
namespace DiceSurvivor.Manager {

    public class ShopManagerTest : SingletonManager<ShopManagerTest>
    {
        #region Variables
        //아이템 정보가 담긴 배열과 아이템 칸이 담긴 배열
        public TestItemArray testItems;
        public ItemSlot[] itemSlots;

        //현재 아이템 레벨 정보가 담긴 딕셔너리
        Dictionary<string, int> currItemLevelDict;

        //아이템 삭제 가능 횟수
        [SerializeField] int vanishChance = 5;
        //이번 상점에서 새로고침을 한 횟수 (구매 시/새 상점 열릴 때 초기화)
        int consecutiveRerollCount = 0;
        //리롤 가격
        int rerollCost = 10;

        //아이템의 가격
        Dictionary<TestItem.ItemType, int> itemCosts;

        //아이템 정보가 담긴 싱글톤
        ItemManager im;

        //JSON 파일을 통해 아이템 설명문을 불러옴
        DataTable dt;
        DataTableManager dtm;

        //상점 잠금 여부
        bool isShopLocked = false;

        //플레이어 참조
        PlayerController player;

        [Header("UI Elements")]
        public Button rerollButton;
        public Button lockButton;
        public Button vanishButton;
        public TextMeshProUGUI goldText;
        public TextMeshProUGUI shopLevelText;

        public Sprite closedLock;
        public Sprite openedLock;


        public ItemDetailDisplay detailDisplay;

        //아이템 구매/삭제 모드 전환
        public bool isBuyingMode = true;
        #endregion

        #region Properties
        #endregion

        #region Unity Event Methods
        protected override void Awake()
        {
            base.Awake();

            player = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<PlayerController>();
            //딕셔너리 초기화
            currItemLevelDict = new Dictionary<string, int>();
            itemCosts = new Dictionary<TestItem.ItemType, int> {
                { TestItem.ItemType.MeleeWeapon, 10 },
                { TestItem.ItemType.SubWeapon, 5 },
                { TestItem.ItemType.Passive, 3 }
            };
            im = ItemManager.Instance;
            dtm = DataTableManager.Instance;
            dt = DataTableManager.Instance.GetDataTable();
            //GoldWallet.Instance.RefreshUI();
            //전체 아이템 정보 설정하기
            foreach (var item in testItems.items)
            {
                item.SetWeaponStats(1);
            }

            gameObject.SetActive(false);    
        }

        private void OnEnable()
        {
            //플레이어 보유 금액에 따라 이자 지급;
            consecutiveRerollCount = 0;
            FillItem();
            GoldWallet.Instance.RefreshUI();
            GiveInterest();
            RefreshGold();
        }

        void OnDestroy()
        {
            foreach(TestItem item in testItems.items)
            {
                item.canIBuy = true;
            }
        }

        #endregion

        #region Custom Methods
        public bool CheckCond()
        {
            DiceSurvivor.Player.PlayerExperience pExp = player.GetComponent<PlayerExperience>();
            //상점은 플레이어 레벨 5마다 열림
            return pExp.level % 5 == 0;
        }
        public void OpenShop()
        {
            //상점 열기
            gameObject.SetActive(true);
            Time.timeScale = 0.01f;
        }

        public void CloseShop()
        {
            //상점 닫기
            gameObject.SetActive(false);
            Time.timeScale = 1f;
        }
        //구매 버튼 눌렀을 때 실행
        public void BuyItem(ItemSlot slot)
        {
            //해당 칸 아이템 정보를 플레이어 한테 넘김
            TestItem itemToBuy = slot.currentItem;
            if (itemToBuy == null) return;

            // 1. 금액 확인
            int itemCost = 0;
            //enum을 비트 형태로 만들어서 카테고리를 파악
            TestItem.ItemType category = (int)(itemToBuy.type & TestItem.ItemType.SubWeapon) != 0 ?
                            TestItem.ItemType.SubWeapon : itemToBuy.type;
            //Debug.Log(category);

            if (itemCosts.TryGetValue(category, out int cost))
            {
                //Debug.Log(cost);
                itemCost = cost;
            }
            else Debug.LogError("해당 아이템 구매 시도 시 오류 발생 : " + itemToBuy);

            if (GoldWallet.Instance.GetGold() < itemCost) return;

            // 2. 해당 아이템을 보유하고 있는 지 확인
            bool hasItem = im.GetItemLevel(itemToBuy.itemName) > 0;

            // 3-1. 보유한 아이템이라면 최대 레벨 이상인 지 확인
            if (hasItem)
            {
                if (im.GetItemLevel(itemToBuy.itemName) >= itemToBuy.maxLevel)
                {
                    VanishMaxLevelItem(itemToBuy);
                    return;
                }
            }
            else
            {
                //3-2. 보유하지 않은 아이템이라면 남은 자리가 있는 지 확인
                if (im.GetCurrentItemCount(itemToBuy.type) >= im.GetMaxItemCount(itemToBuy.type))
                {
                    return;
                }
            }

            // 4. 구매 처리
            GoldWallet.Instance.TrySpend(itemCost);
            im.BuyItem(itemToBuy);
            GoldWallet.Instance.RefreshUI();
            RefreshGold();

            //5. 아이템 칸 정보 갱신
            ClearItem(slot);

            //6. 리롤 가격 초기화
            consecutiveRerollCount = 0;
        }

        //상점 열림 또는 새로고침 버튼 누르면 실행
        void FillItem()
        {
            if (isShopLocked) return;
            //현재 상점에 있는 아이템 정보를 전부 지우고 새로 채움
            foreach (ItemSlot slot in itemSlots)
            {
                ClearItem(slot);
            }
            foreach (ItemSlot slot in itemSlots)
            {
                AddItem(slot);
            }
            //플레이어가 이미 최대 레벨에 도달한 아이템은 구매 불가능하게 만듦
            ClearAllUnused();
        }

        void AddItem(ItemSlot slot)
        {
            //아이템 칸에 무작위로 구매 가능한 아이템을 채움
            int randomIndex = Random.Range(0, testItems.items.Count);
            TestItem randomItem = testItems.items[randomIndex];

            while (!randomItem.canIBuy)
            {
                randomIndex = Random.Range(0, testItems.items.Count);
                randomItem = testItems.items[randomIndex];
            }

            slot.currentItem = randomItem;
            slot.changeInfo();
        }

        //버튼 리스너
        public void OnRerollButtonClicked()
        {
            int currRerollCost = rerollCost + (consecutiveRerollCount * 5);
            if (GoldWallet.Instance.GetGold() >= currRerollCost)
            {
                RerollShop();
                GoldWallet.Instance.TrySpend(currRerollCost);
                GoldWallet.Instance.RefreshUI();
                RefreshGold();
                consecutiveRerollCount++;
            }

        }
        //새로고침 했을 때 상점 아이템을 새로 채우기
        void RerollShop()
        {
            //잠금 여부와 상관 없이 아이템을 새로 채운다
            if (isShopLocked) ToggleLockShop();
            FillItem();
        }

        public void OnLockButtonClicked()
        {
            ToggleLockShop();
            //TODO : 상점 잠금 버튼 스프라이트 변경
        }
        //상점 잠금 및 잠금 해제
        void ToggleLockShop()
        {
            if (isShopLocked)
            {
                isShopLocked = false;
                lockButton.GetComponent<Image>().sprite = openedLock;
            }
            else
            {
                isShopLocked = true;
                lockButton.GetComponent<Image>().sprite = closedLock;
            }
        }

        public void OnVanishButtonClicked()
        {
            //TODO : 아이템 칸을 추가로 클릭했을 때 해당 칸의 아이템을 삭제
            /*
            1. 상점을 삭제 모드로 전환
            -> 각 아이템 슬롯을 전부 삭제 모드로 전환
            2. 아이템 슬롯의 버튼 클릭 이벤트 함수에 삭제 모드 시 수행할 행동을 추가
            */
            //구매 모드 일 경우 삭제 모드로 진입, 삭제 모드에서 다시 누르면 구매 모드로 전환 (삭제 취소)
            if (isBuyingMode) isBuyingMode = false;
            else
            {
                isBuyingMode = true;
                foreach (ItemSlot iSlot in itemSlots)
                {
                    iSlot.GetComponent<Image>().color = new Color(1, 1, 1, 0.25f); // 패널 색 변경
                }
                return;
            }

            //디테일 텍스트를 삭제 안내로 전환
            DetailTextChange();
            //각 아이템 슬롯의 패널 색을 빨갛게 변경
            foreach (ItemSlot slot in itemSlots)
            {
                slot.GetComponent<Image>().color = new Color(1, 0, 0, 0.25f);
            }
        }
        //아이템 삭제 모드 에서 디테일 텍스트 전환하기
        void DetailTextChange()
        {
            detailDisplay.gameObject.SetActive(true);
            if (!isBuyingMode)
            {

                detailDisplay.itemNameText.text = "";
                detailDisplay.itemLevelText.text = "";
                detailDisplay.itemDescriptionText.text = "삭제할 아이템을 클릭하세요. (삭제 모드 취소 : 삭제 버튼을 다시 클릭)";
            }
        }

        //아이템 영구 삭제 (게임당 가능한 횟수 정해져 있음)
        public void VanishItem(ItemSlot slot)
        {
            if (isBuyingMode) return;
            if (vanishChance <= 0)
            {
                return;
            }
            else
            {
                vanishChance--;
                ClearItem(slot);
                //삭제 모드 종료. 구매 모드로 전환
                isBuyingMode = true;
                detailDisplay.HideDetail();
                foreach (ItemSlot iSlot in itemSlots)
                {
                    iSlot.GetComponent<Image>().color = new Color(1, 1, 1, 0.25f); // 패널 색 변경
                }
            }

            if (vanishChance <= 0)
            {
                //버튼 비활성화
                vanishButton.interactable = false;
            }
        }

        //플레이어 아이템 레벨 정보 업데이트 (PlayerTest에서 호출)
        public void UpdatePlayerItemLevel(string itemName, int level)
        {
            if (currItemLevelDict.ContainsKey(itemName))
            {
                currItemLevelDict[itemName] = level;
            }
            else
            {
                currItemLevelDict.Add(itemName, level);
            }
        }

        //최대 레벨에 도달한 아이템 삭제
        public void VanishMaxLevelItem(TestItem item)
        {
            string itemName = item.itemName;
            int maxLevel = item.maxLevel;

            if (currItemLevelDict.TryGetValue(itemName, out int value))
            {
                //아이템 레벨이 최대 레벨에 도달했을 경우 더 이상 상점에 안나오게 만들기
                if (value >= maxLevel)
                {
                    item.canIBuy = false;
                }
                else return;
            }
            else return;
        }

        //진화 조건을 만족했을 경우 진화 아이템 상점 풀에 추가하기
        void AddEvolvedWeapon()
        {
            //구매 시 플레이어 아이템 칸과 상호작용하여 진화 무기로 교체
        }
        //아이템 칸 정보 지우기
        void ClearItem(ItemSlot slot)
        {
            if (!isBuyingMode) slot.currentItem.canIBuy = false;
            slot.currentItem = null;
            slot.changeInfo();
        }

        //플레이어 무기 칸이 최대치에 도달한 경우 갖고 있지 않은 아이템은 모두 삭제
        void ClearAllUnused()
        {
            foreach (var item in testItems.items)
            {
                if (!im.CanAddItem(item.type) && item.type != TestItem.ItemType.MeleeWeapon)
                {
                    item.canIBuy = false;
                }
            }
        }

        //아이템 설명 가져오기
        public string GetItemDescription(TestItem item)
        {
            //아이템 이름을 통해 json 파일에서 설명문을 가져옴
            if (dt == null)
            {
                Debug.LogError("DataTable이 null입니다!");
                return "아이템 설명을 불러올 수 없습니다.";
            }

            WeaponStats currWeapon = new WeaponStats();
            int currLevel = im.GetItemLevel(item.itemName) + 1;

            switch (item.type)
            {
                case TestItem.ItemType.MeleeWeapon:
                    currWeapon = dt.MeleeWeapons.GetWeapon(item.itemName, currLevel);
                    Debug.Log(currWeapon.description);
                    return currWeapon.description;
                case TestItem.ItemType.RangedWeapon:
                    currWeapon = dt.RangedWeapons.GetWeapon(item.itemName, currLevel);
                    Debug.Log(currWeapon.description);
                    return currWeapon.description;
                case TestItem.ItemType.SplashWeapon:
                    currWeapon = dt.SplashWeapons.GetWeapon(item.itemName, currLevel);
                    Debug.Log(currWeapon.description);
                    return currWeapon.description;
                default:
                    return "알 수 없는 아이템 타입입니다.";
            }
        }

        //골드 정보 갱신
        void RefreshGold()
        {
            //UI에 표시
            if (goldText != null)
            {
                goldText.text = GoldWallet.Instance.GetGold().ToString();
            }
        }

        void GiveInterest()
        {
            //플레이어 보유 금액에 따라 이자 지급
            int currGold = GoldWallet.Instance.GetGold();
            int interest = (int)Mathf.Min(100, currGold / 10);
            GoldWallet.Instance.Add(interest);
            Debug.Log("이자 지급 : " + interest);
        }
        #endregion
    }
}
