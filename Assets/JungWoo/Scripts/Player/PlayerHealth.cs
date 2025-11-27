using UnityEngine;

namespace DiceSurvivor.PlayerStats
{
    public class PlayerHealther : MonoBehaviour
    {
        public float maxHealth = 100f;
        private float currentHealth;

        public HPBar hpBarUI;

        private void Start()
        {
            currentHealth = maxHealth;
            if (hpBarUI != null)
            {
                hpBarUI.MaxValue = maxHealth;
                hpBarUI.CurrentValue = currentHealth;
            }
        }

        public void TakeDamage(float amount)
        {
            currentHealth -= amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            if (hpBarUI != null)
                hpBarUI.CurrentValue = currentHealth;
        }

        public void Heal(float amount)
        {
            currentHealth += amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            if (hpBarUI != null)
                hpBarUI.CurrentValue = currentHealth;
        }
    }
}