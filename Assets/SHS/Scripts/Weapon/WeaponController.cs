using DiceSurvivor.Attack;
using DiceSurvivor.Audio;
using DiceSurvivor.Manager;
using DiceSurvivor.Type;
using UnityEngine;

namespace DiceSurvivor.Weapon
{
    public class WeaponController : MonoBehaviour
    {
        #region Variables
        //참조
        private Animator animator;
        private AttackEffectSpawn attackEffect;

        [Header("------Weapon-------")]
        [SerializeField] private string weaponName = "Hammer";
        [SerializeField] private int weaponLevel = 1;
        [SerializeField] private int maxLevel = 8;
        [SerializeField] private SfxType sfxType;
        [SerializeField] private WeaponType weaponType;

        private int currentLevel;

        [Header("------WeaponStat------")]
        [SerializeField] public WeaponStats currentWeaponStats;

        public event System.Action<WeaponStats> OnWeaponLoaded;
        #endregion

        #region Properties
        public string WeaponName { get { return weaponName; } }
        #endregion

        #region Unity Event Methods
        private void Awake()
        {
            animator = this.GetComponent<Animator>();
            attackEffect = this.GetComponentInChildren<AttackEffectSpawn>();
            this.gameObject.SetActive(false);
        }

        private void Start()
        {
            if (weaponType == WeaponType.MeleeWeapon)
            {
                LoadMeleeWeaponData();
            }
            else if (weaponType == WeaponType.RangedWeapon)
            {
                LoadRangedWeaponData();
            }
            else if (weaponType == WeaponType.SplashWeapon)
            {
                LoadSplashWeaponData();
            }

            currentLevel = weaponLevel;
        }
        private void Update()
        {
            if (weaponLevel != currentLevel)
            {
                if (currentWeaponStats.type == "Wp_Me")
                {
                    LoadMeleeWeaponData();
                }
                else if (currentWeaponStats.type == "Wp_Ra")
                {
                    LoadRangedWeaponData();
                }
                else if (currentWeaponStats.type == "Wp_Sp")
                {
                    LoadSplashWeaponData();
                }
                currentLevel = weaponLevel;
            }
        }

        private void OnEnable()
        {
            if (animator != null)
            {
                GetData();
            }
        }
        private void OnDisable()
        {
            if (animator != null)
            {
                animator.SetBool("IsAttack", false);
            }
        }
        #endregion

        #region Custom Methods
        public void LoadMeleeWeaponData()
        {
            currentWeaponStats = DataTableManager.Instance.GetMeleeWeapon(weaponName, weaponLevel);
            OnWeaponLoaded?.Invoke(currentWeaponStats);     //Weapon 값 넘겨주기
        }
        public void LoadRangedWeaponData()
        {
            currentWeaponStats = DataTableManager.Instance.GetRangedWeapon(weaponName, weaponLevel);
            OnWeaponLoaded?.Invoke(currentWeaponStats);     //Weapon 값 넘겨주기
        }
        public void LoadSplashWeaponData()
        {
            currentWeaponStats = DataTableManager.Instance.GetSplashWeapon(weaponName, weaponLevel);
            OnWeaponLoaded?.Invoke(currentWeaponStats);     //Weapon 값 넘겨주기
        }

        public void AttackEffectSpawn()
        {
            attackEffect.SpawnAttackEffect();
        }

        public void SpearAndStaffAttack()
        {
            this.GetComponentInChildren<SpearStaffAttack>().ExecuteAttack();
        }
        public void PlaySfx()
        {
            SfxManager.Instance.PlaySfx(sfxType);
        }

        public void GetData()
        {
            int dictLevel = ItemManager.Instance.GetItemLevel(weaponName);
                if (weaponLevel < dictLevel) weaponLevel = dictLevel;
                animator.SetBool("IsAttack", true); // 공격 애니메이션 트리거 실행
        }
        #endregion
    }

}
