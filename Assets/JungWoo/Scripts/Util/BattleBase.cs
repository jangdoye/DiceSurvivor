using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace DiceSurvivor.Battle
{
    /// <summary>
    /// 캐릭터의 Health/ Damage를 관리하는 클래스
    /// 
    /// </summary>

    public class BattleBase : MonoBehaviour, IBattle
    {
        #region Variables
        //참조
        private float maxHealth = 100f;             //Max 체력

        public float invulnerablityTime = 2f;       //데미지를 받은후 무적 타임

        public bool isDeath;                        //죽음 처리
        #endregion

        #region Properties

        public float CurrentHealth { get; private set; }
        public bool IsInvulnerable { get; set; }
        #endregion

        #region Unity Event Methods
        private void Start()
        {
            ResetDamage();
            //StartStateMachine();
        }

        private void OnDestroy()
        {
            //StopStateMachine();
        }

        private void FixedUpdate()
        {
        }
        #endregion

        /*protected void StartStateMachine()
        {
            if (stateMachineCoroutine == null)
            {
                stateMachineCoroutine = StartCoroutine(StateMachineLoop());
            }
        }

        protected void StopStateMachine()
        {
            if (stateMachineCoroutine != null)
            {
                StopCoroutine(stateMachineCoroutine);
                stateMachineCoroutine = null;
            }
        }

        protected IEnumerator StateMachineLoop()
        {
            var updateInterval = new WaitForSeconds(stateUpdateInterval);

        }

        protected void ChangeState(CombatState newState)
        {
            if (currentState == newState) return;
        }*/

        protected virtual void HandleDeadState()
        {
            StopAllCoroutines();
            Die();
        }

        #region Custom Methods
        private void ResetDamage()
        {
            CurrentHealth = maxHealth;
            IsInvulnerable = false;
            isDeath = false;
        }

        public virtual void TakeDamage(float damage)
        {
            CurrentHealth -= damage;
            //Debug.Log($"Damage In: {damage}");

            if (CurrentHealth <= 0 && isDeath == false)
            {
                Die();
            }
        }

        public virtual void HealHealth(float healAmount)
        {
            CurrentHealth += healAmount;
            if (CurrentHealth >= maxHealth)
            {
                CurrentHealth = maxHealth;
            }

            if (isDeath == true)
            {
                return;
            }
        }

        public virtual void Die()
        {
            //ChangeState(CombatState.Dead);
            Destroy(gameObject);
        }

        public virtual bool IsInRange(GameObject target)
        {
            return false;
        }

        public virtual void Attack(GameObject target)
        {
        }
        #endregion
    }
}