using System;
using System.Collections.Generic;
using UnityEngine;

namespace DiceSurvivor.Manager
{
    /// <summary>
    /// Json 파일에 있는 weapon, passive 속성 참조 및 가져오기
    /// </summary>
    [Serializable]
    public class WeaponStats
    {
        public string id;
        public string name;
        public string type;
        public int level;
        public int damage;
        public float cooldown;
        public float radius;
        public float range;
        public float projectileSize;
        public float projectileSpeed;
        public int projectileCount;
        public float explosionRadius;
        public float explosionDamage;
        public float dotDamage;
        public float duration;
        public bool isPiercing;
        public bool canReturn;
        public string description;
    }

    [Serializable]
    public class PassiveSkill
    {
        public string id;
        public string name;
        public string type;
        public int level;
        public string effect;
        public float? value;
        public string unit;
        public float cooldown;
        public string evolutionFor;
        public bool isConsumable;
        public string description;
    }

    // 무기 범주별 데이터 구조
    [Serializable]
    public class WeaponCategory
    {
        private Dictionary<string, Dictionary<int, WeaponStats>> weaponData;

        public WeaponCategory()
        {
            weaponData = new Dictionary<string, Dictionary<int, WeaponStats>>();
        }

        public void AddWeapon(string weaponName, int level, WeaponStats stats)
        {
            if (!weaponData.ContainsKey(weaponName))
            {
                weaponData[weaponName] = new Dictionary<int, WeaponStats>();
            }
            weaponData[weaponName][level] = stats;
        }

        public WeaponStats GetWeapon(string weaponName, int level)
        {
            if (weaponData.ContainsKey(weaponName) && weaponData[weaponName].ContainsKey(level))
            {
                return weaponData[weaponName][level];
            }
            return null;
        }

        public Dictionary<int, WeaponStats> GetAllLevelsOfWeapon(string weaponName)
        {
            return weaponData.ContainsKey(weaponName) ? weaponData[weaponName] : null;
        }

        public List<string> GetWeaponNames()
        {
            return new List<string>(weaponData.Keys);
        }
    }

    // 패시브 범주별 데이터 구조
    [Serializable]
    public class PassiveCategory
    {
        private Dictionary<string, Dictionary<int, PassiveSkill>> passiveData;

        public PassiveCategory()
        {
            passiveData = new Dictionary<string, Dictionary<int, PassiveSkill>>();
        }

        public void AddPassive(string passiveName, int level, PassiveSkill passive)
        {
            if (!passiveData.ContainsKey(passiveName))
            {
                passiveData[passiveName] = new Dictionary<int, PassiveSkill>();
            }
            passiveData[passiveName][level] = passive;
        }

        public PassiveSkill GetPassive(string passiveName, int level)
        {
            if (passiveData.ContainsKey(passiveName) && passiveData[passiveName].ContainsKey(level))
            {
                return passiveData[passiveName][level];
            }

            return null;
        }

        public Dictionary<int, PassiveSkill> GetAllLevelsOfPassive(string passiveName)
        {
            return passiveData.ContainsKey(passiveName) ? passiveData[passiveName] : null;
        }

        public List<string> GetPassiveNames()
        {
            return new List<string>(passiveData.Keys);
        }
    }

    // 메인 데이터 테이블
    public class DataTable
    {
        public WeaponCategory MeleeWeapons { get; private set; }
        public WeaponCategory RangedWeapons { get; private set; }
        public WeaponCategory SplashWeapons { get; private set; }

        public PassiveCategory StatPassives { get; private set; }
        public PassiveCategory ActivePassives { get; private set; }
        public PassiveCategory RevivePassives { get; private set; }

        public DataTable()
        {
            MeleeWeapons = new WeaponCategory();
            RangedWeapons = new WeaponCategory();
            SplashWeapons = new WeaponCategory();

            StatPassives = new PassiveCategory();
            ActivePassives = new PassiveCategory();
            RevivePassives = new PassiveCategory();
        }
    }
}