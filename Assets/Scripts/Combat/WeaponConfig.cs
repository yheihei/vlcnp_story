using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace VLCNP.Combat
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/Make New Weapon", order = 0)]
    public class WeaponConfig : ScriptableObject
    {
        [SerializeField]
        Weapon equippedPrefab = null;

        [SerializeField]
        WeaponLevel[] weaponLevels = null;

        [System.Serializable]
        class WeaponLevel
        {
            [SerializeField]
            public float damage;

            [SerializeField]
            public GameObject projectilePrefab; // IProjectileを実装したGameObject

            [SerializeField]  // 射出パワー
            public float launchPower = 0f;

            // 1回の攻撃で扇状に同時に出す弾の数。
            // IProjectileVolleyReceiver を実装した弾なら、同じ相手へのダメージは1回にまとまる
            [SerializeField]
            public int projectileCount = 1;

            // 扇状に並べる弾どうしの角度(度)。
            // 1発目は1発だけ撃つときと同じ向きに出し、2発目以降を上側へ傾ける(マイナスなら下側)
            [SerializeField]
            public float spreadAngle = 0f;

            // 扇の要を手からどれだけ後ろに置くか。
            // 要から放射状に並べるので、大きいほど撃った瞬間から弾が離れて見える
            [SerializeField]
            public float spreadOriginDistance = 0f;

            [SerializeField]  // 発射位置に出す演出
            public GameObject launchEffect = null;
        }

        const string weaponName = "Weapon";
        const float launchEffectLifetime = 1f;

        public Weapon Spawn(Transform handTransform)
        {
            // DestroyOldWeapon(rightHand, leftHand);
            Weapon weapon = null;

            if (equippedPrefab != null)
            {
                weapon = Instantiate(equippedPrefab, handTransform);
                weapon.gameObject.name = weaponName;
            }
            return weapon;
        }

        public bool HasProjectile(int level = 1)
        {
            WeaponLevel _weaponLevel = GetCurrentWeapon(level);
            if (_weaponLevel == null)
                return false;
            return _weaponLevel.projectilePrefab != null;
        }

        public bool CanLaunchProjectile(Transform handTransform, int level = 1)
        {
            WeaponLevel _weaponLevel = GetCurrentWeapon(level);
            if (_weaponLevel == null || _weaponLevel.projectilePrefab == null)
                return false;

            IProjectileLaunchGate launchGate =
                _weaponLevel.projectilePrefab.GetComponent<IProjectileLaunchGate>();
            if (launchGate == null)
                return true;

            GameObject projectileOwner = GetProjectileOwner(handTransform);
            return launchGate.CanLaunch(projectileOwner);
        }

        public void LaunchProjectile(Transform handTransform, int level = 1, bool isLeft = false)
        {
            WeaponLevel _weaponLevel = GetCurrentWeapon(level);
            int projectileCount = Mathf.Max(1, _weaponLevel.projectileCount);
            // 同時に出した弾どうしで、同じ相手に何度もダメージを与えないようにする
            ProjectileVolley volley = projectileCount > 1 ? new ProjectileVolley() : null;
            // 弾は回転の +x へ進み、左向きのときは -x へ進む
            float directionSign = isLeft ? -1 : 1;
            Vector3 spreadOrigin =
                handTransform.position
                - handTransform.rotation * Vector3.right * directionSign * _weaponLevel.spreadOriginDistance;
            for (int i = 0; i < projectileCount; i++)
            {
                // 1発目を手の向きにそろえ、残りを上側へ spreadAngle ずつ広げる。
                // 左向きは回転の向きが逆になるので、符号をそろえて同じ上側へ広げる
                Quaternion rotation =
                    handTransform.rotation
                    * Quaternion.Euler(0, 0, directionSign * i * _weaponLevel.spreadAngle);
                Vector3 position =
                    spreadOrigin
                    + rotation * Vector3.right * directionSign * _weaponLevel.spreadOriginDistance;
                GameObject projectileObj = SpawnProjectile(
                    _weaponLevel,
                    handTransform,
                    position,
                    rotation,
                    level,
                    isLeft,
                    volley
                );
                // 1発目ほど手前に描く。どの弾も Prefab の並び順より奥には回さない
                int front = projectileCount - 1 - i;
                if (front > 0)
                {
                    foreach (SpriteRenderer sprite in projectileObj.GetComponentsInChildren<SpriteRenderer>())
                        sprite.sortingOrder += front;
                }
            }

            // 音声処理
            AudioSource projectileAudioSource =
                _weaponLevel.projectilePrefab.GetComponent<AudioSource>();
            if (projectileAudioSource != null && projectileAudioSource.clip != null)
            {
                AudioSource.PlayClipAtPoint(projectileAudioSource.clip, handTransform.position);
            }

            SpawnLaunchEffect(_weaponLevel, handTransform, isLeft);
        }

        GameObject SpawnProjectile(
            WeaponLevel weaponLevel,
            Transform handTransform,
            Vector3 position,
            Quaternion rotation,
            int level,
            bool isLeft,
            ProjectileVolley volley
        )
        {
            GameObject projectileObj = Instantiate(weaponLevel.projectilePrefab, position, rotation);
            // 射出パワーがある場合、生成した射出物にRigidBodyで力を加える
            if (weaponLevel.launchPower > 0)
            {
                Rigidbody2D rb = projectileObj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // 弾の向きに力を加える
                    Vector2 direction = rotation * Vector3.right;
                    rb.AddForce(new Vector2(weaponLevel.launchPower * direction.x, weaponLevel.launchPower * direction.y), ForceMode2D.Impulse);
                }
            }

            // IProjectileインターフェースを通じて操作
            IProjectile projectile = projectileObj.GetComponent<IProjectile>();
            if (projectile != null)
            {
                IProjectileOwnerReceiver ownerReceiver =
                    projectileObj.GetComponent<IProjectileOwnerReceiver>();
                GameObject projectileOwner = GetProjectileOwner(handTransform);
                ownerReceiver?.SetOwner(projectileOwner);

                IProjectileLevelReceiver levelReceiver =
                    projectileObj.GetComponent<IProjectileLevelReceiver>();
                levelReceiver?.SetLevel(level);

                IProjectileVolleyReceiver volleyReceiver =
                    projectileObj.GetComponent<IProjectileVolleyReceiver>();
                volleyReceiver?.SetVolley(volley);

                projectile.SetDirection(isLeft);
                projectile.SetDamage(weaponLevel.damage);
            }
            return projectileObj;
        }

        void SpawnLaunchEffect(WeaponLevel weaponLevel, Transform handTransform, bool isLeft)
        {
            if (weaponLevel.launchEffect == null)
                return;
            GameObject effect = Instantiate(
                weaponLevel.launchEffect,
                handTransform.position,
                handTransform.rotation
            );
            // 弾と同じく、左向きのときは左右を反転して撃つ向きにそろえる
            if (isLeft)
            {
                Vector3 scale = effect.transform.localScale;
                effect.transform.localScale = new Vector3(-scale.x, scale.y, scale.z);
            }
            Destroy(effect, launchEffectLifetime);
        }

        GameObject GetProjectileOwner(Transform handTransform)
        {
            Fighter ownerFighter = handTransform.GetComponentInParent<Fighter>();
            return ownerFighter != null ? ownerFighter.gameObject : handTransform.root.gameObject;
        }

        private WeaponLevel GetCurrentWeapon(int level = 1)
        {
            if (weaponLevels.Length == 0)
                return null;
            // 武器のMaxレベル以上にはならない
            return weaponLevels[Math.Min(level, weaponLevels.Length) - 1];
        }

        public float GetDamage(int level = 1)
        {
            WeaponLevel _weaponLevel = GetCurrentWeapon(level);
            if (_weaponLevel == null)
                return 0;
            return _weaponLevel.damage;
        }
    }
}
