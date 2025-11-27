using UnityEngine;

public class UsageExample : MonoBehaviour
{
    void Start()
    {
       /* // 사용 예시
        DataTable dataTable = DataTableManager.Instance.GetDataTable();
        
        // 1레벨 Scythe 무기 정보 가져오기
        WeaponStats scythe1 = dataTable.MeleeWeapons.GetWeapon("Scythe", 1);
        if (scythe1 != null)
        {
            Debug.Log($"Scythe Level 1 - Damage: {scythe1.damage}, Cooldown: {scythe1.cooldown}");
        }
        
        // 8레벨 Fireball 무기 정보 가져오기
        WeaponStats fireball8 = dataTable.RangedWeapons.GetWeapon("Fireball", 8);
        if (fireball8 != null)
        {
            Debug.Log($"Fireball Level 8 - Damage: {fireball8.damage}, Explosion Damage: {fireball8.explosionDamage}");
        }
        
        // Protein 패시브 정보 가져오기
        PassiveSkill protein = dataTable.StatPassives.GetPassive("Protein");
        if (protein != null)
        {
            Debug.Log($"Protein - Effect: {protein.effect}, Value: {protein.value}%");
        }
        
        // 특정 무기의 모든 레벨 정보 가져오기
        var allScytheLevels = dataTable.MeleeWeapons.GetAllLevelsOfWeapon("Scythe");
        if (allScytheLevels != null)
        {
            foreach (var level in allScytheLevels)
            {
                Debug.Log($"Scythe Level {level.Key} - Damage: {level.Value.damage}");
            }
        }*/
    }
}