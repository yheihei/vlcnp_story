using UnityEngine;
using VLCNP.Attributes;
using VLCNP.Control;
using VLCNP.Stats;

namespace VLCNP.UI
{
    /**
     * 操作キャラクターのHPが少ないとき、画面端の危険エフェクトを制御する。
     */
    [RequireComponent(typeof(CanvasGroup))]
    public class LowHealthScreenEffect : MonoBehaviour
    {
        [SerializeField]
        [Range(0f, 1f)]
        private float healthThreshold = 0.25f;

        [SerializeField]
        [Range(0f, 1f)]
        private float minimumAlpha = 0.22f;

        [SerializeField]
        [Range(0f, 1f)]
        private float maximumAlpha = 0.38f;

        [SerializeField]
        [Min(0.01f)]
        private float fadeDuration = 0.25f;

        [SerializeField]
        [Min(0.01f)]
        private float pulsePeriod = 1.2f;

        [SerializeField]
        [Min(1f)]
        private float criticalPulseSpeedScale = 1.6f;

        private CanvasGroup canvasGroup;
        private float effectIntensity;
        private float heartbeatPhase;
        private PartyCongroller partyController;
        private GameObject currentPlayer;
        private Health currentHealth;
        private BaseStats currentBaseStats;
        private float nextPartySearchTime;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        private void Start()
        {
            FindAndBindPartyController();
        }

        private void OnDisable()
        {
            UnbindPartyController();
            effectIntensity = 0f;
            heartbeatPhase = 0f;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }

        private void LateUpdate()
        {
            EnsurePartyController();
            RefreshCurrentPlayer();
            UpdateEffectAlpha();
        }

        private void EnsurePartyController()
        {
            if (partyController != null || Time.unscaledTime < nextPartySearchTime)
            {
                return;
            }

            nextPartySearchTime = Time.unscaledTime + 1f;
            FindAndBindPartyController();
        }

        private void FindAndBindPartyController()
        {
            PartyCongroller foundController = PartyCongroller.FindInScene();
            if (foundController == partyController)
            {
                return;
            }

            UnbindPartyController();
            partyController = foundController;
            if (partyController == null)
            {
                return;
            }

            partyController.OnChangeCharacter += SetPlayer;
            SetPlayer(partyController.GetCurrentPlayer());
        }

        private void UnbindPartyController()
        {
            if (partyController != null)
            {
                partyController.OnChangeCharacter -= SetPlayer;
            }

            partyController = null;
            SetPlayer(null);
        }

        private void RefreshCurrentPlayer()
        {
            if (partyController == null)
            {
                return;
            }

            GameObject player = partyController.GetCurrentPlayer();
            if (player != currentPlayer)
            {
                SetPlayer(player);
            }
        }

        public void SetPlayer(GameObject player)
        {
            currentPlayer = player;
            currentHealth = player != null ? player.GetComponent<Health>() : null;
            currentBaseStats = player != null ? player.GetComponent<BaseStats>() : null;
        }

        private void UpdateEffectAlpha()
        {
            float healthRate = GetHealthRate();
            bool isEffectActive = healthRate >= 0f && healthRate <= healthThreshold;
            effectIntensity = Mathf.MoveTowards(
                effectIntensity,
                isEffectActive ? 1f : 0f,
                Time.unscaledDeltaTime / fadeDuration
            );

            if (effectIntensity <= 0f)
            {
                heartbeatPhase = 0f;
                canvasGroup.alpha = 0f;
                return;
            }

            AdvanceHeartbeat(healthRate);
            float pulse = EvaluateHeartbeat();
            canvasGroup.alpha = effectIntensity * Mathf.Lerp(minimumAlpha, maximumAlpha, pulse);
        }

        // HP残量率を返す。取得できないときは -1
        private float GetHealthRate()
        {
            if (currentHealth == null || currentBaseStats == null)
            {
                return -1f;
            }

            float maximumHealth = currentBaseStats.GetStat(Stat.Health);
            if (maximumHealth <= 0f)
            {
                return -1f;
            }

            return Mathf.Clamp01(currentHealth.GetHealthPoints() / maximumHealth);
        }

        private void AdvanceHeartbeat(float healthRate)
        {
            // 残HPがしきい値から0に近づくほど鼓動を criticalPulseSpeedScale 倍まで速める
            float dangerRate =
                healthThreshold > 0f
                    ? Mathf.Clamp01(1f - Mathf.Max(healthRate, 0f) / healthThreshold)
                    : 1f;
            float period = pulsePeriod / Mathf.Lerp(1f, criticalPulseSpeedScale, dangerRate);
            heartbeatPhase = Mathf.Repeat(heartbeatPhase + Time.unscaledDeltaTime / period, 1f);
        }

        // ドクッ、ドクッと2拍打って残りは休む心拍波形 (0〜1)
        private float EvaluateHeartbeat()
        {
            float firstBeat = Mathf.Exp(-Square((heartbeatPhase - 0.1f) / 0.09f));
            float secondBeat = 0.5f * Mathf.Exp(-Square((heartbeatPhase - 0.32f) / 0.1f));
            return Mathf.Clamp01(firstBeat + secondBeat);
        }

        private static float Square(float value) => value * value;
    }
}
