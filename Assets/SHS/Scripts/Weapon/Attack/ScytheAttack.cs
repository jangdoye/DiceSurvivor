using UnityEngine;

namespace DiceSurvivor.Weapon
{
    public class ScytheAttack : MeleeWeaponBase
    {
        #region Variables
        #endregion

        #region Properties
        #endregion

        #region Unity Event Methods
        protected override void Awake()
        {
            base.Awake();
        }
        private void Start()
        {
            Transform parentTransform = this.transform.parent;
            if (Weapon.range == 4)
            {
                parentTransform.localScale = new Vector3(parentTransform.localScale.x + 30, parentTransform.localScale.y + 30, parentTransform.localScale.z + 30);
            }
            else if (Weapon.range == 5)
            {
                parentTransform.localScale = new Vector3(parentTransform.localScale.x + 30 * 2, parentTransform.localScale.y + 30 * 2, parentTransform.localScale.z + 30 * 2);
            }
            else if (Weapon.range == 6)
            {
                parentTransform.localScale = new Vector3(parentTransform.localScale.x + 50 * 2, parentTransform.localScale.y + 50 * 2, parentTransform.localScale.z + 50 * 2);
            }
        }
        protected override void Update()
        {
            base.Update();
        }
        protected override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);
        }
        #endregion

        #region Custom Methods
        #endregion
    }

}
