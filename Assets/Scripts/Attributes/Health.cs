using UnityEngine;
using VLCNP.Stats;
using UnityEngine.Events;
using VLCNP.Saving;
using Newtonsoft.Json.Linq;
using System;

namespace VLCNP.Attributes
{
    public class Health : MonoBehaviour, IJsonSaveable
    {
        float healthPoints = -1f;
        // 無敵時間
        [SerializeField] float invincibleTime = 3f;
        [SerializeField] GameObject deadEffect = null;
        [SerializeField] public UnityEvent<float> takeDamage;
        public event Action onDie;

        bool isDead = false;

        public bool IsDead { get => isDead; }

        float timeSinceLastHit = Mathf.Infinity;
        SpriteRenderer playerSprite;

        private void Awake() {
            healthPoints = GetComponent<BaseStats>().GetStat(Stat.Health);
            playerSprite = GetComponent<SpriteRenderer>();
        }

        private void Update() {
            timeSinceLastHit += Time.deltaTime;
        }

        public void TakeDamage(float damage)
        {
            if (isDead) return;
            if (IsInvincible()) return;
            timeSinceLastHit = 0f;
            healthPoints = Mathf.Max(healthPoints - damage, 0);
            takeDamage.Invoke(damage);
            if (healthPoints == 0) {
                Die();
            } else {
                // 吹っ飛ばす
                GetComponent<Rigidbody2D>().AddForce(Vector2.up * 4, ForceMode2D.Impulse);
            }
        }

        private void FixedUpdate()
        {
            FlashingIfDamaged();
        }

        private void FlashingIfDamaged()
        {
            if (IsInvincible())
            {
                SpriteRenderer playerSprite = GetComponent<SpriteRenderer>();
                float level = Mathf.Abs(Mathf.Sin(Time.time * 10));
                playerSprite.color = new Color(1f, 1f, 1f, level);
            }
            else
            {
                playerSprite.color = new Color(1f, 1f, 1f, 1);
            }
        }

        private bool IsInvincible()
        {
            return timeSinceLastHit < invincibleTime;
        }

        private void Die()
        {
            if (isDead) return;
            Instantiate(deadEffect, transform.position, Quaternion.identity);
            isDead = true;
            onDie?.Invoke();
            gameObject.SetActive(false);
        }

        public float GetHealthPoints()
        {
            return healthPoints;
        }

        public void SetHealthPointsFromOther(Health other)
        {
            healthPoints = other.GetHealthPoints();
        }

        public void SetHealthPoints(float healthPoints)
        {
            this.healthPoints = healthPoints;
        }

        public JToken CaptureAsJToken()
        {
            return JToken.FromObject(healthPoints);
        }

        public void RestoreFromJToken(JToken state)
        {
            healthPoints = state.ToObject<float>();
            if (healthPoints == 0)
            {
                Die();
            }
        }
    }    
}
