using System;
using System.Collections;
using Fungus;
using MoonSharp.VsCodeDebugger.SDK;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using VLCNP.Combat;
using VLCNP.Core;
using VLCNP.Saving;
using VLCNP.Stats;

namespace VLCNP.Attributes
{
    public class Health : MonoBehaviour, IStoppable
    {
        float healthPoints = -1f;

        // 無敵時間
        [SerializeField]
        float invincibleTime = 3f;

        [SerializeField]
        GameObject deadEffect = null;

        [SerializeField]
        public UnityEvent<float> takeDamage;

        [SerializeField]
        public UnityEvent<GameObject> dieEvent;
        public event Action onDie;

        [SerializeField]
        public bool IsGameOverEventExecute = false;

        // 一時的な無敵状態かどうか
        private bool isTempInvincible = false;
        public bool IsTempInvincible
        {
            get => isTempInvincible;
            set => isTempInvincible = value;
        }

        bool isDead = false;

        public bool IsDead
        {
            get => isDead;
        }
        private bool isStopped = false;
        public bool IsStopped
        {
            get => isStopped;
            set => isStopped = value;
        }

        float timeSinceLastHit = Mathf.Infinity;
        public float TimeSinceLastHit
        {
            get => timeSinceLastHit;
        }
        SpriteRenderer playerSprite;
        TakeDamageSe takeDamageSe;

        [SerializeField]
        AudioClip zeroDamageSe = null;

        // ダメージを受けたときにふっとばすかどうか
        [SerializeField]
        bool isBlowAway = false;

        [SerializeField]
        float guardPower = 0f;
        private DamageStun damageStun = null;
        AudioSource audioSource;

        private void Awake()
        {
            healthPoints = GetComponent<BaseStats>().GetStat(Stat.Health);
            playerSprite = GetComponent<SpriteRenderer>();
            takeDamageSe = GetComponent<TakeDamageSe>();
            damageStun = GetComponent<DamageStun>();
            audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            timeSinceLastHit += Time.deltaTime;
        }

        public void TakeDamage(float damage, bool isBlowAwayDirectionLeft = false)
        {
            if (isTempInvincible)
                return;
            if (isStopped)
                return;
            if (isDead)
                return;
            if (IsInvincible())
                return;
            timeSinceLastHit = 0f;
            float _damage = damage - guardPower;
            if (_damage <= 0)
            {
                AudioSource audioSource = GetComponent<AudioSource>();
                if (audioSource)
                    audioSource.PlayOneShot(zeroDamageSe, 0.5f);
                takeDamage?.Invoke(_damage);
                return;
            }
            healthPoints = Mathf.Max(healthPoints - _damage, 0);
            takeDamageSe?.Play();
            takeDamage.Invoke(_damage);
            if (damageStun != null)
                damageStun.Stun();
            if (healthPoints == 0)
            {
                Die();
            }
            else
            {
                // 吹っ飛ばす
                Rigidbody2D rBody = GetComponent<Rigidbody2D>();
                if (isBlowAway)
                {
                    rBody.AddForce(
                        new Vector2(isBlowAwayDirectionLeft ? -6 : 6, 3),
                        ForceMode2D.Impulse
                    );
                }
                else
                {
                    rBody.AddForce(new Vector2(0, 3), ForceMode2D.Impulse);
                }
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

        public void InheritInvincible(Health health)
        {
            timeSinceLastHit = health.TimeSinceLastHit;
        }

        private void Die()
        {
            if (isDead)
                return;
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
            isDead = true;
            StartCoroutine(ExecuteGameOverEvent());
        }

        public IEnumerator ExecuteGameOverEvent()
        {
            if (IsGameOverEventExecute)
            {
                // ヒットストップ
                Time.timeScale = 0.001f;
                yield return new WaitForSecondsRealtime(1f);
                Time.timeScale = 1;
            }

            GameObject _deadEffect = Instantiate(
                deadEffect,
                transform.position,
                Quaternion.identity
            );
            Destroy(_deadEffect, 2f);
            onDie?.Invoke();
            if (IsGameOverEventExecute)
            {
                FindObjectOfType<GameOver>()?.Execute();
                gameObject.SetActive(false);
                yield return null;
            }
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

        public void RestoreHealthBy(float amount, AudioClip se = null, float seVolume = 0.3f)
        {
            healthPoints = Mathf.Min(
                healthPoints + amount,
                GetComponent<BaseStats>().GetStat(Stat.Health)
            );
            if (audioSource != null && se != null)
            {
                audioSource.PlayOneShot(se, seVolume);
            }
        }
    }
}
