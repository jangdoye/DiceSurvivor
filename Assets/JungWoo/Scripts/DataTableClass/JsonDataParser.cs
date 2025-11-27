using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DiceSurvivor.Manager
{
    public class JsonDataParser : MonoBehaviour
    {
        /// <summary>
        /// 현재 JSON 구조에 맞게 수정된 파서
        /// </summary>
        public static DataTable ParseJsonToDataTable(string jsonString)
        {
            DataTable dataTable = new DataTable();

            try
            {
                JObject rootObject = JObject.Parse(jsonString);

                // 직접 MeleeWeapons 파싱
                if (rootObject["MeleeWeapons"] != null)
                {
                    //Debug.Log("MeleeWeapons 발견");
                    ParseWeaponArray(rootObject["MeleeWeapons"] as JArray, dataTable.MeleeWeapons);
                }

                // RangedWeapons 파싱
                if (rootObject["RangedWeapons"] != null)
                {
                    //Debug.Log("RangedWeapons 발견");
                    ParseWeaponArray(rootObject["RangedWeapons"] as JArray, dataTable.RangedWeapons);
                }

                // SplashWeapons 파싱
                if (rootObject["SplashWeapons"] != null)
                {
                    //Debug.Log("SplashWeapons 발견");
                    ParseWeaponArray(rootObject["SplashWeapons"] as JArray, dataTable.SplashWeapons);
                }

                if (rootObject["StatPassive"] != null)
                {
                    ParsePassiveArray(rootObject["StatPassive"] as JArray, dataTable.StatPassives);
                }

                if (rootObject["RevivePassive"] != null)
                {
                    ParsePassiveArray(rootObject["RevivePassive"] as JArray, dataTable.RevivePassives);
                }
                // 파싱 결과 로그
                //Debug.Log($"파싱 완료 - 근접무기: {dataTable.MeleeWeapons.GetWeaponNames().Count}개");
                //Debug.Log($"근접무기 목록: {string.Join(", ", dataTable.MeleeWeapons.GetWeaponNames())}");
            }
            catch (Exception e)
            {
                Debug.LogError($"JSON 파싱 중 오류 발생: {e.Message}");
                Debug.LogError($"Stack Trace: {e.StackTrace}");
            }

            return dataTable;
        }

        /// <summary>
        /// 무기 배열 파싱 (현재 JSON 구조에 맞게 수정)
        /// </summary>
        private static void ParseWeaponArray(JArray weaponArray, WeaponCategory weaponCategory)
        {
            if (weaponArray == null)
            {
                Debug.LogError("weaponArray가 null입니다.");
                return;
            }

            // 배열의 첫 번째 요소가 무기들을 담은 객체
            foreach (JToken weaponGroupToken in weaponArray)
            {
                JObject weaponGroup = weaponGroupToken as JObject;
                if (weaponGroup == null) continue;

                // 각 무기 타입별로 처리
                foreach (var property in weaponGroup.Properties())
                {
                    string weaponName = property.Name;
                    JArray weaponLevels = property.Value as JArray;

                    //Debug.Log($"무기 발견: {weaponName}, 레벨 수: {weaponLevels?.Count ?? 0}");

                    if (weaponLevels != null)
                    {
                        foreach (JToken levelToken in weaponLevels)
                        {
                            try
                            {
                                WeaponStats stats = JsonConvert.DeserializeObject<WeaponStats>(levelToken.ToString());
                                if (stats != null)
                                {
                                    weaponCategory.AddWeapon(weaponName, stats.level, stats);
                                    //Debug.Log($"{weaponName} Lv.{stats.level} 추가됨 - 데미지: {stats.damage}");
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"{weaponName} 파싱 오류: {e.Message}");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 기존 메서드는 그대로 유지 (호환성)
        /// </summary>
        private static void ParsePassiveArray(JToken passiveArray, PassiveCategory passiveCategory)
        {
            if (passiveArray == null)
            {
                Debug.LogError("ParsePassiveArray null입니다.");
                return;
            }

            // 배열의 첫 번째 요소가 무기들을 담은 객체
            foreach (JToken passiveGroupToken in passiveArray)
            {
                JObject passiveGroup = passiveGroupToken as JObject;
                if (passiveGroup == null) continue;

                // 각 무기 타입별로 처리
                foreach (var property in passiveGroup.Properties())
                {
                    string passiveName = property.Name;
                    JArray passiveLevels = property.Value as JArray;

                    //Debug.Log($"무기 발견: {weaponName}, 레벨 수: {weaponLevels?.Count ?? 0}");

                    if (passiveLevels != null)
                    {
                        foreach (JToken levelToken in passiveLevels)
                        {
                            try
                            {
                                PassiveSkill skill = JsonConvert.DeserializeObject<PassiveSkill>(levelToken.ToString());
                                if (skill != null)
                                {
                                    passiveCategory.AddPassive(passiveName, skill.level, skill);
                                    //Debug.Log($"{weaponName} Lv.{stats.level} 추가됨 - 데미지: {stats.damage}");
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"{passiveName} 파싱 오류: {e.Message}");
                            }
                        }
                    }
                }
            }
        }
    }
}