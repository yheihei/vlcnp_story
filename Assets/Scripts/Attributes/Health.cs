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
using VLCNP.UI;

namespace VLCNP.Attributes
{
    /**
     * キャラクターの体力と死亡時の処理を管理する
     */
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

        [Header("ボス撃破時の制御")]
        [SerializeField]
        private Flag defeatedFlag = Flag.None;

        [SerializeField]
        private bool hideBossStatusOnDeath = false;

        [SerializeField]
        private AudioClip defeatedFlagSe = null;

        [SerializeField]
        [Range(0f, 1f)]
        private float defeatedFlagSeVolume = 0.5f;

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
                _damage = 0;
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

            isDead = true;
            HideBossStatusOnDeath();

            if (dieEvent.GetPersistentEventCount() > 0)
            {
                dieEvent.Invoke(gameObject);
                return;
            }
            DeadEffectAndDestroy();
        }

        private void HideBossStatusOnDeath()
        {
            if (!hideBossStatusOnDeath)
                return;

            if (!BossStatusVisibility.SetVisible(false))
            {
                Debug.LogWarning(
                    $"[{nameof(Health)}] BossStatus が見つからないため、非表示にできません。",
                    this
                );
            }
        }

        private void SetDefeatedFlag()
        {
            if (defeatedFlag == Flag.None)
                return;

            FlagManager flagManager = FlagManager.FindInScene();
            if (flagManager != null)
            {
                flagManager.SetFlag(defeatedFlag, true);
                PlayDefeatedFlagSe();
                return;
            }

            Debug.LogWarning(
                $"[{nameof(Health)}] FlagManager が見つからないため、{defeatedFlag} を設定できません。",
                this
            );
        }

        private void PlayDefeatedFlagSe()
        {
            if (defeatedFlagSe == null)
                return;

            AudioSource.PlayClipAtPoint(
                defeatedFlagSe,
                transform.position,
                defeatedFlagSeVolume
            );
        }

        // 落下ミスなど、無敵時間や停止状態に関係なく即死させる
        public void Kill()
        {
            if (isDead)
                return;
            healthPoints = 0;
            Die();
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

            if (deadEffect)
            {
                // 死亡エフェクトを生成
                GameObject _deadEffect = Instantiate(
                    deadEffect,
                    transform.position,
                    Quaternion.identity
                );
                Destroy(_deadEffect, 2f);
            }
            onDie?.Invoke();
            if (IsGameOverEventExecute)
            {
                FindObjectOfType<GameOver>()?.Execute();
                gameObject.SetActive(false);
                yield return null;
            }
            Destroy(gameObject);
            SetDefeatedFlag();
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
