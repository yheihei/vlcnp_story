using UnityEngine;
using VLCNP.Attributes;
using VLCNP.Stats;

namespace VLCNP.Combat
{
    public class PoisonProjectile : Projectile, IPoisonous
    {
        [Header("毒設定")]
        [SerializeField]
        bool isPoisonous = true;
        
        [SerializeField]
        float poisonDamage = 1f;
        
        [SerializeField]
        float poisonDuration = 5f;
        
        [SerializeField]
        float poisonInterval = 1f;
        
        public bool IsPoisonous => isPoisonous;
        
        public float GetPoisonDamage()
        {
            return poisonDamage;
        }
        
        public float GetPoisonDuration()
        {
            return poisonDuration;
        }
        
        public float GetPoisonInterval()
        {
            return poisonInterval;
        }
        
        protected override void OnTriggerEnter2D(Collider2D other)
        {
            base.OnTriggerEnter2D(other);
            
            if (!isPoisonous)
                return;
                
            if (other.gameObject.CompareTag(targetTagName))
            {
                EnemyPoisonStatus enemyPoison = other.gameObject.GetComponent<EnemyPoisonStatus>();
                if (enemyPoison != null && !enemyPoison.IsPoisoned)
                {
                    enemyPoison.ActivatePoison(poisonDuration, poisonDamage, poisonInterval);
                    Debug.Log($"Applied poison to {other.gameObject.name}");
                }
            }
        }
    }
}