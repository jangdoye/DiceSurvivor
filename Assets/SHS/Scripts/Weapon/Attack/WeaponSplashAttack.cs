using DiceSurvivor.Weapon;
using UnityEngine;

namespace DiceSurvivor.Weapon
{
    public class WeaponSplashAttack : MeleeWeaponBase
    {
        #region Variables
        private SphereCollider sphereCollider;
        #endregion

        #region Properties
        #endregion

        #region Unity Event Methods
        protected override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);

            sphereCollider = this.GetComponent<SphereCollider>();
            Invoke("EnableCollider", 0.5f);
        }
        #endregion

        #region Custom Methods
        private void EnableCollider()
        {
            sphereCollider.enabled = false;
        }
        #endregion
    }
}
