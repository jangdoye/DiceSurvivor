using UnityEngine;

namespace DiceSurvivor.Weapon
{
    public class GreatswordAttack : MeleeWeaponBase
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
            if(parentTransform != null)
            {
                if(Weapon.range == 6)
                {
                    parentTransform.localScale = new Vector3(parentTransform.localScale.x + 80, parentTransform.localScale.y + 80, parentTransform.localScale.z + 80);
                }
                else if(Weapon.range == 7)
                {
                    parentTransform.localScale = new Vector3(parentTransform.localScale.x + 80 * 2, parentTransform.localScale.y + 80 * 2, parentTransform.localScale.z + 80 * 2);
                }
                else if(Weapon.range == 8)
                {
                    parentTransform.localScale = new Vector3(parentTransform.localScale.x + 80 * 3, parentTransform.localScale.y + 80 * 3, parentTransform.localScale.z + 80 * 3);
                }
                else if(Weapon.range == 9)
                {
                    parentTransform.localScale = new Vector3(parentTransform.localScale.x + 80 * 4, parentTransform.localScale.y + 80 * 4, parentTransform.localScale.z + 80 * 4);
                }
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
