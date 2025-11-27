using UnityEngine;
using UnityEngine.UI;

namespace DiceSurvivor.Player
{
    public class PlayerExperience : MonoBehaviour
    {
        [Header("UI 연결")]
        public ProgressBar expBar;   // ProgressBar 컴포넌트
        public Text expText;         // "000/000  00%" 형식 텍스트
        public Text levelText;       // "Lv. 1" 표시 텍스트

        [Header("경험치 관련")]
        public float currentExp = 0f;       // 현재 경험치
        public float expToLevelUp = 100f;   // 레벨업 필요 경험치
        public int level = 1;               // 현재 레벨

        [Header("레벨업 증가율")]
        public float expGrowthRate = 1.1f;  // 레벨업할 때마다 필요 경험치 증가 비율

        private void Start()
        {
            expBar.Title = "EXP";
            expBar.BarValue = 0f;
            UpdateExpText();
            UpdateLevelText();
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.K))
            {
                GainExp(40);
            }
        }
        // 경험치 획득
        public void GainExp(float amount)
        {
            currentExp += amount;

            // 레벨업 체크
            while (currentExp >= expToLevelUp)
            {
                currentExp -= expToLevelUp;
                LevelUp();
            }

            // 바 업데이트
            float expPercent = (currentExp / expToLevelUp) * 100f;
            expBar.BarValue = expPercent;

            // UI 갱신
            UpdateExpText();
        }

        // 레벨업 처리
        void LevelUp()
        {
            level++;
            expToLevelUp = Mathf.Ceil(expToLevelUp * expGrowthRate); // 올림 처리
            UpdateLevelText();
            //Debug.Log($"레벨업! 현재 레벨: {level}");
            DiceSurvivor.Manager.ShopManagerTest smt = DiceSurvivor.Manager.ShopManagerTest.Instance;
            if (smt.CheckCond())
            {
                smt.OpenShop();
            }
        }

        // 경험치 텍스트 갱신 ("000/000  00%" 형식)
        void UpdateExpText()
        {
            if (expText != null)
            {
                int cur = Mathf.FloorToInt(currentExp);
                int max = Mathf.FloorToInt(expToLevelUp);
                float percent = (currentExp / expToLevelUp) * 100f;

                expText.text = $"{cur:000}/{max:000}  ({percent:00.00}%)";
            }
        }

        // 레벨 텍스트 갱신 ("Lv. 1")
        void UpdateLevelText()
        {
            if (levelText != null)
            {
                levelText.text = $"Lv. {level}";
            }
        }
    }
}