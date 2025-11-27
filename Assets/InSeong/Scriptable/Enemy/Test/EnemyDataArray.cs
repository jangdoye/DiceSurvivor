using UnityEngine;

namespace DiceSurvivor.Enemy {
    [CreateAssetMenu(fileName = "EnemyDataArray", menuName = "Scriptable Objects/EnemyDataArray")]
    public class EnemyDataArray : ScriptableObject {
        public EnemyData[] enemyDataArray;
    }

}
