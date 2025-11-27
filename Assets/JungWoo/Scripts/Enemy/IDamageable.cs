
namespace DiceSurvivor.Enemy
{
    // 인터페이스들
    public interface IDamageable
    {
        void TakeDamage(float damage);
    }

    public interface IMovement
    {
        void ApplySlow(float slowPercent, float duration);
    }
}