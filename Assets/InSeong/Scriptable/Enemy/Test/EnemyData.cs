using UnityEngine;

namespace DiceSurvivor.Enemy {
    /// <summary>
    /// EnemyData ScriptableObject + 기존 JSON 기반 EnemyStat 통합 버전 (구현 없이 메서드/필드와 주석만)
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        // 1. 적 유형 Enum (Normal/Elite/Boss/Summoned)
        public enum EnemyType
        {
            Normal = 0,
            Elite = 1,
            Boss = 2,
            Summoned = 3 // 소환수(보상 없음)
        }

        [Header("적 고유 정보")]
        public string enemyId;      // JSON id와 매칭
        public string enemyName;    // 적 이름(표시용)
        public GameObject body;     // 적 오브젝트 프리팹

        [Header("적 능력치")]
        public EnemyType type;      // 적 유형(일반/엘리트/보스/소환수)
        public float currHealth;    // 현재 체력
        public float maxHealth;     // 기본 체력
        public float damage;        // 공격력
        public float speed;         // 이동속도
        public float armor;         // 방어력

        [Header("처치 시 보상")]
        public float exp;           // 경험치 보상
        public int gold;          // 골드 보상
        public float expDropRate;   // 경험치 드랍 확률 (ex: 0.2 = 20%)
        public float goldDropRate;  // 골드 드랍 확률 (ex: 0.1 = 10%)
        public bool boxDropSwitch;  // 보상 상자 출현 여부

        [Header("추가 정보 (엘리트/보스 전용)")]
        public string eliteBuff;    // 강화 형태(예: "BigSize,FastSpeed")
        public string extraReward;  // 추가 보상(예: "Chest", "BossChest")

        [Header("계수 적용 관련 (동적 강화용)")]
        // 2. 시간/플레이어 레벨 등에 따라 적용되는 능력치 계수 값
        //    (적 유형별로 계수 JSON에서 받아서 사용)
        public float hpPerLevelMultiplier;
        public float hpPerTimeMultiplier;
        public float damagePerLevelMultiplier;
        public float damagePerTimeMultiplier;

        // 3. 메서드: JSON 데이터 기반으로 EnemyData에 값 적용
        public void ApplyJsonData(/*EnemyStat stat, EnemyStatMultiplier multiplier*/)
        {
            // stat의 기본 값과 multiplier의 계수 값을 EnemyData 필드에 적용
        }

        // 4. 메서드: 플레이어 레벨/시간에 따라 동적으로 능력치 갱신
        public void UpdateDynamicStats(/*int playerLevel, float elapsedTime*/)
        {
            // health, damage 등 능력치에 계수 적용하여 동적으로 갱신
        }

        public EnemyData GetCopy()
        {
            try
            {
                EnemyData copy = Instantiate(this);
                if (copy == null)
                {
                    Debug.LogError("Failed to create a copy of EnemyData.");
                    return null;
                }
                return copy;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error occurred while copying EnemyData: " + e.Message);
                return null;
            }
        }
    }
}
