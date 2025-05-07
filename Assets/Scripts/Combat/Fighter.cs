using System;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using VLCNP.Attributes;
using VLCNP.Saving;
using VLCNP.Stats;

namespace VLCNP.Combat
{
    public class Fighter : MonoBehaviour, IJsonSaveable
    {
        [SerializeField]
        WeaponConfig defaultWeaponConfig = null;

        [SerializeField]
        WeaponConfig directAttackWeaponConfig = null;

        [SerializeField]
        Transform handTransform = null;

        [SerializeField]
        bool canVerticalShot = true;
        public bool CanVerticalShot
        {
            get => canVerticalShot;
        }

        [SerializeField]
        public UnityEvent onDirectAttack;

        WeaponConfig currentWeaponConfig;
        BaseStats baseStats;
        public Vector3 positionWhenHorizontal = new(-0.903f, -0.365f, 0f);
        public Vector3 positionWhenUp = new(-0.7f, 0.1f, 0f);
        public Vector3 positionWhenDown = new(-0.7f, -0.515f, 0f);

        protected void Awake()
        {
            EquipWeapon(defaultWeaponConfig);
            baseStats = GetComponent<BaseStats>();
        }

        public void WeaponUp()
        {
            if (!canVerticalShot)
                return;
            handTransform.rotation = GetIsLeft()
                ? Quaternion.Euler(0, 0, -90)
                : Quaternion.Euler(0, 0, 90);
            // 回転の軸を少し上にずらす
            handTransform.localPosition = positionWhenUp;
        }

        public void WeaponDown()
        {
            if (!canVerticalShot)
                return;
            handTransform.rotation = GetIsLeft()
                ? Quaternion.Euler(0, 0, 90)
                : Quaternion.Euler(0, 0, -90);
            // 回転の軸を少し下にずらす
            handTransform.localPosition = positionWhenDown;
        }

        public void WeaponHorizontal()
        {
            handTransform.rotation = Quaternion.Euler(0, 0, 0);
            handTransform.localPosition = positionWhenHorizontal;
        }

        public void EquipWeapon(WeaponConfig weaponConfig)
        {
            if (weaponConfig == null)
                return;
            currentWeaponConfig = weaponConfig;
            currentWeaponConfig.Spawn(handTransform);
            WeaponHorizontal();
        }

        public void Attack()
        {
            int level = baseStats ? baseStats.GetLevel() : 1;
            if (!currentWeaponConfig.HasProjectile())
                return;
            currentWeaponConfig.LaunchProjectile(handTransform, level, GetIsLeft());
        }

        public void DirectAttack(GameObject target = null)
        {
            if (directAttackWeaponConfig == null)
                return;
            int level = baseStats ? baseStats.GetLevel() : 1;
            target
                .GetComponent<Health>()
                .TakeDamage(directAttackWeaponConfig.GetDamage(level), transform.lossyScale.x < 0);
            onDirectAttack?.Invoke();
        }

        private bool GetIsLeft()
        {
            // 左向きキャラクターをlocalScaleで反転させているため、右を向いているときscaleが-1になる
            return transform.lossyScale.x > 0;
        }

        public JToken CaptureAsJToken()
        {
            return JToken.FromObject(currentWeaponConfig.name);
        }

        public void RestoreFromJToken(JToken state)
        {
            WeaponConfig weaponConfig = Resources.Load<WeaponConfig>(state.ToObject<string>());
            if (weaponConfig == null)
                return;
            EquipWeapon(weaponConfig);
        }
    }
}
