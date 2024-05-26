using UnityEngine;
using VLCNP.Stats;
using UnityEngine.Events;
using VLCNP.Saving;
using VLCNP.Combat;
using Newtonsoft.Json.Linq;
using System;
using VLCNP.Core;

namespace VLCNP.Attributes
{
    public class Health : MonoBehaviour, IJsonSaveable, IStoppable
    {
        float healthPoints = -1f;
        // 無敵時間
        [SerializeField] float invincibleTime = 3f;
        [SerializeField] GameObject deadEffect = null;
        [SerializeField] public UnityEvent<float> takeDamage;
        [SerializeField] public UnityEvent<GameObject> dieEvent;
        public event Action onDie;

        bool isDead = false;

        public bool IsDead { get => isDead; }
        private bool isStopped = false;
        public bool IsStopped { get => isStopped; set => isStopped = value; }

        float timeSinceLastHit = Mathf.Infinity;
        SpriteRenderer playerSprite;
        TakeDamageSe takeDamageSe;
        [SerializeField] AudioClip zeroDamageSe = null;
        // ダメージを受けたときにふっとばすかどうか
        [SerializeField] bool isBlowAway = false;
        [SerializeField] float guardPower = 0f;

        private void Awake() {
            healthPoints = GetComponent<BaseStats>().GetStat(Stat.Health);
            playerSprite = GetComponent<SpriteRenderer>();
            takeDamageSe = GetComponent<TakeDamageSe>();
        }

        private void Update() {
            timeSinceLastHit += Time.deltaTime;
        }

        public void TakeDamage(float damage)
        {
            if (isStopped) return;
            if (isDead) return;
            if (IsInvincible()) return;
            timeSinceLastHit = 0f;
            float _damage = damage - guardPower;
            if (_damage <= 0)
            {
                AudioSource audioSource = GetComponent<AudioSource>();
                if (audioSource) audioSource.PlayOneShot(zeroDamageSe, 2.5f);
                takeDamage?.Invoke(_damage);
                return;
            }
            healthPoints = Mathf.Max(healthPoints - _damage, 0);
            takeDamageSe?.Play();
            takeDamage.Invoke(_damage);
            if (healthPoints == 0) {
                Die();
            } else {
                // 吹っ飛ばす
                Rigidbody2D rBody = GetComponent<Rigidbody2D>();
                rBody.AddForce(new Vector2(rBody.velocity.x, 3), ForceMode2D.Impulse);
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
            if (dieEvent.GetPersistentEventCount() > 0)
            {
                isDead = true;
                dieEvent.Invoke(gameObject);
                return;
            }
            DeadEffectAndDestroy();
        }

        public void DeadEffectAndDestroy()
        {
            GameObject _deadEffect = Instantiate(deadEffect, transform.position, Quaternion.identity);
            Destroy(_deadEffect, 2f);
            isDead = true;
            onDie?.Invoke();
            Destroy(gameObject);
        }

        public float GetHealthPoints()
        {
            return healthPoints;
        }

        public void SetHealthPoints(float healthPoints)
        {
            this.healthPoints = healthPoints;
        }

        // 全回復させるメソッド
        public void RestoreHealth()
        {
            healthPoints = GetComponent<BaseStats>().GetStat(Stat.Health);
            SetHealthPoints(healthPoints);
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
