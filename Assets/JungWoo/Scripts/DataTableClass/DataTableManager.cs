using System.IO;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

namespace DiceSurvivor.Manager
{
    /// <summary>
    /// 무기 / 패시브  Data 싱글톤 
    /// </summary>
    public class DataTableManager : SingletonManager<DataTableManager>
    {
        [SerializeField] private TextAsset jsonFile; // Inspector에서 JSON 파일을 할당
        private DataTable dataTable;
        //private PassiveTable passiveTable;

        public TextAsset GetJson {
            get { return jsonFile; }
        }
        public DataTable GetDT
        {
            get { return dataTable; }
        }

        protected override void Awake()
        {
            base.Awake();
            LoadDataTable();
        }

        private void LoadDataTable()
        {
            if (jsonFile != null)
            {
                //Debug.Log($"JSON 파일 로드 시작: {jsonFile.name}");
                //Debug.Log($"JSON 내용 첫 500자: {jsonFile.text.Substring(0, Mathf.Min(500, jsonFile.text.Length))}");

                dataTable = JsonDataParser.ParseJsonToDataTable(jsonFile.text);

                //로드된 데이터 확인
                VerifyLoadedData();
                //PrintAllWeapons();
            }
            else
            {
                Debug.LogError("JSON 파일이 할당되지 않았습니다. Inspector에서 JSON 파일을 할당해주세요.");
            }
        }

        /// <summary>
        /// 로드된 데이터 검증
        /// </summary>
        private void VerifyLoadedData()
        {
            if (dataTable == null)
            {
                Debug.LogError("DataTable이 null입니다!");
                return;
            }

            if (dataTable.MeleeWeapons == null)
            {
                Debug.LogError("MeleeWeapons가 null입니다!");
                return;
            }

            // 근접 무기 목록 출력
            var meleeWeaponNames = dataTable.MeleeWeapons.GetWeaponNames();
            //Debug.Log($"로드된 근접 무기 수: {meleeWeaponNames.Count}");
            foreach (var weaponName in meleeWeaponNames)
            {
                //Debug.Log($"- {weaponName}");

                // 각 레벨 확인
                var weaponLevels = dataTable.MeleeWeapons.GetAllLevelsOfWeapon(weaponName);
                if (weaponLevels != null)
                {
                    //Debug.Log($"  레벨 수: {weaponLevels.Count}");
                }
            }

            //Debug.Log("데이터 테이블 로드 및 검증 완료");
        }

        public DataTable GetDataTable()
        {
            return dataTable;
        }

        /// <summary>
        /// 근접 무기 데이터 가져오기 (디버그 정보 추가)
        /// </summary>
        public WeaponStats GetMeleeWeapon(string weaponName, int level)
        {
            //Debug.Log($"GetMeleeWeapon 호출 - 무기: {weaponName}, 레벨: {level}");

            if (dataTable == null)
            {
                Debug.LogError("DataTable이 null입니다!");
                return null;
            }

            if (dataTable.MeleeWeapons == null)
            {
                Debug.LogError("MeleeWeapons가 null입니다!");
                return null;
            }

            var weapon = dataTable.MeleeWeapons.GetWeapon(weaponName, level);

            if (weapon == null)
            {
                Debug.LogError($"{weaponName} Lv.{level} 데이터를 찾을 수 없습니다!");

                // 사용 가능한 무기 목록 출력
                var availableWeapons = dataTable.MeleeWeapons.GetWeaponNames();
                //Debug.Log($"사용 가능한 무기: {string.Join(", ", availableWeapons)}");
            }
            else
            {
                //Debug.Log($"{weaponName} Lv.{level} 데이터 찾음 - 데미지: {weapon.damage}, 쿨다운: {weapon.cooldown}");
            }

            return weapon;
        }

        public WeaponStats GetRangedWeapon(string weaponName, int level)
        {
            return dataTable?.RangedWeapons?.GetWeapon(weaponName, level);
        }

        public WeaponStats GetSplashWeapon(string weaponName, int level)
        {
            return dataTable?.SplashWeapons?.GetWeapon(weaponName, level);
        }

        public PassiveSkill GetStatPassive(string passiveName,int level)
        {
            return dataTable?.StatPassives?.GetPassive(passiveName,level);
        }

        public PassiveSkill GetRevivePassive(string passiveName, int level)
        {
            return dataTable?.RevivePassives?.GetPassive(passiveName, level);
        }

        /// <summary>
        /// 디버그용 - 모든 무기 정보 출력
        /// </summary>
        [ContextMenu("Print All Weapons")]
        public void PrintAllWeapons()
        {
            if (dataTable == null)
            {
                Debug.LogError("DataTable이 로드되지 않았습니다!");
                return;
            }

            //Debug.Log("=== 모든 무기 정보 ===");

            // 근접 무기
            //Debug.Log("--- 근접 무기 ---");
            foreach (var weaponName in dataTable.MeleeWeapons.GetWeaponNames())
            {
                var levels = dataTable.MeleeWeapons.GetAllLevelsOfWeapon(weaponName);
                //Debug.Log($"{weaponName}: {levels.Count}개 레벨");
            }

            // 원거리 무기
            //Debug.Log("--- 원거리 무기 ---");
            foreach (var weaponName in dataTable.RangedWeapons.GetWeaponNames())
            {
                var levels = dataTable.RangedWeapons.GetAllLevelsOfWeapon(weaponName);
                //Debug.Log($"{weaponName}: {levels.Count}개 레벨");
            }

            // 범위 무기
            //Debug.Log("--- 범위 무기 ---");
            foreach (var weaponName in dataTable.SplashWeapons.GetWeaponNames())
            {
                var levels = dataTable.SplashWeapons.GetAllLevelsOfWeapon(weaponName);
                //Debug.Log($"{weaponName}: {levels.Count}개 레벨");
            }

            // 패시브 아이템
            //Debug.Log("--- 스텟 패시브 아이템  ---");
            foreach (var passiveName in dataTable.StatPassives.GetPassiveNames())
            {
                var levels = dataTable.StatPassives.GetAllLevelsOfPassive(passiveName);
                //Debug.Log($"{passiveName}: {levels.Count}개 레벨");
            }

            // 패시브 아이템
            //Debug.Log("---Revive 패시브 아이템  ---");
            foreach (var passiveName in dataTable.RevivePassives.GetPassiveNames())
            {
                var levels = dataTable.RevivePassives.GetAllLevelsOfPassive(passiveName);
                //Debug.Log($"{passiveName}: {levels.Count}개 레벨");
            }
        }
    }
}
