using System;
using System.Collections;
using UnityEngine;
using VLCNP.Attributes;

namespace VLCNP.Stats
{
    public class EnemyPoisonStatus : MonoBehaviour
    {
        [SerializeField]
        float poisonDuration = 5f;
        
        [SerializeField]
        float poisonDamage = 1f;
        
        [SerializeField]
        float poisonInterval = 1f;
        
        float remainingTime = 0f;
        bool isPoisoned = false;
        Health health;
        
        public event Action OnPoisonStarted;
        public event Action OnPoisonCured;
        
        public bool IsPoisoned
        {
            get { return isPoisoned; }
        }
        
        void Awake()
        {
            health = GetComponent<Health>();
            if (health == null)
            {
                Debug.LogError($"Health component not found on {gameObject.name}");
            }
        }
        
        void Update()
        {
            if (!isPoisoned)
                return;
                
            remainingTime -= Time.deltaTime;
            if (remainingTime <= 0)
            {
                Cure();
            }
        }
        
        public void ActivatePoison(float duration = 0f, float damage = 0f, float interval = 0f)
        {
            if (isPoisoned)
                return;
                
            if (duration > 0) poisonDuration = duration;
            if (damage > 0) poisonDamage = damage;
            if (interval > 0) poisonInterval = interval;
            
            isPoisoned = true;
            remainingTime = poisonDuration;
            
            StartCoroutine(ApplyPoisonDamage());
            
            Debug.Log($"{gameObject.name} is poisoned for {poisonDuration} seconds.");
            OnPoisonStarted?.Invoke();
        }
        
        private IEnumerator ApplyPoisonDamage()
        {
            while (isPoisoned && health != null)
            {
                yield return new WaitForSeconds(poisonInterval);
                
                if (!isPoisoned || health.IsDead)
                    break;
                
                float currentHealth = health.GetHealthPoints();
                if (currentHealth > 1)
                {
                    float damageToApply = Mathf.Min(poisonDamage, currentHealth - 1);
                    health.SetHealthPoints(currentHealth - damageToApply);
                    Debug.Log($"{gameObject.name} took {damageToApply} poison damage. HP: {health.GetHealthPoints()}");
                }
            }
        }
        
        public void Cure()
        {
            if (!isPoisoned)
                return;
                
            isPoisoned = false;
            remainingTime = 0;
            StopAllCoroutines();
            
            Debug.Log($"{gameObject.name} is cured from poison.");
            OnPoisonCured?.Invoke();
        }
        
        void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}