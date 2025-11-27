using UnityEngine;
using DiceSurvivor.Enemy;

namespace DiceSurvivor.Weapon
{
    public class EffectSlow : MonoBehaviour
    {
        #region Variables
        #endregion

        #region Properties
        #endregion

        #region Unity Event Methods
        #endregion

        #region Custom Methods

        //EffectSlow.ApplySlow()
        public static void ApplySlow(GameObject enemy, float slowDuration)
        {
            if (enemy == null || slowDuration <= 0) return;

            var movement = enemy.GetComponent<IMovement>();
            if (movement != null)
            {
                movement.ApplySlow(0.5f, slowDuration); // 50% 감속
            }
        }
        #endregion
    }

}
