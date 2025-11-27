using UnityEngine;

namespace DiceSurvivor.Type
{
    public enum SkillType
    {
        None = -1,
        Weapon = 0,
        Passive = 1
    }

    public enum WeaponType
    {
        MeleeWeapon,
        RangedWeapon,
        SplashWeapon
    }

    public enum MeleeWeaponType
    {
        Hammer,
        GreatSword,
        Spear,
        Staff,
        Whip,
        Scythe
    }

    public enum RangedWeaponType
    {
        Boomerang,
        Fireball,
        Dagger,
        PoisonFlask,
        Laser
    }

    public enum SplashWeaponType
    {
        KillingAura,
        Icicle,
        LightningStaff,
        Asteroid
    }
}